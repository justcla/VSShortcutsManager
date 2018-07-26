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
            var selectedscope = (Scope)cmbScopeList.SelectedItem;
            Guid scopeGuid = Guid.Parse(selectedscope.ID);     // Get Guid Scope
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

}
