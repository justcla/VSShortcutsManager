using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VSShortcutsManager
{
    internal class CommandBinding
    {
        internal CommandId Command { get; private set;}

        internal KeybindingScope Scope { get; private set; }

        internal IReadOnlyList<BindingSequence> Sequences { get; private set; }

        internal CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence)
        {
            this.Command = command;
            this.Scope = scope;

            this.Sequences = new List<BindingSequence>() { sequence };
        }

        internal CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence1, BindingSequence sequence2) : this(command, scope, sequence1)
        {
            ((List<BindingSequence>)this.Sequences).Add(sequence2);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}::", Scope.Name);
            bool isFirst = true;

            foreach(BindingSequence bs in Sequences)
            {
                if(isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append(bs.ToString());
            }

            return sb.ToString();
        }
    }
}
