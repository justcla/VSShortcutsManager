using System;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("({Guid},{Id}")]
    public struct CommandId
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
