using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using VSShortcutsManager;

namespace AddKeyboardShortcut
{
    class KeyCapturingTextBox : TextBox
    {
        private readonly Tuple<Key, ModifierKeys>[] modifierKeyMapping = new[]
        {
            Tuple.Create(Key.LeftShift, ModifierKeys.Shift),
            Tuple.Create(Key.RightShift, ModifierKeys.Shift),

            Tuple.Create(Key.LeftAlt, ModifierKeys.Alt),
            Tuple.Create(Key.RightAlt, ModifierKeys.Alt),

            Tuple.Create(Key.LeftCtrl, ModifierKeys.Control),
            Tuple.Create(Key.RightCtrl, ModifierKeys.Control),
        };

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

            ModifierKeys modifiers = GetCurrentModifierKeys();
            string keyName = key.ToString();
            BindingSequences.Add(new BindingSequence(modifiers, keyName));

            base.Text = string.Join(", ", BindingSequences);
            base.CaretIndex = base.Text.Length;
        }

        private bool ShouldCapture(Key key)
        {
            return
                !modifierKeyMapping.Select(m => m.Item1).Contains(key)
                && key != Key.Apps
                && key != Key.LWin
                && key != Key.RWin;
        }

        public ObservableCollection<BindingSequence> BindingSequences { get; } = new ObservableCollection<BindingSequence>();

        private Key GetKey(KeyEventArgs args)
        {
            return
                (args.Key == Key.System)
                ? args.SystemKey
                : args.Key;
        }

        private ModifierKeys GetModifierKeys(Key key)
        {
            switch (key)
            {
                case Key.LeftAlt: return ModifierKeys.Alt;
                case Key.RightAlt: return ModifierKeys.Alt;
                case Key.LeftCtrl: return ModifierKeys.Control;
                case Key.RightCtrl: return ModifierKeys.Control;
                case Key.LeftShift: return ModifierKeys.Shift;
                case Key.RightShift: return ModifierKeys.Shift;
                default: return ModifierKeys.None;
            }
        }

        private ModifierKeys GetCurrentModifierKeys()
        {
            ModifierKeys result = ModifierKeys.None;

            foreach (Tuple<Key, ModifierKeys> map in modifierKeyMapping)
            {
                if (Keyboard.IsKeyDown(map.Item1))
                {
                    result |= map.Item2;
                }
            }

            return result;
        }
    }
}
