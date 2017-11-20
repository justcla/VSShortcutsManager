using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSShortcutsManager
{

    public class ShortcutsImportViewModel : INotifyPropertyChanged
    {
        private string _importPath;
        public string ImportPath
        {
            get { return _importPath; }
            set
            {
                if (!string.Equals(_importPath, value, StringComparison.Ordinal))
                {
                    _importPath = value;
                    PropertyChangedEventArgs pce = new PropertyChangedEventArgs(nameof(ImportPath));
                    if (PropertyChanged != null)
                    { PropertyChanged(this, pce); }
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ShortcutsImportDialog : Window
    {
        private ShortcutsImportViewModel viewModel;

        //public ShortcutsImportDialog()
        //{
        //    InitializeComponent();
        //}

        public ShortcutsImportDialog(ShortcutsImportViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
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
        public static bool ImportShortcuts(string defaultPath)
        {
            ShortcutsImportViewModel viewModel = new ShortcutsImportViewModel();
            viewModel.ImportPath = defaultPath;

            ShortcutsImportDialog dlg = new ShortcutsImportDialog(viewModel);
            dlg.DataContext = viewModel;

            bool? wasOK = dlg.ShowDialog();
            return wasOK != null && wasOK == true;
        }
    }
}
