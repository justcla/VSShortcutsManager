using System.Diagnostics;
using System.Windows.Input;

namespace VSShortcutsManager
{
    internal class BindingSequence
    {
        private BindingSequence() { }

        public BindingSequence(ModifierKeys modifierKeys, string key)
        {
            this.Modifiers = modifierKeys;
            this.Key = key;
        }

        public static readonly BindingSequence Empty = new BindingSequence();

        /// <summary>
        /// Modifier keys associated with this binding sequence
        /// </summary>
        public ModifierKeys Modifiers { get; private set; }

        /// <summary>
        /// The key associated with this binding sequence
        /// </summary>
        public string Key { get; private set; }

        public override string ToString()
        {
            string modifierString;
            if(this.Modifiers != ModifierKeys.None)
            {
                modifierString = this.Modifiers.ToString().Replace(',', '+').Replace(" ", string.Empty) + "+";
            }
            else
            {
                modifierString = string.Empty;
            }

            return modifierString + Key;
        }
    }
}
