using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace VSShortcutsManager
{
    /// <summary>
    /// Interaction logic for KeyboardShortcutListView.xaml
    /// </summary>
    public partial class LiveShortcutsView : Window
    {

        #region Fields
        private bool _CtrlKeyPressed;
        private bool _AltKeyPressed;
        private bool _ShiftKeyPressed;
        LiveShortCutViewViewModel viewModel;
        #endregion

        #region Properties

        public IServiceProvider ServiceProvider { get; set; }
        public bool CtrlKeyPressed
        {
            get { return _CtrlKeyPressed; }
            set
            {
                _CtrlKeyPressed = value;
                btnControl.Style = _CtrlKeyPressed ? Resources["buttonPressedStyle"] as Style : FindResource("buttonStyle") as Style;
            }
        }
        public bool AltKeyPressed
        {
            get { return _AltKeyPressed; }
            set
            {
                _AltKeyPressed = value;
                btnAlt.Style = AltKeyPressed ? Resources["buttonPressedStyle"] as Style : FindResource("buttonStyle") as Style;

            }
        }
        public bool ShiftKeyPressed
        {
            get { return _ShiftKeyPressed; }
            set
            {
                _ShiftKeyPressed = value;
                btnShift.Style = ShiftKeyPressed ? Resources["buttonPressedStyle"] as Style : FindResource("buttonStyle") as Style;
            }
        }
        #endregion

        #region Construcor
        public LiveShortcutsView(IServiceProvider ServiceProvider)
        {
            InitializeComponent();
            this.ServiceProvider = ServiceProvider;
            viewModel = new LiveShortCutViewViewModel();
            this.KeyDown += captureKeyDown;
            DataContext = viewModel;
        }
        #endregion

        #region Events
        private void captureKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) CtrlKeyPressed = !CtrlKeyPressed;
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt) AltKeyPressed = !AltKeyPressed;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) ShiftKeyPressed = !ShiftKeyPressed;

            RefreshShortcuts();
        }
        private void lvAlphaKeys_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (KeyList)lvAlphaKeys.SelectedItem;
            viewModel.Chords = "Ctrl + Alt +" + item.Name;
            CtrlKeyPressed = AltKeyPressed = ShiftKeyPressed = false;
        }
        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            CtrlKeyPressed = !CtrlKeyPressed;
            RefreshShortcuts();
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            ShiftKeyPressed = !ShiftKeyPressed;
            RefreshShortcuts();
        }
        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            AltKeyPressed = !AltKeyPressed;
            RefreshShortcuts();
        }
        #endregion

        #region private Methods
        private ModifierKeys GetSelectedModifierKey()
        {
            ModifierKeys modifierkeys;

            if (CtrlKeyPressed && AltKeyPressed && ShiftKeyPressed)
            {
                modifierkeys = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift;
            }
            else if (CtrlKeyPressed && AltKeyPressed)
            {
                modifierkeys = ModifierKeys.Control | ModifierKeys.Alt;
            }
            else if (CtrlKeyPressed && ShiftKeyPressed)
            {
                modifierkeys = ModifierKeys.Control | ModifierKeys.Shift;
            }
            else if (AltKeyPressed && ShiftKeyPressed)
            {
                modifierkeys = ModifierKeys.Alt | ModifierKeys.Shift;
            }
            else if (CtrlKeyPressed)
            {
                modifierkeys = ModifierKeys.Control;
            }
            else if (AltKeyPressed)
            {
                modifierkeys = ModifierKeys.Alt;
            }
            else if (ShiftKeyPressed)
            {
                modifierkeys = ModifierKeys.Shift;
            }
            else
            {
                modifierkeys = ModifierKeys.None;
            }
            return modifierkeys;
        }

        private async void RefreshShortcuts()
        {
            var modifierKey = GetSelectedModifierKey();
            VSShortcutQueryEngine engine = new VSShortcutQueryEngine(ServiceProvider);
            Guid scopeGuid = Guid.Parse("8B382828-6202-11D1-8870-0000F87579D2");     // Get Guid Scope
            BindingSequence bindingSequence = BindingSequence.Empty; // TODO: This is if there is a Chord, otherwise BindingSequence.EMPTY
            const bool includeGlobals = true;
            IDictionary<string, IEnumerable<Tuple<CommandBinding, Command>>> bindingsMap = await engine.GetBindingsForModifiersAsync(scopeGuid, ModifierKeys.None, bindingSequence, includeGlobals);
            
        }

        #endregion

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

    }


    public class LiveShortCutViewViewModel : INotifyPropertyChanged
    {

        #region Private Fields
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Chords;
        private List<KeyList> _AlphaKeys;
        private List<KeyList> _FunctionKeys;
        private List<KeyList> _NumericKeys;
        private List<KeyList> _SpecialKeys;
        private List<KeyList> _NumpadKeys;
        private List<KeyList> _NumpadsymbolKeys;
        private List<KeyList> _SystemKeys1;
        private List<KeyList> _SystemKeys2;
        private List<KeyList> _CursorandEditKeys;
        private List<KeyList> _SystemandStateKeys;
        #endregion

        #region Constructor
        public LiveShortCutViewViewModel()
        {
            LoadKeys();
        }
        #endregion

        #region Properties
        public string Chords
        {
            get { return _Chords; }
            set
            {
                _Chords = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Chords"));
            }
        }
        public List<KeyList> AlphaKeys
        {
            get { return _AlphaKeys; }
            set
            {
                _AlphaKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AlphaKeys"));
            }
        }
        public List<KeyList> FunctionKeys
        {
            get { return _FunctionKeys; }
            set
            {
                _FunctionKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FunctionKeys"));
            }
        }
        public List<KeyList> NumericKeys
        {
            get { return _NumericKeys; }
            set
            {
                _NumericKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NumericKeys"));
            }
        }
        public List<KeyList> SpecialKeys
        {
            get { return _SpecialKeys; }
            set
            {
                _SpecialKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SpecialKeys"));
            }
        }
        public List<KeyList> NumpadKeys
        {
            get { return _NumpadKeys; }
            set
            {
                _NumpadKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NumpadKeys"));

            }
        }
        public List<KeyList> NumpadsymbolKeys
        {
            get { return _NumpadsymbolKeys; }
            set
            {
                _NumpadsymbolKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NumpadsymbolKeys"));

            }
        }
        public List<KeyList> SystemKeys1
        {
            get { return _SystemKeys1; }
            set
            {
                _SystemKeys1 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SystemKeys1"));

            }
        }
        public List<KeyList> SystemKeys2
        {
            get { return _SystemKeys2; }
            set
            {
                _SystemKeys2 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SystemKeys2"));

            }
        }
        public List<KeyList> CursorandEditKeys
        {
            get { return _CursorandEditKeys; }
            set
            {
                _CursorandEditKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CursorandEditKeys"));

            }
        }
        public List<KeyList> SystemandStateKeys
        {
            get { return _SystemandStateKeys; }
            set
            {
                _SystemandStateKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SystemandStateKeys"));

            }
        }

        #endregion

        #region private Methods
        private void LoadKeys()
        {
            AlphaKeys = new List<KeyList>();
            AlphaKeys.Add(new KeyList() { Name = "A", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "B", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "C", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "D", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "E", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "F", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "G", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "H", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "I", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "J", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "K", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "L", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "M", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "N", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "O", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "P", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "Q", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "R", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "S", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "T", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "U", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "V", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "W", Command = "Window.Move Navigation Bar" });
            AlphaKeys.Add(new KeyList() { Name = "X", Command = "Edit.Find Next Selected" });
            AlphaKeys.Add(new KeyList() { Name = "Y", Command = "Help.View Help" });
            AlphaKeys.Add(new KeyList() { Name = "Z", Command = "Window.Move Navigation Bar" });


            FunctionKeys = new List<KeyList>();
            FunctionKeys.Add(new KeyList() { Name = "F1", Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F2", Command = "Help.View Help" });
            FunctionKeys.Add(new KeyList() { Name = "F3", Command = "Edit.Find Next Selected" });
            FunctionKeys.Add(new KeyList() { Name = "F4", Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F5", Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F6", Command = "Help.View Help" });
            FunctionKeys.Add(new KeyList() { Name = "F7", Command = "Edit.Find Next Selected" });
            FunctionKeys.Add(new KeyList() { Name = "F8", Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F9", Command = "Window.Move Navigation Bar" });
            FunctionKeys.Add(new KeyList() { Name = "F10", Command = "Help.View Help" });
            FunctionKeys.Add(new KeyList() { Name = "F11", Command = "Edit.Find Next Selected" });
            FunctionKeys.Add(new KeyList() { Name = "F12", Command = "Window.Move Navigation Bar" });

            NumericKeys = new List<KeyList>();
            NumericKeys.Add(new KeyList() { Name = "1", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "2", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "3", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "4", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "5", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "6", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "7", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "8", Command = "Window.Move Navigation Bar" });
            NumericKeys.Add(new KeyList() { Name = "9", Command = "Window.Move Navigation Bar" });

            SpecialKeys = new List<KeyList>();
            SpecialKeys.Add(new KeyList() { Name = "`~", Command = "OEM_3" });
            SpecialKeys.Add(new KeyList() { Name = "-_", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "=+", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "[{", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "]}", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "\\|", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = ";:", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "'\"", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = ",<", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = ".>", Command = "Window.Move Navigation Bar" });
            SpecialKeys.Add(new KeyList() { Name = "/?", Command = "Window.Move Navigation Bar" });

            NumpadKeys = new List<KeyList>();
            NumpadKeys.Add(new KeyList() { Name = "NumPad0", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad1", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad2", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad3", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad4", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad5", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad6", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad7", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad8", Command = "Window.Move Navigation Bar" });
            NumpadKeys.Add(new KeyList() { Name = "NumPad9", Command = "Window.Move Navigation Bar" });

            NumpadsymbolKeys = new List<KeyList>();
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad.", Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad/", Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad*", Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad-", Command = "Window.Move Navigation Bar" });
            NumpadsymbolKeys.Add(new KeyList() { Name = "NumPad+", Command = "Window.Move Navigation Bar" });


            SystemKeys1 = new List<KeyList>();
            SystemKeys1.Add(new KeyList() { Name = "ESC", Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "TAB", Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "BackSpace", Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "ENTER", Command = "Window.Move Navigation Bar" });
            SystemKeys1.Add(new KeyList() { Name = "SPACE", Command = "Window.Move Navigation Bar" });

            SystemKeys2 = new List<KeyList>();
            SystemKeys2.Add(new KeyList() { Name = "INS", Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "DEL", Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "HOME", Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "END", Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "PgUp", Command = "Window.Move Navigation Bar" });
            SystemKeys2.Add(new KeyList() { Name = "PgDn", Command = "Window.Move Navigation Bar" });

            CursorandEditKeys = new List<KeyList>();
            CursorandEditKeys.Add(new KeyList() { Name = "Up", Command = "Window.Move Navigation Bar" });
            CursorandEditKeys.Add(new KeyList() { Name = "Down", Command = "Window.Move Navigation Bar" });
            CursorandEditKeys.Add(new KeyList() { Name = "Left", Command = "Window.Move Navigation Bar" });
            CursorandEditKeys.Add(new KeyList() { Name = "Right", Command = "Window.Move Navigation Bar" });

            SystemandStateKeys = new List<KeyList>();
            SystemandStateKeys.Add(new KeyList() { Name = "PrintScreen", Command = "Window.Move Navigation Bar" });
            SystemandStateKeys.Add(new KeyList() { Name = "ScrollLock", Command = "Window.Move Navigation Bar" });
            SystemandStateKeys.Add(new KeyList() { Name = "Pause/Break", Command = "Window.Move Navigation Bar" });

        }
        #endregion

        #region Public Method
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #endregion
    }

}
