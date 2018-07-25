using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSShortcutsManager
{
    /// <summary>
    /// Interaction logic for KeyboardShortcutListView.xaml
    /// </summary>
    public partial class KeyboardShortcutListView : Window
    {

        private string scope;

        #region Properties
        public bool CtrlKeyPressed { get; set; }
        public bool AltKeyPressed { get; set; }
        public bool ShiftKeyPressed { get; set; }

        public List<KeyList> AlphaKeys { get; set; }
        public List<KeyList> FunctionKeys { get; set; }
        public List<KeyList> NumericKeys { get; set; }
        public List<KeyList> SpecialKeys { get; set; }
        public List<KeyList> NumpadKeys { get; set; }
        public List<KeyList> NumpadsymbolKeys { get; set; }
        public List<KeyList> SystemKeys1 { get; set; }
        public List<KeyList> SystemKeys2 { get; set; }
        public List<KeyList> CursorandEditKeys { get; set; }
        public List<KeyList> SystemandStateKeys { get; set; }
        #endregion

        public KeyboardShortcutListView()
        {
            InitializeComponent();
            this.KeyDown += captureKeyDown;
            LoadKeys();
            lvAlphaKeys.ItemsSource = AlphaKeys;
            lvFunctionKeys.ItemsSource = FunctionKeys;
            lvnumKeys.ItemsSource = NumericKeys;
            lvSpecialKeys.ItemsSource = SpecialKeys;
            lvNumpadKeys.ItemsSource = NumpadKeys;
            lvNumpadSymbolKeys.ItemsSource = NumpadsymbolKeys;
            lvSystemKeys1.ItemsSource = SystemKeys1;
            lvSystemKeys2.ItemsSource = SystemKeys2;
            lvCursorandEditKeys.ItemsSource = CursorandEditKeys;
            lvSystemandStateKeys.ItemsSource = SystemandStateKeys;

        }

        private void LoadKeys()
        {
            AlphaKeys = new List<KeyList>();
            AlphaKeys.Add(new KeyList() { Name = "A", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "B", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "C", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "D", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "E", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "F", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "G", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "H", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "I", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "J", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "K", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "L", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "M", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "N", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "O", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "P", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "Q", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "R", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "S", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "T", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "U", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "V", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "W", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "X", GroupeId = 1, Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "Y", GroupeId = 1, Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "Z", GroupeId = 1, Command = "Window.Move Navigation Bar" });


            FunctionKeys = new List<KeyList>();
            FunctionKeys.Add(new KeyList() { Name = "F1", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F2", GroupeId = 1, Command = "Help.View Help" });
            FunctionKeys.Add(new KeyList() { Name = "F3", GroupeId = 1, Command = "Edit.Find Next Selected" });
            FunctionKeys.Add(new KeyList() { Name = "F4", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F5", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F6", GroupeId = 1, Command = "Help.View Help" });
            FunctionKeys.Add(new KeyList() { Name = "F7", GroupeId = 1, Command = "Edit.Find Next Selected" });
            FunctionKeys.Add(new KeyList() { Name = "F8", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F9", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F10", GroupeId = 1, Command = "Help.View Help" });
            FunctionKeys.Add(new KeyList() { Name = "F11", GroupeId = 1, Command = "Edit.Find Next Selected" });
            FunctionKeys.Add(new KeyList() { Name = "F12", GroupeId = 1, Command = "Window.Move Navigation Bar" });

            NumericKeys = new List<KeyList>();
            NumericKeys.Add(new KeyList() { Name = "1", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "2", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "3", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "4", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "5", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "6", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "7", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "8", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "9", GroupeId = 1, Command = "Window.Move Navigation Bar" });

            SpecialKeys = new List<KeyList>();
            SpecialKeys.Add(new KeyList() { Name = "`~", GroupeId = 1, Command = "OEM_3" });
            SpecialKeys.Add(new KeyList() { Name = "-_", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "=+", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "[{", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "]}", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "\\|", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = ";:", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "'\"", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = ",<", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = ".>", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "/?", GroupeId = 1, Command = "Window.Move Navigation Bar" });

            NumpadKeys = new List<KeyList>();
            NumpadKeys.Add(new KeyList() { Name = "NumPad0", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad1", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad2", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad3", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad4", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad5", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad6", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad7", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad8", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad9", GroupeId = 1, Command = "Window.Move Navigation Bar" });

            NumpadsymbolKeys = new List<KeyList>();
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad.", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad/", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad*", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad-", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad+", GroupeId = 1, Command = "Window.Move Navigation Bar" });


            SystemKeys1 = new List<KeyList>();
            SystemKeys1.Add(new KeyList() { Name = "ESC", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "TAB", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "BackSpace", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "ENTER", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "SPACE", GroupeId = 1, Command = "Window.Move Navigation Bar" });

            SystemKeys2 = new List<KeyList>();
            SystemKeys2.Add(new KeyList() { Name = "INS", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "DEL", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "HOME", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "END", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "PgUp", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "PgDn", GroupeId = 1, Command = "Window.Move Navigation Bar" });

            CursorandEditKeys = new List<KeyList>();
            CursorandEditKeys.Add(new KeyList() { Name = "Up", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            CursorandEditKeys.Add(new KeyList() { Name = "Down", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            CursorandEditKeys.Add(new KeyList() { Name = "Left", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            CursorandEditKeys.Add(new KeyList() { Name = "Right", GroupeId = 1, Command = "Window.Move Navigation Bar" });

            SystemandStateKeys = new List<KeyList>();
            SystemandStateKeys.Add(new KeyList() { Name = "PrintScreen", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemandStateKeys.Add(new KeyList() { Name = "ScrollLock", GroupeId = 1, Command = "Window.Move Navigation Bar" });
            SystemandStateKeys.Add(new KeyList() { Name = "Pause/Break", GroupeId = 1, Command = "Window.Move Navigation Bar" });

        }

        #region Private Methods
        private void SwitchCtrlKey()
        {
            CtrlKeyPressed = !CtrlKeyPressed;
            btnControl.Style = CtrlKeyPressed ? Resources["buttonPressedStyle"] as Style : FindResource("buttonStyle") as Style;
        }
        private void SwitchAltKey()
        {
            AltKeyPressed = !AltKeyPressed;
            btnAlt.Style = AltKeyPressed ? Resources["buttonPressedStyle"] as Style : FindResource("buttonStyle") as Style;
        }
        private void switchShiftKey()
        {
            ShiftKeyPressed = !ShiftKeyPressed;
            btnShift.Style = ShiftKeyPressed ? Resources["buttonPressedStyle"] as Style : FindResource("buttonStyle") as Style;
        }
        private void captureKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) SwitchCtrlKey();
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt) SwitchAltKey();
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) switchShiftKey();
            var host = new Window();
        }

        #endregion

        #region Events
        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            SwitchCtrlKey();
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            switchShiftKey();
        }
        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            SwitchAltKey();
        }


        #endregion

    }

    public class KeyboardShortcutList
    {
        public static void DispalyKeyboardShortcutList()
        {
            KeyboardShortcutListView dialog = new KeyboardShortcutListView();
            dialog.ShowDialog();
        }
    }

    public class KeyList
    {
        public KeyList()
        {
            Commands = new List<string>();
        }

        public string Name { get; set; }

        public string Command { get; set; }

        public List<string> Commands { get; set; }

        public int GroupeId { get; set; }
    }
}
