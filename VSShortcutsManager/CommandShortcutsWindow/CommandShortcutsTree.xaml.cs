using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace VSShortcutsManager.CommandTreeView
{
    /// <summary>
    /// Interaction logic for CommandShortcutsTree.xaml
    /// </summary>
    public partial class CommandShortcutsTree : UserControl
    {
        public CommandShortcutsTree()
        {
            InitializeComponent();
        }

        private void InitializeViews()
        {
            List<CommandGroup> commandsData;
            commandsData = GetDummyCommandShortcutData();
            // Set the item source (Default to all commands)
            trvCommands.ItemsSource = commandsData;
        }

        public IEnumerable Source
        {
            get => trvCommands.ItemsSource;
            set => trvCommands.ItemsSource = value;
        }

        private static List<CommandGroup> GetDummyCommandShortcutData()
        {
            Dictionary<string, List<string>> shortcutGroup1 = new Dictionary<string, List<string>>();
            shortcutGroup1.Add("Text Editor", new List<string> { "Ctrl+N", "Ctrl+Shift+N" });
            shortcutGroup1.Add("Global", new List<string> { "Ctrl+O" });
            Dictionary<string, List<string>> shortcutGroup2 = new Dictionary<string, List<string>>();
            shortcutGroup2.Add("Text Editor", new List<string> { "Ctrl+R,Ctrl+R", "Ctrl+R,R" });
            Dictionary<string, List<string>> editUndo = new Dictionary<string, List<string>>();
            editUndo.Add("Global", new List<string> { "Ctrl+Z", "Alt+Backspace" });
            Dictionary<string, List<string>> editRedo = new Dictionary<string, List<string>>();
            editRedo.Add("Global", new List<string> { "Ctrl+Y", "Ctrl+Shift+Z", "Alt+Backspace" });

            List<CommandGroup> allCommands = new List<CommandGroup>();

            CommandGroup fileMenu = new CommandGroup() { GroupName = "File" };
            CommandGroup fileNewMenu = new CommandGroup() { GroupName = "New" };
            CommandGroup fileOpenMenu = new CommandGroup() { GroupName = "Open" };
            fileMenu.Add(fileNewMenu);
            fileMenu.Add(fileOpenMenu);
            fileNewMenu.Items.Add(new CommandItem() { CommandName = "File.NewProject", ShortcutGroup = shortcutGroup1 });
            fileOpenMenu.Items.Add(new CommandItem() { CommandName = "File.OpenFile", ShortcutGroup = shortcutGroup2 });
            allCommands.Add(fileMenu);


            CommandGroup editMenu = new CommandGroup() { GroupName = "Edit" };
            editMenu.Items.Add(new CommandItem() { CommandName = "Edit.Undo", ShortcutGroup = editUndo });
            editMenu.Items.Add(new CommandItem() { CommandName = "Edit.Redo", ShortcutGroup = editRedo });
            allCommands.Add(editMenu);
            return allCommands;
        }
    }
    public class CommandGroup
    {
        public CommandGroup()
        {
            this.Items = new ObservableCollection<object>();
        }

        public string GroupName { get; set; }

        public ObservableCollection<object> Items { get; set; }

        internal void Add(object item)
        {
            this.Items.Add(item);
        }
    }

    public class CommandItem
    {
        public string CommandName { get; set; }

        /// <summary>
        ///  Map[Scope => List of shortcut text combinations]
        /// </summary>
        public Dictionary<string, List<string>> ShortcutGroup { get; internal set; }
    }

}