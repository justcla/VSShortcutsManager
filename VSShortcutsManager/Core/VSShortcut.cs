using System.Collections.Generic;

namespace VSShortcutsManager
{
    public class VSShortcut
    {
        public string Command { get; set; }
        public string Scope { get; set; }
        public string Shortcut { get; set; }
        public IEnumerable<string> Conflicts { get; set; }
        public string Operation { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as VSShortcut;

            if (item == null)
            {
                return false;
            }

            return Operation.Equals(item.Operation) &&
                Command.Equals(item.Command) && Scope.Equals(item.Scope) && Shortcut.Equals(item.Shortcut);
        }

        public override int GetHashCode()
        {
            return Operation.GetHashCode() + Command.GetHashCode() + Scope.GetHashCode() + Shortcut.GetHashCode();
        }
    }
}