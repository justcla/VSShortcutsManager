using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VSShortcutsManager
{
    public class CommandShortcutsControlDataContext : INotifyPropertyChanged
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

        #endregion // Public methods

        #region INotifyPropertyChanged

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // INotifyPropertyChanged

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
                .Where(chord => chord != null)
                .Select(chord => GenerateBindingSequenceText(chord))
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
                buffer.Append("Control+");
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

        #endregion // Private helpers

        #region Fields

        private VSCommandShortcuts allCommands;
        private VSCommandShortcuts commands;
        private IServiceProvider serviceProvider;

        #endregion // Fields
    }

    public class CommandShortcut
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
        public CommandBinding Binding { get; private set; }
        public string ShortcutText { get; private set; }
        public string ScopeText { get; private set; }
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
}