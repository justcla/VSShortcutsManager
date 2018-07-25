using System.Collections.Generic;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("{DisplayName} ({CanonicalName})")]
    internal class Command
    {
        internal Command(CommandId id, string displayName,  string canonicalName, IEnumerable<CommandBinding> bindings)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.CanonicalName = canonicalName;
            this.Bindings = bindings;
        }

        internal CommandId Id { get; private set; }

        internal string DisplayName { get; private set; }

        internal string CanonicalName { get; private set;}

        internal IEnumerable<CommandBinding> Bindings { get; private set; }
    }
}
