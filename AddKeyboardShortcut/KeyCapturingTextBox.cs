using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using VSShortcutsManager;

namespace AddKeyboardShortcut
{
    class KeyCapturingTextBox : TextBox
    {
        public KeyCapturingTextBox()
        {
            base.PreviewKeyDown += KeyCapturingTextBox_PreviewKeyDown;
        }

        private void KeyCapturingTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = GetKey(e);

            // Certain keys are not captured by design.  Check for those here and exit early.
            if (!ShouldCapture(key))
            {
                return;
            }

            // Allow user to input Tab only when there are no keys in the sequence yet; otherwise, Tab
            // moves to the next control.
            if (BindingSequences.Count > 0 && (key == Key.Tab))
            {
                return;
            }

            e.Handled = true;

            if (BindingSequences.Count == 2)
            {
                BindingSequences.Clear();
            }

            ModifierKeys modifiers = Keyboard.Modifiers;
            string keyName = key.ToString();
            BindingSequences.Add(new BindingSequence(modifiers, keyName));

            base.Text = string.Join(", ", BindingSequences);
            base.CaretIndex = base.Text.Length;
        }

        private bool ShouldCapture(Key key)
        {
            return
                key != Key.Apps
                && key != Key.LWin
                && key != Key.RWin
                && key != Key.LeftAlt
                && key != Key.RightAlt
                && key != Key.LeftCtrl
                && key != Key.RightCtrl
                && key != Key.LeftShift
                && key != Key.RightShift;
        }

        public ObservableCollection<BindingSequence> BindingSequences { get; } = new ObservableCollection<BindingSequence>();

        private Key GetKey(KeyEventArgs args)
        {
            return
                (args.Key == Key.System)
                ? args.SystemKey
                : args.Key;
        }
    }
}
