using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using VSShortcutsManager;

namespace CustomControls
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

            e.Handled = true;

            ModifierKeys modifiers = Keyboard.Modifiers;

            if (e.Key == Key.Back && modifiers == ModifierKeys.None)
            {
                if (BindingSequences.Count > 0)
                {
                    BindingSequences.RemoveAt(BindingSequences.Count - 1);
                }
            }
            else
            {
                if (BindingSequences.Count == 2)
                {
                    BindingSequences.Clear();
                }

                string keyName = key.ToString();
                BindingSequences.Add(new BindingSequence(modifiers, keyName));
            }

            // Print the text to the input box
            UpdateShortcutKeysDisplay();
        }

        private void UpdateShortcutKeysDisplay()
        {
            base.Text = GetLocalizedShortcutText(BindingSequences);
            base.CaretIndex = base.Text.Length;
        }

        private string GetLocalizedShortcutText(ObservableCollection<BindingSequence> bindingSequences)
        {
            // Localize the binding sequence (ie. "Control+D8" => "Ctrl+8")
            VSShortcutQueryEngine queryEngine = VSShortcutsManager.VSShortcutsManager.Instance.queryEngine;
            if (queryEngine == null)
            {
                // Abort!
                System.Diagnostics.Debug.WriteLine("VSShortcutQueryEngine not initialized.");
                return string.Join(", ", bindingSequences);
            }

            return queryEngine.GetLocalizedShortcutText(bindingSequences);
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
                && key != Key.RightShift
                && key != Key.Tab;
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
