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

            this.allCommands = GenerateAllCommandsShortcuts();

            this.Commands = this.allCommands.Clone();
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

        private VSCommandShortcuts GenerateAllCommandsShortcuts()
        {
            var queryEngine = new VSShortcutQueryEngine(this.serviceProvider);

            var allCommands = queryEngine
                .AllCommands
                .Where(command => !string.IsNullOrWhiteSpace(command?.Text))
                .SelectMany(command => GenerateCommandsShortcuts(command));

            //var commands = new VSCommandShortcuts
            //{
            //    new CommandShortcut(null, "Best Action", null, "Ctrl+A", "Global"),
            //    new CommandShortcut(null, "Second Best Action", null, "Ctrl+A", "Some Scope"),
            //    new CommandShortcut(null, "Another Best Action", null, "Ctrl+Z", "Global"),
            //    new CommandShortcut(null, "Great Action", null, "Ctrl+Shift+A", "Global"),
            //    new CommandShortcut(null, "Another Great Action", null, "Ctrl+Shift+Z", "Global")
            //};

            var commands = new VSCommandShortcuts(allCommands);

            return commands;
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
                shortcutText = GenerateShortcutText(binding.Chords);
                scopeText = GenerateScopeText(binding.Scope);
            }

            return new CommandShortcut(command.Id, command.Text, binding, shortcutText, scopeText);
        }

        private static string GenerateShortcutText(IEnumerable<BindingSequence> chords)
        {
            if (chords == null || !chords.Any())
            {
                return null;
            }

            string s = chords
                .Where(chord => chord != null)
                .Select(chord => GenerateBindingSequenceText(chord))
                .Aggregate((current, next) => current + ", " + next);

            return s;
        }

        private static string GenerateBindingSequenceText(BindingSequence bindingSequence)
        {
            if (bindingSequence.Modifiers == System.Windows.Input.ModifierKeys.None)
            {
                return bindingSequence.Chord;
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
            buffer.Append(bindingSequence.Chord);

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