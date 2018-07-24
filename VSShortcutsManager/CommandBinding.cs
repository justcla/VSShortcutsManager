using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("Scope = {Scope.Name}")]
    internal class CommandBinding
    {
        internal CommandId Command { get; private set;}

        internal KeybindingScope Scope { get; private set; }

        internal IEnumerable<BindingSequence> Chords { get; private set; }

        internal CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence)
        {
            this.Command = command;
            this.Scope = scope;

            this.Chords = new List<BindingSequence>() { sequence };
        }

        internal CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence1, BindingSequence sequence2) : this(command, scope, sequence1)
        {
            ((List<BindingSequence>)this.Chords).Add(sequence2);
        }
    }
}
