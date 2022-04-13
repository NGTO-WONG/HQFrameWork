using System;

namespace QFramework.Pro
{
    public static class DialogueTextsExtension
    {
        public static DialogueKit.DialogueOptions Option(this DialogueKit.DialogueOptions self, string title, string id)
        {
            self.Options.Add(new DialogueKit.DialogueOptions.Option()
            {
                MenuTitle = title,
                ToId = id,
            });

            return self;
        }
    }
}