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
        public bool isCancelled { get; private set; }

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
            public bool isIncluded { private set; get; }
            public VSShortcut vsShortcut { get; }

            public bool Included
            {
                get
                {
                    return this.isIncluded;
                }
                set
                {
                    this.isIncluded = value;
                    OnPropertyChanged();
                }
            }

            public string Command
            {
                get
                {
                    return this.vsShortcut.Command;
                }
            }

            public string Scope
            {
                get
                {
                    return this.vsShortcut.Scope;
                }
            }

            public string Shortcut
            {
                get
                {
                    return this.vsShortcut.Shortcut;
                }
            }

            public string Conflict
            {
                get
                {
                    return this.vsShortcut.Conflict;
                }
            }

            public VSShortcutUI(bool included, VSShortcut shortcut)
            {
                this.isIncluded = included;
                this.vsShortcut = shortcut;
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void OkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.isCancelled = false;
            Close();
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.isCancelled = true;
            Close();
        }

        public List<VSShortcut> GetUncheckedShortcuts()
        {
            var shortcuts = new List<VSShortcut>();
            foreach (var shortcut in (List<VSShortcutUI>) (this.DataContext))
            {
                if (!shortcut.isIncluded)
                {
                    shortcuts.Add(shortcut.vsShortcut);
                }
            }
            return shortcuts;
        }
    }
}
