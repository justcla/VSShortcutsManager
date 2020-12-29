namespace CommandTable
{
    public class Command
    {
        public Command(ItemText itemText, ItemId itemId)
        {
            ItemText = itemText;
            ItemId = itemId;
        }

        public ItemText ItemText { get; internal set; }
        public ItemId ItemId { get; internal set; }
    }

}