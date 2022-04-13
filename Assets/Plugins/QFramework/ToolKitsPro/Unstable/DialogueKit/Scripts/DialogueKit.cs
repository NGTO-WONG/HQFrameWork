/****************************************************************************
 * Copyright (c) 2022.3 liangxiegame UNDER MIT LICENSE
 * 
 * http://qframework.io
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QFramework.Pro
{
    public class DialogueKit : Architecture<DialogueKit>
    {
        #region To Outside

        public static EasyEvent PlayStart = new EasyEvent();
        public static EasyEvent<string, Sprite, string> PlaySayEvent = new EasyEvent<string, Sprite, string>();
        public static EasyEvent<string> PlayTextEvent = new EasyEvent<string>();
        public static EasyEvent<string[]> PlayOptionsEvent = new EasyEvent<string[]>();
        public static EasyEvent PlayFinish = new EasyEvent();

        #endregion


        #region From Outside

        public static EasyEvent<int> OnSelectOptionInput = new EasyEvent<int>();
        public static EasyEvent OnNextInput = new EasyEvent();

        #endregion


        public static void SelectOption(int optionIndex)
        {
            OnSelectOptionInput.Trigger(optionIndex);
        }

        public static void Next()
        {
            OnNextInput.Trigger();
        }


        public static DialogueBuilder Builder()
        {
            return new DialogueBuilder();
        }


        public interface IDialogueNode : IAction
        {
            Dialogue Dialogue { get; set; }
        }

        public class DialogueId : IDialogueNode
        {
            public string Id;
            public bool Deinited { get; set; }
            public ActionStatus Status { get; set; }

            public void OnStart()
            {
                this.Finish();
            }

            public void OnExecute(float dt)
            {
            }

            public void OnFinish()
            {
            }

            public void OnEnd()
            {
            }

            public Dialogue Dialogue { get; set; }

            public void Reset()
            {
                Status = ActionStatus.NotStart;
            }

            public bool Paused { get; set; }

            public void Deinit()
            {
            }
        }

        public class DialogueOptions : IDialogueNode
        {
            public class Option
            {
                public string MenuTitle;
                public string ToId;
            }

            public List<Option> Options = new List<Option>();

            public Dialogue Dialogue { get; set; }
            public ActionStatus Status { get; set; }

            public void OnStart()
            {
                PlayOptionsEvent.Trigger(Options.Select(o => o.MenuTitle).ToArray());

                OnSelectOptionInput.Register(OnOptionSelect);
            }

            private void OnOptionSelect(int index)
            {
                Dialogue.SwitchToId(Options[index].ToId);
            }

            public void OnExecute(float dt)
            {
            }

            public void OnFinish()
            {
                OnSelectOptionInput.UnRegister(OnOptionSelect);
            }

            public void Reset()
            {
                Status = ActionStatus.NotStart;
            }

            public bool Paused { get; set; }
            public bool Deinited { get; set; }

            public void Deinit()
            {
                if (!Deinited)
                {
                    Deinited = true;

                    Options.Clear();
                }
            }
        }

        public class DialogueText : IDialogueNode
        {
            public string Text;

            public ActionStatus Status { get; set; }

            public void OnStart()
            {
                PlayTextEvent.Trigger(Text);
            }

            public void OnExecute(float dt)
            {
                if (Dialogue.NextCondition())
                {
                    this.Finish();
                }
            }

            public void OnFinish()
            {
            }

            public Dialogue Dialogue { get; set; }

            public void Reset()
            {
                Status = ActionStatus.NotStart;
            }

            public bool Paused { get; set; }
            public bool Deinited { get; set; }

            public void Deinit()
            {
                if (!Deinited)
                {
                    Deinited = true;
                    Text = string.Empty;
                }
            }
        }

        public class Dialogue : IAction
        {
            public List<IDialogueNode> Nodes = new List<IDialogueNode>();

            public int Index = 0;
            public IDialogueNode CurrentNode;

            public ActionStatus Status { get; set; }
            public Dictionary<string, int> IndexForId = new Dictionary<string, int>();


            public void OnStart()
            {
                PlayStart.Trigger();

                if (Nodes.Count == 0)
                {
                    Status = ActionStatus.Finished;
                }
                else
                {
                    CurrentNode = Nodes.First();
                    CurrentNode.Reset();
                    CurrentNode.Dialogue = this;
                }
            }

            public void OnExecute(float dt)
            {
                if (CurrentNode != null)
                {
                    if (CurrentNode.Execute(dt))
                    {
                        Index++;

                        if (Index < Nodes.Count)
                        {
                            CurrentNode = Nodes[Index];
                            CurrentNode.Reset();
                            CurrentNode.Dialogue = this;
                        }
                        else
                        {
                            Status = ActionStatus.Finished;
                            CurrentNode.Dialogue = null;
                        }
                    }
                }
                else
                {
                    Status = ActionStatus.Finished;
                }
            }

            public void OnFinish()
            {
                PlayFinish.Trigger();
            }

            public void Reset()
            {
                Status = ActionStatus.NotStart;
                foreach (var dialogueNode in Nodes)
                {
                    dialogueNode.Reset();
                }
            }

            public bool Paused { get; set; }
            public bool Deinited { get; set; }

            public void Deinit()
            {
                if (!Deinited)
                {
                    Deinited = true;
                    foreach (var dialogueNode in Nodes)
                    {
                        dialogueNode.Reset();
                    }

                    Nodes.Clear();
                    IndexForId.Clear();
                }
            }

            public Func<bool> NextCondition = () => Input.GetMouseButtonDown(0);

            public void SwitchToId(string toId)
            {
                Index = IndexForId[toId];
                CurrentNode = Nodes[Index];
                CurrentNode.Reset();
            }
        }


        public class DialogueBuilder
        {
            private Dialogue mDialogue = new Dialogue();


            public DialogueBuilder Text(string text)
            {
                var textNode = new DialogueText
                {
                    Text = text
                };
                mDialogue.Nodes.Add(textNode);
                return this;
            }


            public Dialogue Build()
            {
                return mDialogue;
            }


            public DialogueBuilder Options(Action<DialogueOptions> optionsSetting)
            {
                var options = new DialogueOptions();
                optionsSetting(options);
                mDialogue.Nodes.Add(options);
                return this;
            }

            public DialogueBuilder NextCondition(Func<bool> func)
            {
                mDialogue.NextCondition = func;
                return this;
            }

            public DialogueBuilder Id(string id)
            {
                var dialogueId = new DialogueId()
                {
                    Id = id
                };
                var index = mDialogue.Nodes.Count;
                mDialogue.IndexForId.Add(id, index);
                mDialogue.Nodes.Add(dialogueId);
                return this;
            }

            public DialogueBuilder Node(IDialogueNode node)
            {
                mDialogue.Nodes.Add(node);
                return this;
            }


            public DialogueBuilder Say(string name, Sprite portrait, string text)
            {
                mDialogue.Nodes.Add(new DialogueSay()
                {
                    Name = name,
                    Portrait = portrait,
                    Text = text
                });
                return this;
            }
        }

        protected override void Init()
        {
        }
    }
}