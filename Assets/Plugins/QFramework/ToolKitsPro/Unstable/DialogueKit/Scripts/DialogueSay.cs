using UnityEngine;

namespace QFramework.Pro
{
    public class DialogueSay : DialogueKit.IDialogueNode
    {
        public string Name;
        public Sprite Portrait;
        public string Text;
        public bool Paused { get; set; }
        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;
            }
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
        }

        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }
        public void OnStart()
        {
            DialogueKit.PlaySayEvent.Trigger(Name, Portrait, Text);

            DialogueKit.OnNextInput.Register(OnNextInput);
        }

        private void OnNextInput()
        {
            this.Finish();
        }

        public void OnExecute(float dt)
        {
        }

        public void OnFinish()
        {
            DialogueKit.OnNextInput.UnRegister(OnNextInput);
        }

        public DialogueKit.Dialogue Dialogue { get; set; }
    }
}