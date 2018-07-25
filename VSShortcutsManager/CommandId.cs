using System;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("({Guid},{Id}")]
    internal struct CommandId
    {
        internal CommandId(Guid guid, int id)
        {
            this.Guid = guid;
            this.Id = id;
        }

        internal Guid Guid { get; private set; }

        internal int Id { get; private set; }
    }
}
