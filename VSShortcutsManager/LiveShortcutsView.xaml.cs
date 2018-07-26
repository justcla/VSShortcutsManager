using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;

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
            //var item = (KeyList)lvAlphaKeys.SelectedItem;
            //viewModel.Chords = "Ctrl + Alt +" + item.Name;
            //CtrlKeyPressed = AltKeyPressed = ShiftKeyPressed = false;
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
        private ModifierKeys GetSelectedModifierKeys()
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
            try
            {
                var modifierKeys = GetSelectedModifierKeys();
                VSShortcutQueryEngine engine = new VSShortcutQueryEngine(ServiceProvider);
                var selectedscope = (Scope)cmbScopeList.SelectedItem;
                Guid scopeGuid = (selectedscope != null) ? Guid.Parse(selectedscope.ID) : Guid.Empty;
                BindingSequence bindingSequence = BindingSequence.Empty; // TODO: This is if there is a Chord, otherwise BindingSequence.EMPTY
                const bool includeGlobals = true;
                IDictionary<string, IEnumerable<Tuple<CommandBinding, Command>>> matchingShortcuts = await engine.GetBindingsForModifiersAsync(scopeGuid, modifierKeys, bindingSequence, includeGlobals);
                foreach (var matchingShortcut in matchingShortcuts)
                {
                    var key = matchingShortcut.Key;
                    var commandBindings = matchingShortcut.Value;
                    var commandString = String.Join(",", commandBindings.Select(commandBinding => commandBinding.Item2.DisplayName));
                    UpdateShortcutValue(key, commandString);
                }
            }
            catch (Exception ex)
            {

            }

        }
        private void UpdateShortcutValue(string key, string commandString)
        {
            if (viewModel.AlphaKeys?.ContainsKey(key) == true)
            {
                viewModel.AlphaKeys[key].DispalyName = commandString;
            }
            else if (viewModel.FunctionKeys.ContainsKey(key))
            {
                viewModel.FunctionKeys[key].DispalyName = commandString;
            }
            else if (viewModel.NumericKeys?.ContainsKey(key) == true)
            {
                viewModel.NumericKeys[key].DispalyName = commandString;
            }
            else if (viewModel.NumpadKeys?.ContainsKey(key) == true)
            {
                viewModel.NumpadKeys[key].DispalyName = commandString;
            }
            else if (viewModel.NumpadsymbolKeys?.ContainsKey(key) == true)
            {
                viewModel.NumpadsymbolKeys[key].DispalyName = commandString;
            }
            else if (viewModel.SystemKeys1?.ContainsKey(key) == true)
            {
                viewModel.SystemKeys1[key].DispalyName = commandString;
            }
            else if (viewModel.SystemKeys2?.ContainsKey(key) == true)
            {
                viewModel.SystemKeys2[key].DispalyName = commandString;
            }
            else if (viewModel.CursorandEditKeys?.ContainsKey(key) == true)
            {
                viewModel.CursorandEditKeys[key].DispalyName = commandString;
            }
            else if (viewModel.SystemandStateKeys?.ContainsKey(key) == true)
            {
                viewModel.SystemandStateKeys[key].DispalyName = commandString;
            }
        }
        #endregion
    }

    public class LiveShortCutViewViewModel : INotifyPropertyChanged
    {

        #region Private Fields
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Chords;
        private Dictionary<string, ShortcutKeyCommand> _AlphaKeys;
        private Dictionary<string, ShortcutKeyCommand> _FunctionKeys;
        private Dictionary<string, ShortcutKeyCommand> _NumericKeys;
        private Dictionary<string, ShortcutKeyCommand> _SpecialKeys;
        private Dictionary<string, ShortcutKeyCommand> _NumpadKeys;
        private Dictionary<string, ShortcutKeyCommand> _NumpadsymbolKeys;
        private Dictionary<string, ShortcutKeyCommand> _SystemKeys1;
        private Dictionary<string, ShortcutKeyCommand> _SystemKeys2;
        private Dictionary<string, ShortcutKeyCommand> _CursorandEditKeys;
        private Dictionary<string, ShortcutKeyCommand> _SystemandStateKeys;
        private List<Scope> _ScopeLists;

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
        public Dictionary<string, ShortcutKeyCommand> AlphaKeys
        {
            get { return _AlphaKeys; }
            set
            {
                _AlphaKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AlphaKeys"));
            }
        }
        public Dictionary<string, ShortcutKeyCommand> FunctionKeys
        {
            get { return _FunctionKeys; }
            set
            {
                _FunctionKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FunctionKeys"));
            }
        }
        public Dictionary<string, ShortcutKeyCommand> NumericKeys
        {
            get { return _NumericKeys; }
            set
            {
                _NumericKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NumericKeys"));
            }
        }
        public Dictionary<string, ShortcutKeyCommand> SpecialKeys
        {
            get { return _SpecialKeys; }
            set
            {
                _SpecialKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SpecialKeys"));
            }
        }
        public Dictionary<string, ShortcutKeyCommand> NumpadKeys
        {
            get { return _NumpadKeys; }
            set
            {
                _NumpadKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NumpadKeys"));

            }
        }
        public Dictionary<string, ShortcutKeyCommand> NumpadsymbolKeys
        {
            get { return _NumpadsymbolKeys; }
            set
            {
                _NumpadsymbolKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NumpadsymbolKeys"));

            }
        }
        public Dictionary<string, ShortcutKeyCommand> SystemKeys1
        {
            get { return _SystemKeys1; }
            set
            {
                _SystemKeys1 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SystemKeys1"));

            }
        }
        public Dictionary<string, ShortcutKeyCommand> SystemKeys2
        {
            get { return _SystemKeys2; }
            set
            {
                _SystemKeys2 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SystemKeys2"));

            }
        }
        public Dictionary<string, ShortcutKeyCommand> CursorandEditKeys
        {
            get { return _CursorandEditKeys; }
            set
            {
                _CursorandEditKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CursorandEditKeys"));

            }
        }
        public Dictionary<string, ShortcutKeyCommand> SystemandStateKeys
        {
            get { return _SystemandStateKeys; }
            set
            {
                _SystemandStateKeys = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SystemandStateKeys"));

            }
        }
        public List<Scope> ScopeLists
        {
            get { return _ScopeLists; }

            set
            {
                _ScopeLists = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ScopeLists"));

            }
        }
        #endregion

        #region private Methods
        private void LoadKeys()
        {
            AlphaKeys = new Dictionary<string, ShortcutKeyCommand>();
            AlphaKeys.Add("A", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("B", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("C", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("D", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("E", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("F", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("G", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("H", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("I", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("J", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("K", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("L", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("M", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("N", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("O", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("P", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("Q", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("R", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("S", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("T", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("U", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("V", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("W", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });
            AlphaKeys.Add("X", new ShortcutKeyCommand() { DispalyName = "Edit.Find Next Selected" });
            AlphaKeys.Add("Y", new ShortcutKeyCommand() { DispalyName = "Help.View Help" });
            AlphaKeys.Add("Z", new ShortcutKeyCommand() { DispalyName = "Window.Move Navigation Bar" });


            FunctionKeys = new Dictionary<string, ShortcutKeyCommand>();
            FunctionKeys.Add("F1", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F2", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F3", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F4", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F5", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F6", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F7", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F8", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F9", new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F10",new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F11",new ShortcutKeyCommand() { DispalyName = "" });
            FunctionKeys.Add("F12",new ShortcutKeyCommand() { DispalyName = "" });

            NumericKeys = new Dictionary<string, ShortcutKeyCommand>();
            NumericKeys.Add("1", new ShortcutKeyCommand() { DispalyName = "" });
            NumericKeys.Add("2", new ShortcutKeyCommand() { DispalyName = "" });
            NumericKeys.Add("3", new ShortcutKeyCommand() { DispalyName = "" });
            NumericKeys.Add("4", new ShortcutKeyCommand() { DispalyName = "" }); 
            NumericKeys.Add("5", new ShortcutKeyCommand() { DispalyName = "" });
            NumericKeys.Add("6", new ShortcutKeyCommand() { DispalyName = "" });
            NumericKeys.Add("7", new ShortcutKeyCommand() { DispalyName = "" });
            NumericKeys.Add("8", new ShortcutKeyCommand() { DispalyName = "" });
            NumericKeys.Add("9", new ShortcutKeyCommand() { DispalyName = "" });

            SpecialKeys = new Dictionary<string, ShortcutKeyCommand>();
            SpecialKeys.Add("`~", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add("-_", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add("=+", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add("[{", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add("]}", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add( "\\|", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add( ";:", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add( "'\"", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add( ",<", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add( ".>", new ShortcutKeyCommand() { DispalyName = "" });
            SpecialKeys.Add( "/?", new ShortcutKeyCommand() { DispalyName = "" });

            NumpadKeys = new Dictionary<string, ShortcutKeyCommand>();
            NumpadKeys.Add("NumPad0", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad1", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad2", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad3", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad4", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad5", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad6", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad7", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad8", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadKeys.Add("NumPad9", new ShortcutKeyCommand() { DispalyName = "" });

            NumpadsymbolKeys = new Dictionary<string, ShortcutKeyCommand>();
            NumpadsymbolKeys.Add("NumPad.", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadsymbolKeys.Add("NumPad/", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadsymbolKeys.Add("NumPad*", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadsymbolKeys.Add("NumPad-", new ShortcutKeyCommand() { DispalyName = "" });
            NumpadsymbolKeys.Add("NumPad+", new ShortcutKeyCommand() { DispalyName = "" });


            SystemKeys1 = new Dictionary<string, ShortcutKeyCommand>();
            SystemKeys1.Add("ESC", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys1.Add("TAB", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys1.Add("BackSpace", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys1.Add("ENTER", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys1.Add("SPACE", new ShortcutKeyCommand() { DispalyName = "" });

            SystemKeys2 = new Dictionary<string, ShortcutKeyCommand>();
            SystemKeys2.Add("INS", new ShortcutKeyCommand() { DispalyName = "" }); 
            SystemKeys2.Add("DEL", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys2.Add("HOME", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys2.Add("END", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys2.Add("PgUp", new ShortcutKeyCommand() { DispalyName = "" });
            SystemKeys2.Add("PgDn", new ShortcutKeyCommand() { DispalyName = "" });

            CursorandEditKeys = new Dictionary<string, ShortcutKeyCommand>();
            CursorandEditKeys.Add("Up", new ShortcutKeyCommand() { DispalyName = "" });
            CursorandEditKeys.Add("Down", new ShortcutKeyCommand() { DispalyName = "" });
            CursorandEditKeys.Add("Left", new ShortcutKeyCommand() { DispalyName = "" });
            CursorandEditKeys.Add("Right", new ShortcutKeyCommand() { DispalyName = "" });

            SystemandStateKeys = CursorandEditKeys = new Dictionary<string, ShortcutKeyCommand>();
            SystemandStateKeys.Add("PrintScreen", new ShortcutKeyCommand() { DispalyName = "" });
            SystemandStateKeys.Add("ScrollLock", new ShortcutKeyCommand() { DispalyName = "" });
            SystemandStateKeys.Add("Pause/Break", new ShortcutKeyCommand() { DispalyName = "" });

            //Scope Lists
            ScopeLists = new List<Scope>();
            ScopeLists.Add(new Scope() { Name = "Team Explorer", ID = "{7AA20502-9463-47B7-BF43-341BAF51157C}" });
            ScopeLists.Add(new Scope() { Name = "VC Dialog Editor", ID = "{543E0C02-8C85-4E43-933A-5EF320E3431F}" });
            ScopeLists.Add(new Scope() { Name = "Find All References Tool Window", ID = "{1FA1FD06-3592-4D1D-AC75-0B953320140C}" });
            ScopeLists.Add(new Scope() { Name = "Live Property Explorer", ID = "{31FC2115-5126-4A87-B2F7-77EAAB65048B}" });
            ScopeLists.Add(new Scope() { Name = "XML (Text) Editor", ID = "{FA3CD31E-987B-443A-9B81-186104E8DAC1}" });
            ScopeLists.Add(new Scope() { Name = "Text Editor", ID = "{8B382828-6202-11D1-8870-0000F87579D2}" });
            ScopeLists.Add(new Scope() { Name = "Microsoft SQL Server Data Tools, T-SQL PDW Editor", ID = "{C9626B29-6A42-411C-BD76-DAD855C86913}" });
            ScopeLists.Add(new Scope() { Name = "ADO.NET Entity Data Model Designer", ID = "{C99AEA30-8E36-4515-B76F-496F5A48A6AA}" });
            ScopeLists.Add(new Scope() { Name = "Solution Explorer", ID = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}" });
            ScopeLists.Add(new Scope() { Name = "Query Designer", ID = "{B2C40B32-3A37-4CA9-97B9-FA44248B69FF}" });
            ScopeLists.Add(new Scope() { Name = "CSharp Editor with Encoding", ID = "{08467B34-B90F-4D91-BDCA-EB8C8CF3033A}" });
            ScopeLists.Add(new Scope() { Name = "WebBrowser", ID = "{E8B06F41-6D01-11D2-AA7D-00C04F990343}" });
            ScopeLists.Add(new Scope() { Name = "CSS Editor", ID = "{A5401142-F49D-43DB-90B1-F57BA349E55C}" });
            ScopeLists.Add(new Scope() { Name = "DataSet Editor", ID = "{B334A759-F450-40A5-BE2A-65937BCD5415}" });
            ScopeLists.Add(new Scope() { Name = "XAML Designer", ID = "{E9B8485C-1217-4277-9ED6-C825A5AC1968}" });
            ScopeLists.Add(new Scope() { Name = "{DEE22B65-9761-4A26-8FB2-759B971D6DFC}", ID = "{DEE22B65-9761-4A26-8FB2-759B971D6DFC}" });
            ScopeLists.Add(new Scope() { Name = "XAML Designer", ID = "{E9B8485C-1217-4277-9ED6-C825A5AC1968}" });
            ScopeLists.Add(new Scope() { Name = "View Designer", ID = "{B968E165-98E0-41F0-8FBE-A8ED1D246A90}" });
            ScopeLists.Add(new Scope() { Name = "XAML Editor", ID = "{A4F9FF65-A78C-4650-866D-5069CC4127CF}" });
            ScopeLists.Add(new Scope() { Name = "Microsoft SQL Server Data Tools, Table Designer", ID = "{2366CF66-2394-4701-AFCA-37ED7FD41648}" });
            ScopeLists.Add(new Scope() { Name = "Microsoft Visual Basic Editor", ID = "{2C015C70-C72C-11D0-88C3-00A0C9110049}" });
            ScopeLists.Add(new Scope() { Name = "Global", ID = "{5EFC7975-14BC-11CF-9B2B-00AA00573819}" });
            ScopeLists.Add(new Scope() { Name = "HTML Editor", ID = "{40D31677-CBC0-4297-A9EF-89D907823A98}" });
            ScopeLists.Add(new Scope() { Name = "Live Visual Tree", ID = "{A2EAF38F-A0AD-4503-91F8-5F004A69A040}" });
            ScopeLists.Add(new Scope() { Name = "Work Item Query View", ID = "{B6303490-B828-410C-9216-AE727D0E282D}" });
            ScopeLists.Add(new Scope() { Name = "DOM Explorer", ID = "{01E50993-E6A4-491E-AB3F-3A9939DE3524}" });
            ScopeLists.Add(new Scope() { Name = "Team Foundation Build Detail Editor", ID = "{86306A97-84F2-4F5A-889B-1318501AEB5F}" });
            ScopeLists.Add(new Scope() { Name = "Database Designer", ID = "{CFF78A9B-78A3-45A3-9142-0267AFC261FA}" });
            ScopeLists.Add(new Scope() { Name = "Work Item Editor", ID = "{40A91D9D-8076-4D28-87C5-5AF9F0ACFE0F}" });
            ScopeLists.Add(new Scope() { Name = "JSON Editor", ID = "{90A6B3A7-C1A3-4009-A288-E2FF89E96FA0}" });
            ScopeLists.Add(new Scope() { Name = "CSharp Editor", ID = "{A6C744A8-0E4A-4FC6-886A-064283054674}" });
            ScopeLists.Add(new Scope() { Name = "VC String Editor", ID = "{58442DA9-10DA-4AA9-A2AF-96E4D481379B}" });
            ScopeLists.Add(new Scope() { Name = "Interactive Window", ID = "{2D0A56AA-9527-4B78-B6E6-EBE6E05DA749}" });
            ScopeLists.Add(new Scope() { Name = "Merge Editor Window", ID = "{9A9A8AAA-ACD2-4DB6-BD81-8D64176C52B6}" });
            ScopeLists.Add(new Scope() { Name = "Microsoft Visual Basic Code Page Editor", ID = "{6C33E1AA-1401-4536-AB67-0E21E6E569DA}" });
            ScopeLists.Add(new Scope() { Name = "Microsoft Visual Basic Code Page Editor", ID = "{6C33E1AA-1401-4536-AB67-0E21E6E569DA}" });
            ScopeLists.Add(new Scope() { Name = "Settings Designer", ID = "{515231AD-C9DC-4AA3-808F-E1B65E72081C}" });
            ScopeLists.Add(new Scope() { Name = "Windows Forms Designer", ID = "{BA09E2AF-9DF2-4068-B2F0-4C7E5CC19E2F}" });
            ScopeLists.Add(new Scope() { Name = "Markdown Editor", ID = "{B3984FB3-6A50-488A-A8A5-1EA6929ADF43}" });
            ScopeLists.Add(new Scope() { Name = "VC Accelerator Editor", ID = "{EB56D0B5-BEE7-4D0C-8BE6-88A8ED256695}" });
            ScopeLists.Add(new Scope() { Name = "Managed Resources Editor", ID = "{FEA4DCC9-3645-44CD-92E7-84B55A16465C}" });
            ScopeLists.Add(new Scope() { Name = "FSharpEditorFactory", ID = "{8A5AA6CF-46E3-4520-A70A-7393D15233E9}" });
            ScopeLists.Add(new Scope() { Name = "JavaScript Console", ID = "{074F1AD9-308B-488C-861E-CF88182E2788}" });
            ScopeLists.Add(new Scope() { Name = "Difference Viewer", ID = "{79D52DDF-52BC-43F1-9663-B3E85CDCA912}" });
            ScopeLists.Add(new Scope() { Name = "Microsoft SQL Server Data Tools, Schema Compare", ID = "{00E9D2E3-9044-47D1-978C-BE2825249219}" });
            ScopeLists.Add(new Scope() { Name = "Table Designer", ID = "{4194FEE5-6777-419F-A5FC-47A536DF1BDB}" });
            ScopeLists.Add(new Scope() { Name = "HTML Editor Design View", ID = "{CB3FCFEA-03DF-11D1-81D2-00A0C91BBEE3}" });
            ScopeLists.Add(new Scope() { Name = "HTML Editor Source View", ID = "{CB3FCFEB-03DF-11D1-81D2-00A0C91BBEE3}" });
            ScopeLists.Add(new Scope() { Name = "VC Image Editor", ID = "{C0BA70ED-069E-412B-9C06-7442E28A11B9}" });
            ScopeLists.Add(new Scope() { Name = "Microsoft SQL Server Data Tools, T-SQL Editor", ID = "{CC5D8DF0-88F4-4BB2-9DBB-B48CEE65C30A}" });
            ScopeLists.Add(new Scope() { Name = "Query Results", ID = "{0058A1F7-65F3-4DB9-B3D0-CA7E64DD73CD}" });
            ScopeLists.Add(new Scope() { Name = "Test Explorer", ID = "{E1B7D1F8-9B3C-49B1-8F4F-BFC63A88835D}" });
            ScopeLists.Add(new Scope() { Name = "XML Schema Designer", ID = "{DEE6CEF9-3BCA-449A-82A6-FC757D6956FB}" });

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
    public class Scope
    {
        public string Name { get; set; }

        public string ID { get; set; }
    }
    public class ShortcutKeyCommand : INotifyPropertyChanged
    {
        #region Private Fields
        public event PropertyChangedEventHandler PropertyChanged;
        private string _DispalyName;
        #endregion

        public string DispalyName
        {
            get { return _DispalyName; }
            set
            {
                _DispalyName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DispalyName"));
            }
        }
        public string DisplaySymbol { get; set; }



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
