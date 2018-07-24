using System.Collections.Generic;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("{Text}")]
    internal class Command
    {
        internal Command(CommandId id, string text, IEnumerable<CommandBinding> bindings)
        {
            this.Id = id;
            this.Text = text;
            this.Bindings = bindings;
        }

        internal CommandId Id { get; private set; }

        internal string Text { get; private set; }

        internal IEnumerable<CommandBinding> Bindings { get; private set; }
    }
}
