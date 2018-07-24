using System;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("{Name}")]
    public class KeybindingScope
    {
        internal KeybindingScope(string name, Guid guid, bool allowNavKeyBinding)
        {
            this.Name = name;
            this.Guid = guid;
            this.AllowNavKeyBinding = allowNavKeyBinding;
        }

        internal string Name { get; private set; }

        internal Guid Guid { get; private set; }

        internal bool AllowNavKeyBinding { get; private set; }
    }

}
