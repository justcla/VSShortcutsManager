using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VSShortcutsManager
{
    public class CommandShortcutsControlDataContext : INotifyPropertyChanged
    {
        public CommandShortcutsControlDataContext()
        {
            this.allCommands = GenerateAllCommandsShortcuts();

            this.Commands = this.allCommands.Clone();
        }

        public void ClearSearch()
        {
            this.Commands = this.allCommands.Clone();
        }

        public uint SearchCommands(string searchCriteria, bool matchCase = true)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria))
            {
                return 0;
            }

            var commandsFilter = new CommandsFilterFactory().GetCommandsFilter(searchCriteria, matchCase);

            this.Commands = commandsFilter.Filter(this.allCommands);

            return (uint)this.Commands.Count;
        }

        private static VSCommandShortcuts GenerateAllCommandsShortcuts()
        {
            var commands = new VSCommandShortcuts
            {
                new VSCommandShortcut("Best Action", "Ctrl+A", "Global"),
                new VSCommandShortcut("Second Best Action", "Ctrl+A", "Some Scope"),
                new VSCommandShortcut("Another Best Action", "Ctrl+Z", "Global"),
                new VSCommandShortcut("Great Action", "Ctrl+Shift+A", "Global"),
                new VSCommandShortcut("Another Great Action", "Ctrl+Shift+Z", "Global")
            };

            return commands;
        }

        public VSCommandShortcuts Commands
        {
            get { return commands; }

            private set
            {
                this.commands = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // INotifyPropertyChanged

        private VSCommandShortcuts commands;
        private VSCommandShortcuts allCommands;
    }

    public class VSCommandShortcut
    {
        public VSCommandShortcut(string command, string shortcut, string scope)
        {
            this.Command = command;
            this.Shortcut = shortcut;
            this.Scope = scope;
        }

        public string Command { get; private set; }
        public string Shortcut { get; private set; }
        public string Scope { get; private set; }
    }

    public class VSCommandShortcuts : List<VSCommandShortcut>
    {
        public VSCommandShortcuts()
            : base()
        { }

        public VSCommandShortcuts(IEnumerable<VSCommandShortcut> collection)
            : base(collection)
        { }

        public VSCommandShortcuts Clone()
        {
            // Shallow clone is enough
            return new VSCommandShortcuts(this);
        }
    }
}