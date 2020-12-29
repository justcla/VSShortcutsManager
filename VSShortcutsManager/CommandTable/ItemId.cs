//using CommandTable;
using System;

namespace CommandTable
{
    public class ItemId
    {
        public ItemId(Guid guid, int dWord)
        {
            Guid = guid;
            DWord = dWord;
        }

        public Guid Guid { get; internal set; }
        public int DWord { get; internal set; }
    }
}