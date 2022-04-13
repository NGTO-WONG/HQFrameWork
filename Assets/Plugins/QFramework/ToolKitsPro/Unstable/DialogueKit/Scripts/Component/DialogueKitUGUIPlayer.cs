using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace QFramework.Pro
{
    public class DialogueKitUGUIPlayer : MonoBehaviour
    {
        private TextTyper mTextTyper;
        
        public Text Name;
        public Text Text;
        public Button NextButton;
        public GameObject OptionRoot;
        public Button OptionButtonPrefab;
        public Image Portrait;

        public UnityEvent OnTypeText;
        public UnityEvent OnOptionSelect;
        
        private void Awake()
        {
            DialogueKit.PlayTextEvent.Register(t =>
            {
                Text.Show();
                Text.text = string.Empty;
                mTypeTextCoroutine = StartCoroutine(TypeText(t));
                OptionRoot.Hide();
                Portrait.Hide();
                Name.Hide();

            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            DialogueKit.PlaySayEvent.Register((n, p, t) =>
            {
                Name.text = n;
                Portrait.sprite = p;
                Text.text = string.Empty;

                Name.Show();
                Portrait.Show();
                Text.Show();
                mTypeTextCoroutine = StartCoroutine(TypeText(t));
                OptionRoot.Hide();

            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            DialogueKit.PlayOptionsEvent.Register(options =>
            {
                OptionButtonPrefab.Parent(OptionRoot.transform.parent);
                
                OptionRoot.DestroyChildren();
                OptionRoot.Show();
                Text.Hide();

                var optionIndex = 0;
                foreach (var option in options)
                {
                    var index = optionIndex;
                    OptionButtonPrefab.gameObject.InstantiateWithParent(OptionRoot.transform)
                        .Self(button =>
                        {
                            button.GetComponentInChildren<Text>().text = option;
                            button.GetComponent<Button>().onClick.AddListener(() =>
                            {
                                OnOptionSelect?.Invoke();
                                DialogueKit.SelectOption(index);
                            });
                        })
                        .Show();
                    optionIndex++;
                }
                
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            DialogueKit.PlayFinish.Register(() =>
            {
                Text.Hide();
                OptionRoot.Hide();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            NextButton.onClick.AddListener(() =>
            {
                if (IsTyping)
                {
                    FinishTyping();
                }
                else
                {
                    DialogueKit.Next();
                }
            });
        }

        private Coroutine mTypeTextCoroutine;

        public bool IsTyping = false;

        private string mCurrentSentence;
        
        private IEnumerator TypeText(string sentence) {
            IsTyping = true;
            Text.text = "";
            mCurrentSentence = sentence;
            float typeSpeedDelay = typeSpeedFast;
            switch(Speed) {
                case TypeSpeed.Fast:
                    typeSpeedDelay = typeSpeedFast;
                    break;
                case TypeSpeed.Medium:
                    typeSpeedDelay = typeSpeedMedium;
                    break;
                case TypeSpeed.Slow:
                    typeSpeedDelay = typeSpeedSlow;
                    break;
            }

            foreach(char letter in sentence.ToCharArray()) {
                Text.text += letter;
                OnTypeText?.Invoke();
                yield return new WaitForSeconds(typeSpeedDelay);
            }

            IsTyping = false;
            mTypeTextCoroutine = null;
        }

        void FinishTyping()
        {
            IsTyping = true;
            
            if (mTypeTextCoroutine != null)
            {
                StopCoroutine(mTypeTextCoroutine);
                mTypeTextCoroutine = null;
            }

            Text.text = mCurrentSentence;
        }

        public float typeSpeedFast = 0.01f;
        public float typeSpeedMedium = 0.05f;
        public float typeSpeedSlow = 0.1f;
        
        public TypeSpeed Speed;
        public enum TypeSpeed
        {
            Fast,
            Medium,
            Slow,
        }
    }
}