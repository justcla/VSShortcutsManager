using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VSShortcutsManager
{
    public class CommandBinding
    {
        public CommandId Command { get; private set;}

        public KeybindingScope Scope { get; private set; }

        public IReadOnlyList<BindingSequence> Sequences { get; private set; }

        public string DteBindingString { get; private set; }

        public CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence, string dteBindingString)
        {
            this.Command = command;
            this.Scope = scope;

            this.Sequences = new List<BindingSequence>() { sequence };

            this.DteBindingString = dteBindingString;
        }

        public CommandBinding(CommandId command, KeybindingScope scope, BindingSequence sequence1, BindingSequence sequence2, string dteBindingString) : this(command, scope, sequence1, dteBindingString)
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
