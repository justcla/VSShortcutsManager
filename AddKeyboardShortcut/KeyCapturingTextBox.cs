using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using VSShortcutsManager;

namespace AddKeyboardShortcut
{
    class KeyCapturingTextBox : TextBox, INotifyPropertyChanged
    {
        private ModifierKeys currentModifierKeys;
        private readonly ObservableCollection<BindingSequence> bindingSequences = new ObservableCollection<BindingSequence>();

        public event PropertyChangedEventHandler PropertyChanged;

        public KeyCapturingTextBox()
        {
            base.PreviewKeyDown += KeyCapturingTextBox_PreviewKeyDown;
            base.PreviewKeyUp += KeyCapturingTextBox_PreviewKeyUp;
        }

        private void KeyCapturingTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            this.currentModifierKeys &= ~GetModifierKeys(GetKey(e));
        }

        private void KeyCapturingTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = GetKey(e);
            ModifierKeys modifier = GetModifierKeys(key);
            this.currentModifierKeys |= modifier;
            bool isModifier = (modifier != ModifierKeys.None);
            if (!isModifier)
            {
                e.Handled = true;

                if (bindingSequences.Count == 2)
                {
                    bindingSequences.Clear();
                }

                string keyName = key.ToString();
                bindingSequences.Add(new BindingSequence(this.currentModifierKeys, keyName));

                base.Text = string.Join(", ", bindingSequences);
                base.CaretIndex = base.Text.Length;
            }
        }

        public ObservableCollection<BindingSequence> BindingSequences => this.bindingSequences;

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
                case Key.LWin: return ModifierKeys.Windows;
                case Key.RWin: return ModifierKeys.Windows;
                default: return ModifierKeys.None;
            }
        }
    }
}
