using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace VSShortcutsManager.AddNewShortcut
{
    public static class InputKeys
    {
        public static ModifierKeys ConvertToModifierKey(Key inputKey)
        {
            switch (inputKey)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return ModifierKeys.Control;
                case Key.LeftAlt:
                case Key.RightAlt:
                    return ModifierKeys.Alt;
                case Key.LeftShift:
                case Key.RightShift:
                    return ModifierKeys.Shift;
            }

            return ModifierKeys.None;
        }
        public static bool checkIfModifierKey (Key inputKey)
        {
            if (ConvertToModifierKey(inputKey) != ModifierKeys.None)
            {
                return true;
            }
            return false;
        }
    }
}
