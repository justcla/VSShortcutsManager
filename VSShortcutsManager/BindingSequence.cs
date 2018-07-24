using System.Diagnostics;
using System.Windows.Input;

namespace VSShortcutsManager
{
    [DebuggerDisplay("{Modifiers}+{Chord}")]
    internal class BindingSequence
    {
        public BindingSequence(ModifierKeys modifierKeys, string chord)
        {
            this.Modifiers = modifierKeys;
            this.Chord = chord;
        }

        /// <summary>
        /// Modifier keys associated with this binding sequence
        /// </summary>
        public ModifierKeys Modifiers { get; private set; }

        /// <summary>
        /// The binding chord associated with this binding sequence
        /// </summary>
        public string Chord { get; private set; }
    }
}
