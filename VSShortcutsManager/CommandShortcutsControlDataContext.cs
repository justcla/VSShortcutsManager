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

            this.PopulateCommands(serviceProvider);
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

        public void DeleteShortcuts(IEnumerable<CommandShortcut> commandShortcuts)
        {
            if (!commandShortcuts.Where(command => command.Binding != null).Any())
            {
                return;
            }

            if (System.Windows.MessageBoxResult.Yes != MessageBox.Show("Are you sure you want to delete the selected shortcuts", "Deleting shortcuts", System.Windows.MessageBoxButton.YesNo))
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
                var commandShortcut = kvp.Key;
                var dteCommand = dte.Commands.Item(commandShortcut.CommandText);
                if (dteCommand == null)
                {
                    Debug.WriteLine("Could not find command '{0}' in the DTE Commands.", kvp.Key.CommandText);
                    continue;
                }

                DeleteCommandBindings(dteCommand, kvp.Value);
                commandShortcut.Binding = null;
                commandShortcut.ShortcutText = null;
                commandShortcut.ScopeText = null;
            }
        }

        #endregion // Public methods

        #region Private helpers

        private void PopulateCommands(IServiceProvider serviceProvider)
        {
            var queryEngine = new VSShortcutQueryEngine(serviceProvider);

            queryEngine.GetAllCommandsAsync().ContinueWith((task) =>
            {
                var allCommands = task.Result;

                var allCommandShortcuts = allCommands
                    .Where(command => !string.IsNullOrWhiteSpace(command?.CanonicalName))
                    .SelectMany(command => GenerateCommandsShortcuts(command));
                
                this.allCommands = new VSCommandShortcuts(allCommandShortcuts);
                this.Commands = this.allCommands.Clone();
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
            if (bindingSequence.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt))
            {
                buffer.Append("Alt+");
            }
            if (bindingSequence.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            {
                buffer.Append("Shift+");
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
            var deletedBindingsSet = new HashSet<string>(deletedBindings.Select(binding => binding.DteBindingString));

            var oldBindings = (object[])command.Bindings;

            var newBindings = oldBindings
                .Where(bindingText => !deletedBindingsSet.Contains(bindingText.ToString()))
                .ToArray();

            command.Bindings = new object[0]; // unexplained workaround - resetting a binding array requires resetting it to an empty array first.
            command.Bindings = newBindings;
        }

        #endregion // Private helpers

        #region Fields

        private VSCommandShortcuts allCommands;
        private VSCommandShortcuts commands;
        private IServiceProvider serviceProvider;

        #endregion // Fields
    }

    public class CommandShortcut : NotifyPropertyChangedBase
    {
        public CommandShortcut(CommandId id, string commandText, CommandBinding binding, string shortcutText, string scopeText)
        {
            this.Id = id;
            this.CommandText = commandText;
            this.Binding = binding;
            this.ShortcutText = shortcutText;
            this.ScopeText = scopeText;
        }

        public CommandId Id { get; private set; }

        public string CommandText { get; private set; }

        public CommandBinding Binding { get; set; }

        public string ShortcutText
        {
            get { return shortcutText; }

            set
            {
                this.shortcutText = value;
                OnPropertyChanged();
            }
        }
        private string shortcutText;

        public string ScopeText
        {
            get { return scopeText; }

            set
            {
                this.scopeText = value;
                OnPropertyChanged();
            }
        }
        private string scopeText;
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