using System;
using UnityEngine;

namespace QFramework.Pro
{
    public class MouseNextInput : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                DialogueKit.Next();
            }

            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    DialogueKit.SelectOption(i);
                }
            }
        }
    }
}