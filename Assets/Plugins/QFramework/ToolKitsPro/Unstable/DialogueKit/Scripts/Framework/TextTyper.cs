using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.Pro
{
    public class TextTyper
    {
        public float TypeSpeedFast = 0.01f;
        public float TypeSpeedMedium = 0.05f;
        public float TypeSpeedSlow = 0.1f;

        public enum TypeSpeed
        {
            Fast,
            Medium,
            Slow,
        }

        private Coroutine mTypeTextCoroutine;

        public bool IsTyping = false;

        public TypeSpeed Speed;

        public Action OnTypeText;

        private string mCurrentSentence;

        private IEnumerator TypeText(string sentence, Text text,Action onFinish)
        {
            IsTyping = true;
            text.text = "";
            mCurrentSentence = sentence;
            float typeSpeedDelay = TypeSpeedFast;
            switch (Speed)
            {
                case TypeSpeed.Fast:
                    typeSpeedDelay = TypeSpeedFast;
                    break;
                case TypeSpeed.Medium:
                    typeSpeedDelay = TypeSpeedMedium;
                    break;
                case TypeSpeed.Slow:
                    typeSpeedDelay = TypeSpeedSlow;
                    break;
            }

            foreach (char letter in sentence.ToCharArray())
            {
                text.text += letter;
                OnTypeText?.Invoke();
                yield return new WaitForSeconds(typeSpeedDelay);
            }

            IsTyping = false;
            mTypeTextCoroutine = null;
            onFinish();
        }

        public void StartTyping(string sentence, Text text,Action onFinish)
        {
            mTypeTextCoroutine = text.StartCoroutine(TypeText(sentence, text,onFinish));
        }

        public void FinishTyping(Text text)
        {
            IsTyping = true;

            if (mTypeTextCoroutine != null)
            {
                text.StopCoroutine(mTypeTextCoroutine);
                mTypeTextCoroutine = null;
            }

            text.text = mCurrentSentence;
        }
    }
}