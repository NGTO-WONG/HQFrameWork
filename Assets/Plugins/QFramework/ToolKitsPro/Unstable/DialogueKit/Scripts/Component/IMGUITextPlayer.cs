using UnityEngine;

namespace QFramework.Pro
{
    /// <summary>
    /// 用来测试用的
    /// </summary>
    public class IMGUITextPlayer : MonoBehaviour
    {
        private string Current = string.Empty;

        private string[] Options = null;

        private void Awake()
        {
            DialogueKit.PlayTextEvent.Register(text =>
                {
                    Current = text;
                    Options = null;
                })
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            DialogueKit.PlayOptionsEvent.Register(options =>
            {
                Current = null;
                Options = options;
            });

            DialogueKit.PlayFinish.Register(() =>
            {
                Current = string.Empty;
                Options = null;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnGUI()
        {
            IMGUIHelper.SetDesignResolution(512, 384);

            if (Options != null)
            {
                foreach (var option in Options)
                {
                    GUILayout.Label(option);
                }
            }
            else if (Current != null)
            {
                GUILayout.Label(Current);
            }
        }
    }
}