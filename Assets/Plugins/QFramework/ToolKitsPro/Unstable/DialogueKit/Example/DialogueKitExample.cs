using UnityEngine;

namespace QFramework.Pro.Example
{
    public class DialogueKitExample : MonoBehaviour
    {
        public Sprite LiangxiePortrait;
        
        private void Start()
        {
            DialogueKit.Builder()
                .Id("介绍")
                .Say("凉鞋",LiangxiePortrait,"你好呀")
                .Say("凉鞋",LiangxiePortrait,"我是凉鞋")
                .Say("凉鞋",LiangxiePortrait,"欢迎使用 DialogueKit")
                .Options(o =>
                {
                    o.Option("再来一遍介绍", "介绍")
                        .Option("继续", "继续");
                })
                .Id("继续")
                .Say("凉鞋",LiangxiePortrait,"就说到这里吧")
                .NextCondition(() => Input.GetMouseButtonDown(0))
                .Build()
                .Start(this);
            
        }
    }
}