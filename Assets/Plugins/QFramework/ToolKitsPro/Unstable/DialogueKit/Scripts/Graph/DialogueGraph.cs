#if UNITY_EDITOR
using QFramework.XNodeEditor;
#endif
using System;
using UnityEngine;

namespace QFramework.Pro
{
    [CreateAssetMenu(menuName = "@DialogueKit/Create Dialogue Graph-创建对话图", order = int.MinValue + 10)]
    public class DialogueGraph : IMGUIGraph, DialogueKit.IDialogueNode
    {
        private void OnValidate()
        {
            if (nodes.Count == 0)
            {
                AddNode<DialogueNode>();
            }
        }

        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        public DialogueNode CurrentNode;


        public void OnStart()
        {
            CurrentNode = this.nodes[0].As<DialogueNode>();
            CurrentNode.Reset();
        }

        public void OnExecute(float dt)
        {
            if (CurrentNode.Execute(dt))
            {
                Debug.Log("Finished");
                CurrentNode = CurrentNode.GetNext();
                if (CurrentNode == null)
                {
                    this.Finish();
                }
                else
                {
                    CurrentNode.Reset();
                }
            }
        }

        public void OnFinish()
        {
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;

            foreach (var node in this.nodes)
            {
                node.As<DialogueKit.IDialogueNode>().Reset();
            }
        }

        public bool Paused { get; set; }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;

                nodes.As<DialogueKit.IDialogueNode>().Deinit();
            }
        }

        public DialogueKit.Dialogue Dialogue { get; set; }
    }

#if UNITY_EDITOR

    [CustomNodeGraphEditor(typeof(DialogueGraph))]
    public class DialogueGraphEditor : IMGUIGraphEditor
    {
        public override void OnCreate()
        {
            base.OnCreate();

            window.titleContent.text = "Dialogue Graph";
        }


        public override Color GetTypeColor(Type type)
        {
            return Color.white;
        }

        public override Color GetPortColor(IMGUIGraphNodePort port)
        {
            return Color.white;
        }
    }
#endif
}