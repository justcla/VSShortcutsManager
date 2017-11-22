using System;
using System.ComponentModel;
using System.IO;
using System.Windows;


namespace VSShortcutsManager
{

    public class ShortcutsImportViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _importPath;
        public string ImportPath
        {
            get { return _importPath; }
            set
            {
                if (!string.Equals(_importPath, value, StringComparison.Ordinal))
                {
                    _importPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImportPath)));
                }
            }
        }

        //public ICommand BrowseButtonClick
        //{
        //    get { return () =>{
        //        FilePickerDialog dialog(ImportPath);
        //        if(dialog.Run())
        //        {
        //            ImportPath = dialog.FilePath;
        //        }
                
        //    }
        //}

    }

    public partial class ShortcutsImportDialog : Window
    {
        private ShortcutsImportViewModel viewModel;

        public ShortcutsImportDialog(ShortcutsImportViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "VS settings files (*.vssettings)|*.vssettings|XML files (*.xml)|*.xml|All files (*.*)|*.*";
            if (File.Exists(viewModel.ImportPath)) {
                fileDialog.InitialDirectory = Path.GetDirectoryName(viewModel.ImportPath);
            }
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.viewModel.ImportPath = fileDialog.FileName;
            }
        }

        void OnOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class ShortcutsImport
    {
        public static bool ImportShortcuts(ref string defaultPath)
        {
            ShortcutsImportViewModel viewModel = new ShortcutsImportViewModel();
            viewModel.ImportPath = defaultPath;

            ShortcutsImportDialog dlg = new ShortcutsImportDialog(viewModel);
            dlg.DataContext = viewModel;

            bool? wasOK = dlg.ShowDialog();
            defaultPath = viewModel.ImportPath;
            return wasOK.HasValue && wasOK.Equals(true);
        }
    }
}
