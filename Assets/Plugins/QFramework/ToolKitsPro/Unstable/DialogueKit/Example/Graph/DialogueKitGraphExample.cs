using UnityEngine;

namespace QFramework.Pro.Example
{
    public class DialogueKitGraphExample : MonoBehaviour
    {
        public DialogueGraph Graph;

        private void Start()
        {
            DialogueKit.Builder()
                .Node(Graph)
                .Build()
                .Start(this);
        }
    }
}