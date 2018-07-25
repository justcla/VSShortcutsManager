using System;

namespace VSShortcutsManager
{
    public sealed class CommandId
    {
        public CommandId(Guid guid, int id)
        {
            this.Guid = guid;
            this.Id = id;
        }

        public Guid Guid { get; private set; }

        public int Id { get; private set; }
    }
}
