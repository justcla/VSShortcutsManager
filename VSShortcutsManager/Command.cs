﻿using System.Collections.Generic;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("{DisplayName} ({CanonicalName})")]
    public class Command
    {
        public Command(CommandId id, string displayName,  string canonicalName, IEnumerable<CommandBinding> bindings)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.CanonicalName = canonicalName;
            this.Bindings = bindings;
        }

        public CommandId Id { get; private set; }

        public string DisplayName { get; private set; }

        public string CanonicalName { get; private set;}

        public IEnumerable<CommandBinding> Bindings { get; private set; }
    }
}
