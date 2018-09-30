using System.Collections.Generic;

namespace VSShortcutsManager
{
    public class VSShortcut
    {
        public string Command { get; set; }
        public string Scope { get; set; }
        public string Shortcut { get; set; }
        public IEnumerable<string> Conflicts { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as VSShortcut;

            if (item == null)
            {
                return false;
            }

            return Command.Equals(item.Command) && Scope.Equals(item.Scope) && Shortcut.Equals(item.Shortcut);
        }

        public override int GetHashCode()
        {
            return Command.GetHashCode() + Scope.GetHashCode() + Shortcut.GetHashCode();
        }
    }
}