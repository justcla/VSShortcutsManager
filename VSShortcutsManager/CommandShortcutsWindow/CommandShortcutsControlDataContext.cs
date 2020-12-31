using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using EnvDTE;

namespace VSShortcutsManager
{
    public class CommandShortcutsControlDataContext : NotifyPropertyChangedBase
    {
        #region ctor

        public CommandShortcutsControlDataContext(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            this.queryEngine = VSShortcutQueryEngine.GetInstance(this.serviceProvider);

            this.PopulateCommands();
        }

        #endregion // ctor

        #region Public properties
        public VSCommandShortcuts Commands
        {
            get { return commands; }

            private set
            {
                this.commands = value;
                OnPropertyChanged();
            }
        }

        #endregion // Public properties

        #region Public methods

        public void ClearSearch()
        {
            this.Commands = this.allCommandsCache.Clone();
        }

        public void RefreshView()
        {
            // Update local cache with fresh copy of commandShortcuts from the QueryEngine
            PopulateCommands();

            // Update the Commands object to get the view to refresh
            // Being done inside PopulateCommand method. Consider moving that out.
            //this.Commands = this.allCommandsCache.Clone();
        }

        public uint SearchCommands(string searchCriteria, bool matchCase = true)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria))
            {
                return 0;
            }

            var commandsFilter = new CommandsFilterFactory().GetCommandsFilter(searchCriteria, matchCase);

            this.Commands = commandsFilter.Filter(this.allCommandsCache);

            return (uint)this.Commands.Count;
        }

        public void DeleteShortcuts(IEnumerable<CommandShortcut> commandShortcuts)
        {
            int count = commandShortcuts.Where(command => command.Binding != null).Count();
            if (count < 1)
            {
                return;
            }

            string msg = count == 1
                ? "Are you sure you want to delete the selected shortcut?"
                : "Are you sure you want to delete the selected shortcuts?";
            if (System.Windows.MessageBoxResult.Yes != MessageBox.Show(msg, "Deleting shortcuts", System.Windows.MessageBoxButton.YesNo))
            {
                return;
            }

            var commands = commandShortcuts
                .Where(command => command.Binding != null)
                .GroupBy(k => k, v => v.Binding)
                .ToDictionary(k => k.Key, v => v.ToList());

            DTE dte = (DTE)this.serviceProvider.GetService(typeof(DTE));

            foreach (var kvp in commands)
            {
                // Find the command in the DTE command table
                CommandShortcut commandShortcut = kvp.Key;
                var dteCommand = dte.Commands.Item(commandShortcut.CommandText);
                if (dteCommand == null)
                {
                    Debug.WriteLine("Could not find command '{0}' in the DTE Commands.", kvp.Key.CommandText);
                    continue;
                }

                // Delete the shortcut from the command table (via DTE)
                DeleteCommandBindings(dteCommand, kvp.Value);

                // Update the model object for changes to reflect on the view
                commandShortcut.IsRemoved = true;
                commandShortcut.Binding = null;
                commandShortcut.ShortcutText = null;
                commandShortcut.ScopeText = null;
            }
        }

        #endregion // Public methods

        #region Private helpers

        private void PopulateCommands()
        {
            queryEngine.GetAllCommandsAsync().ContinueWith((task) =>
            {
                // Pull all command shortcuts into a temporary var
                IEnumerable<Command> allCommands = task.Result;

                // Remove all commands with no valid name
                // and convert them to the CommandShortcut objects
                IEnumerable<CommandShortcut> allCommandShortcuts = allCommands
                    .Where(command => !string.IsNullOrWhiteSpace(command?.CanonicalName))
                    .SelectMany(command => GenerateCommandsShortcuts(command));

                // Update the local commands cache
                this.allCommandsCache = new VSCommandShortcuts(allCommandShortcuts);

                // Update the backing object on the Toolwindow control (with a clone of the cache)
                this.Commands = this.allCommandsCache.Clone();
            });

        }

        private static IEnumerable<CommandShortcut> GenerateCommandsShortcuts(Command command)
        {
            if (command.Bindings == null || !command.Bindings.Any())
            {
                yield return GenerateCommandShortcut(command, null);
            }

            foreach (var binding in command.Bindings)
            {
                yield return GenerateCommandShortcut(command, binding);
            }
        }

        private static CommandShortcut GenerateCommandShortcut(Command command, CommandBinding binding)
        {
            string shortcutText = null, scopeText = null;
            if (binding != null)
            {
                shortcutText = GenerateShortcutText(binding.Sequences);
                scopeText = GenerateScopeText(binding.Scope);
            }

            return new CommandShortcut(command.Id, command.CanonicalName, binding, shortcutText, scopeText);
        }

        private static string GenerateShortcutText(IEnumerable<BindingSequence> sequences)
        {
            if (sequences == null || !sequences.Any())
            {
                return null;
            }

            string s = sequences
                .Where(sequence => sequence != null)
                .Select(sequence => GenerateBindingSequenceText(sequence))
                .Aggregate((current, next) => current + ", " + next);

            return s;
        }

        private static string GenerateBindingSequenceText(BindingSequence bindingSequence)
        {
            if (bindingSequence.Modifiers == System.Windows.Input.ModifierKeys.None)
            {
                return bindingSequence.Key;
            }

            var buffer = new System.Text.StringBuilder();
            if (bindingSequence.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Windows))
            {
                buffer.Append("Windows+");
            }
            if (bindingSequence.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
            {
                buffer.Append("Ctrl+");
            }
            if (bindingSequence.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            {
                buffer.Append("Shift+");
            }
            if (bindingSequence.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt))
            {
                buffer.Append("Alt+");
            }
            buffer.Append(bindingSequence.Key);

            string s = buffer.ToString();

            return s;
        }

        private static string GenerateScopeText(KeybindingScope scope)
        {
            if (string.IsNullOrWhiteSpace(scope?.Name))
            {
                return null;
            }

            return scope.Name;
        }

        private static void DeleteCommandBindings(EnvDTE.Command command, List<CommandBinding> deletedBindings)
        {
            HashSet<string> deletedBindingsSet = new HashSet<string>(deletedBindings.Select(binding => string.Concat(binding.Scope.Name, "::", binding.OriginalDTEString)));

            object[] oldBindings = (object[])command.Bindings;

            var newBindings = oldBindings
                .Where(bindingText => !deletedBindingsSet.Contains(bindingText.ToString()))
                .ToArray();

            command.Bindings = newBindings;
        }

        #endregion // Private helpers

        #region Fields

        private VSCommandShortcuts allCommandsCache;    // This holds all 4000+ commands in the system. Updated in PopulateCommands()
        private VSCommandShortcuts commands;    // This is the object that is displayed on the window (the list of commands)
        private readonly IServiceProvider serviceProvider;
        private readonly VSShortcutQueryEngine queryEngine;

        internal void ApplyAllShortcutsFilter()
        {
            this.Commands = allCommandsCache.Clone();
        }

        internal void ApplyPopularShortcutsFilter()
        {
            this.Commands = GetPopularCommands();
        }

        private VSCommandShortcuts GetPopularCommands()
        {
            List<string> popularCmdNames = PopularCommands.CommandList;
            // Note: Since each command can have many shortcuts, there can be many shortcuts in allCommandsCache for each listed command
            IEnumerable<CommandShortcut> popularCmdShortcuts = allCommandsCache.Where(command => popularCmdNames.Contains(command.CommandText));
            return new VSCommandShortcuts(popularCmdShortcuts);
        }

        internal void ApplyUserShortcutsFilter(List<VSShortcut> userShortcuts)
        {
            // Convert VSShortcut objects to CommandShortcut object
            this.Commands = ConvertVSShortcutListToVSCommandShortcuts(userShortcuts, isUserShortcut: true);
        }

        private VSCommandShortcuts ConvertVSShortcutListToVSCommandShortcuts(List<VSShortcut> userShortcuts, bool isUserShortcut = false)
        {
            VSCommandShortcuts vsCmdShortcuts = new VSCommandShortcuts();

            foreach (VSShortcut userShortcut in userShortcuts)
            {
                var commandShortcut = new CommandShortcut()
                {
                    CommandText = userShortcut.Command,
                    ShortcutText = userShortcut.Shortcut,
                    ScopeText = userShortcut.Scope,
                    IsRemoved = userShortcut.Operation.Equals("Remove"),
                    IsUserShortcut = isUserShortcut
                };
                vsCmdShortcuts.Add(commandShortcut);
            }
            return vsCmdShortcuts;
        }

        #endregion // Fields
    }

    public class CommandShortcut : NotifyPropertyChangedBase
    {

        public CommandShortcut() { }

        public CommandShortcut(CommandId id, string commandText, CommandBinding binding, string shortcutText, string scopeText)
        {
            this.Id = id;
            this.CommandText = commandText;
            this.Binding = binding;
            this.ShortcutText = shortcutText;
            this.ScopeText = scopeText;
        }

        public CommandShortcut(string commandText, string shortcutText, string scopeText)
        {
            CommandText = commandText;
            ShortcutText = shortcutText;
            ScopeText = scopeText;
        }

        public CommandId Id { get; set; }

        public string CommandText { get; set; }

        public CommandBinding Binding { get; set; }

        private string shortcutText;
        public string ShortcutText
        {
            get { return shortcutText; }

            set
            {
                this.shortcutText = value;
                OnPropertyChanged();
            }
        }

        private string scopeText;
        public string ScopeText
        {
            get { return scopeText; }

            set
            {
                this.scopeText = value;
                OnPropertyChanged();
            }
        }

        private bool isRemoved = false;
        public bool IsRemoved
        {
            get { return isRemoved; }

            set
            {
                this.isRemoved = value;
                OnPropertyChanged();
            }
        }

        public bool IsUserShortcut { get; set; }

    }

    public class VSCommandShortcuts : List<CommandShortcut>
    {
        public VSCommandShortcuts()
            : base()
        { }

        public VSCommandShortcuts(IEnumerable<CommandShortcut> collection)
            : base(collection)
        { }

        public VSCommandShortcuts Clone()
        {
            // Shallow clone is enough
            return new VSCommandShortcuts(this);
        }
    }

    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}