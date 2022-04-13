using UnityEngine;

namespace QFramework.Pro
{
    [AddComponentMenu("QFramework Pro/Dialogue Kit/Player/Console Text Player")]
    public class ConsoleTextPlayer : MonoBehaviour
    {
        private void Awake()
        {
            DialogueKit.PlayStart.Register(() => Debug.Log("对话开始"));
            DialogueKit.PlayTextEvent.Register(Debug.Log).UnRegisterWhenGameObjectDestroyed(gameObject);
            DialogueKit.PlayFinish.Register(() => Debug.Log("对话完成"));
        }
        
    }
}