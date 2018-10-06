using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;

namespace VSShortcutsManager
{
    /// <summary>
    /// Interaction logic for ImportShortcuts.xaml
    /// </summary>
    public partial class ImportShortcuts : DialogWindow
    {
        private readonly string chosenFile;
        private readonly List<VSShortcut> shortcuts;
        private XDocument vsSettingsXDoc;

        public bool isCancelled { get; private set; }

        public ImportShortcuts(string chosenFile, XDocument vsSettingsXDoc, List<VSShortcut> shortcuts)
        {
            // Store the input parameters
            this.chosenFile = chosenFile;
            this.vsSettingsXDoc = vsSettingsXDoc;
            this.shortcuts = shortcuts;

            // Convert shortcut objects to UI objects
            var shortcutUIs = new List<VSShortcutUI>();
            foreach (var shortcut in shortcuts)
            {
                shortcutUIs.Add(new VSShortcutUI(true, shortcut, setSelectAllFalse));
            }

            var importShortcutsDataModel = new ImportShortcutsDataModel(shortcutUIs);
            this.DataContext = importShortcutsDataModel;
            InitializeComponent();
            ((FrameworkElement)this.Resources["ProxyElement"]).DataContext = importShortcutsDataModel;
        }

        public class ImportShortcutsDataModel : INotifyPropertyChanged
        {
            private bool isSelectAll { get; set; }
            private List<VSShortcutUI> vSShortcutUIs { get; }

            public ImportShortcutsDataModel(List<VSShortcutUI> vSShortcutUIs)
            {
                this.isSelectAll = true;
                this.vSShortcutUIs = vSShortcutUIs;
            }

            public List<VSShortcutUI> VSShortcutUIs
            {
                get
                {
                    return this.vSShortcutUIs;
                }
            }

            public bool IsSelectAll
            {
                get
                {
                    return this.isSelectAll;
                }

                set
                {
                    setSelectAll(value, true);
                }
            }

            public void setSelectAll(bool state, bool synchronizeChildren)
            {
                if (synchronizeChildren)
                {
                    SynchronizeChildren(state);
                }
                this.isSelectAll = state;
                OnPropertyChanged("IsSelectAll");
            }

            private void SynchronizeChildren(bool state)
            {
                foreach (var shortcut in this.vSShortcutUIs)
                {
                    shortcut.Included = state;
                    shortcut.OnPropertyChanged();
                }
            }

            public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class VSShortcutUI : INotifyPropertyChanged
        {
            public bool isIncluded { private set; get; }
            public VSShortcut vsShortcut { get; }
            private Action setSelectAllFalse;

            public bool Included
            {
                get
                {
                    return this.isIncluded;
                }
                set
                {
                    this.isIncluded = value;
                    if (!value)
                    {
                        setSelectAllFalse();
                    }
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

            public IEnumerable<string> Conflicts
            {
                get
                {
                    return this.vsShortcut.Conflicts;
                }
            }

            public VSShortcutUI(bool included, VSShortcut shortcut, Action setSelectAllFalse)
            {
                this.isIncluded = included;
                this.vsShortcut = shortcut;
                this.setSelectAllFalse = setSelectAllFalse;
            }

            public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.isCancelled = false;

            bool success = VSShortcutsManager.Instance.PerformImportUserShortcuts(chosenFile, vsSettingsXDoc, this);

            // Close the window if shortcuts successfully imported.
            if (success) Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.isCancelled = true;
            Close();
        }

        private void setSelectAllFalse()
        {
            ((ImportShortcutsDataModel)this.DataContext).setSelectAll(false, false);
        }

        public List<VSShortcut> GetUncheckedShortcuts()
        {
            var shortcuts = new List<VSShortcut>();
            foreach (var shortcut in ((ImportShortcutsDataModel)this.DataContext).VSShortcutUIs)
            {
                if (!shortcut.Included)
                {
                    shortcuts.Add(shortcut.vsShortcut);
                }
            }
            return shortcuts;
        }
    }
}
