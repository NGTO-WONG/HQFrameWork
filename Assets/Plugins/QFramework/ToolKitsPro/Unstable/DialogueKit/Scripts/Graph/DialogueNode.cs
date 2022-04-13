using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QFramework.Pro
{
    [CreateNodeMenu("DialogueNode")]
    public class DialogueNode : IMGUIGraphNode, DialogueKit.IDialogueNode, IUnRegisterList
    {
        [Input] public DialogueConnection Parent;

        [TextArea(3, int.MaxValue)] public string Text;

        [Output] public DialogueConnection Children;
        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        private IMGUIGraphNodePort mOutputPort;
        private DialogueNode[] mChildNodes;
        private DialogueNode mNextNode;

        public void OnStart()
        {
            mOutputPort = GetOutputPort("Children");

            if (mOutputPort.ConnectionCount == 1 || mOutputPort.ConnectionCount == 0)
            {
                DialogueKit.PlayTextEvent.Trigger(Text);
                DialogueKit.OnNextInput.Register(()=>
                {
                    Debug.Log("OnNextInput:" + mOutputPort.ConnectionCount);
                    this.Finish();
                }).AddToUnregisterList(this);
            }
            else
            {
                mChildNodes = mOutputPort.GetConnections().Select(c => c.node.As<DialogueNode>()).ToArray();
                var options = mChildNodes.Select(n => n.Text).ToArray();
                DialogueKit.PlayOptionsEvent.Trigger(options);
                DialogueKit.OnSelectOptionInput.Register(index =>
                {
                    var nextNode = mChildNodes[mChildNodes.Length - 1 - index];

                    var outputPort = nextNode.GetOutputPort("Children");

                    if (outputPort.ConnectionCount == 0)
                    {
                        mNextNode = null;
                    } else if (outputPort.ConnectionCount == 1)
                    {
                        mNextNode = outputPort.Connection.node.As<DialogueNode>();
                    }
                    else
                    {
                        mNextNode = nextNode;
                    }

                    this.Finish();
                }).AddToUnregisterList(this);
            }
        }

        public DialogueNode GetNext()
        {
            if (mOutputPort.ConnectionCount == 0)
            {
                return null;
            }
            else if (mOutputPort.ConnectionCount == 1)
            {
                return mOutputPort.Connection.node.As<DialogueNode>();
            }
            else
            {
                return mNextNode;
            }
        }

        public void OnExecute(float dt)
        {
        }
        

        public void OnFinish()
        {
            this.UnRegisterAll();
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
        }

        public bool Paused { get; set; }
        public void Deinit()
        {
            
        }

        public DialogueKit.Dialogue Dialogue { get; set; }
        public List<IUnRegister> UnregisterList { get; } = new List<IUnRegister>();
    }

    [System.Serializable]
    public class DialogueConnection
    {
    }
}