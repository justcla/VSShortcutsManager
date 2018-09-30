using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace VSShortcutsManager
{
    class KeycharUtil
    {
        // --- Get char from Key, courtesy of https://stackoverflow.com/a/5826175/879243

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            char ch = '\0';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            // To get the base keys (ie. '/' instead of '?'), get a keyboard state with no modifiers
            // Special handling applied to a-z so they print nicely as A-Z.
            byte[] keyboardState = GetKeyboardStateWithoutModifiers(forceShiftKey: IsAlphaKey(key));

            StringBuilder stringBuilder = new StringBuilder(2);
            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    // An error occurred (or dead key)
                    break;
                case 0:
                    // No key was found with this combination of key and keyboard state
                    break;
                case 1:
                    // Exactly one char returned
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    // More than one char returned (could be dead key / French, etc)
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

        private static bool IsAlphaKey(Key key)
        {
            // Return true if between [A-Z] (A=44, Z=69)
            int keyCodeInt = (int)key;
            return keyCodeInt >= (int)Key.A && keyCodeInt <= (int)Key.Z;
        }

        /// <summary>
        ///  Get a keyboard state with no modifiers.
        ///  ie. get the base keys ('/' instead of '?')
        ///  Special handling applied to a-z so they print nicely as A-Z.
        /// </summary>
        /// <param name="forceShiftKey">If true, special handling applied so a-z prints nicely as A-Z</param>
        /// <returns></returns>
        private static byte[] GetKeyboardStateWithoutModifiers(bool forceShiftKey)
        {
            // Return a keyboardState array with all modifiers stripped
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            // TODO: Find out where these constants are defined for real and reference them.
            const byte VK_SHIFT = 0x10;
            const byte VK_CONTROL = 0x11;
            const byte VK_ALT = 0x12;
            const byte VK_LSHIFT = 0xA0;
            const byte VK_RSHIFT = 0xA1;
            const byte VK_LCONTROL = 0xA2;
            const byte VK_RCONTROL = 0xA3;
            const byte VK_LALT = 0xA4;
            const byte VK_RALT = 0xA5;

            // Set all modifier keys to OFF (Ctrl, Shift, Alt)
            // Handle Shift Key modifier.
            // Special treatment for keys A-Z: If forceShiftKey is true, chars will print in uppercase. (ie. "R" instead of "r")
            SetKeyState(ref keyboardState, VK_SHIFT, setOn: forceShiftKey);
            SetKeyState(ref keyboardState, VK_LSHIFT, setOn: forceShiftKey);
            SetKeyState(ref keyboardState, VK_RSHIFT, setOn: forceShiftKey);
            // Turn off the Control Key modifier
            TurnOffKey(ref keyboardState, VK_CONTROL);
            TurnOffKey(ref keyboardState, VK_ALT);
            TurnOffKey(ref keyboardState, VK_LCONTROL);
            // Turn off the Alt Key modifier
            TurnOffKey(ref keyboardState, VK_RCONTROL);
            TurnOffKey(ref keyboardState, VK_LALT);
            TurnOffKey(ref keyboardState, VK_RALT);

            return keyboardState;
        }

        private static void SetKeyState(ref byte[] keyboardState, byte vkKey, bool setOn)
        {
            if (setOn)
            {
                TurnOnKey(ref keyboardState, vkKey);
            }
            else
            {
                TurnOffKey(ref keyboardState, vkKey);
            }
        }

        private static void TurnOnKey(ref byte[] keyboardState, byte vkKey)
        {
            // The highest-order bit represents on/off.
            // When the key is on, the value is 0x81 or 0x80, when off, the value is 0x01 or 0x00.
            keyboardState[vkKey] = (byte)(keyboardState[vkKey] | 0x80);
        }

        private static void TurnOffKey(ref byte[] keyboardState, byte vkKey)
        {
            // The highest-order bit represents on/off.
            // When the key is on, the value is 0x81 or 0x80, when off, the value is 0x01 or 0x00.
            keyboardState[vkKey] = (byte)(keyboardState[vkKey] & ~0x80);
        }

    }
}