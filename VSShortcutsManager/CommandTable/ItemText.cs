//using CommandTable;
namespace CommandTable
{
    public class ItemText
    {
        public ItemText()
        {
        }

        public ItemText(string buttonText, string commandWellText)
        {
            ButtonText = buttonText;
            CommandWellText = commandWellText;
        }

        public string ButtonText { get; internal set; }
        public string CommandWellText { get; internal set; }
    }
}