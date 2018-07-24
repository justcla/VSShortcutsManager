using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VSShortcutsManager
{
    /// <summary>
    /// Interaction logic for ImportShortcuts.xaml
    /// </summary>
    public partial class ImportShortcuts : DialogWindow
    {
        public ImportShortcuts(List<VSShortcut> shortcuts)
        {
            var shortcutsUI = new List<VSShortcutUI>();
            foreach (var shortcut in shortcuts)
            {
                shortcutsUI.Add(new VSShortcutUI(true, shortcut));
            }
            this.DataContext = shortcutsUI;
            InitializeComponent();
        }

        public class VSShortcutUI : INotifyPropertyChanged
        {
            private bool included { set; get; }
            private VSShortcut shortcut;

            public bool Included
            {
                get
                {
                    return this.included;
                }
                set
                {
                    this.included = value;
                    OnPropertyChanged();
                }
            }

            public string Command
            {
                get
                {
                    return this.shortcut.Command;
                }
            }

            public string Scope
            {
                get
                {
                    return this.shortcut.Scope;
                }
            }

            public string Shortcut
            {
                get
                {
                    return this.shortcut.Shortcut;
                }
            }

            public string Conflict
            {
                get
                {
                    return this.shortcut.Conflict;
                }
            }

            public VSShortcutUI(bool included, VSShortcut shortcut)
            {
                this.included = included;
                this.shortcut = shortcut;
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
