using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VSShortcutsManager
{
    [DebuggerDisplay("Scope = {Scope.Name}")]
    public class CommandBinding
    {
        public CommandId Command { get; private set;}

        public KeybindingScope Scope { get; private set; }

        public IReadOnlyList<BindingSequence> Sequences { get; private set; }

        public CommandBinding(CommandId command, KeybindingScope scope, IEnumerable<BindingSequence> sequences)
        {
            this.Command = command;
            this.Scope = scope;

            this.Sequences = new List<BindingSequence>(sequences);
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
