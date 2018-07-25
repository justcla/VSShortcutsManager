using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VSShortcutsManager
{
    [DebuggerDisplay("Scope = {Scope.Name}")]
    public class CommandBinding
    {
        public CommandId Command { get; private set;}

        public KeybindingScope Scope { get; private set; }

        public IEnumerable<BindingSequence> Chords { get; private set; }

        public CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence)
        {
            this.Command = command;
            this.Scope = scope;

            this.Chords = new List<BindingSequence>() { sequence };
        }

        public CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence1, BindingSequence sequence2) : this(command, scope, sequence1)
        {
            ((List<BindingSequence>)this.Chords).Add(sequence2);
        }
    }
}
