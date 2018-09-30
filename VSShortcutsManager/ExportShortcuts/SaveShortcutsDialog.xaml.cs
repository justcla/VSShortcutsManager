using System.Windows;

namespace VSShortcutsManager
{

    public class SaveShortcuts
    {
        public static bool GetShortcutsName(ref string defaultName)
        {
            // Open dialog with default name
            SaveShortcutsViewModel viewModel = new SaveShortcutsViewModel(defaultName);
            bool? wasOK = new SaveShortcutsDialog(viewModel).ShowDialog();

            // Get value from model
            defaultName = viewModel.ShortcutsName;
            // Return success of dialog
            return wasOK.HasValue && wasOK.Equals(true);
        }
    }

    public partial class SaveShortcutsDialog : Window
    {

        private SaveShortcutsViewModel viewModel;

        public SaveShortcutsDialog(SaveShortcutsViewModel viewModel)
        {
            this.viewModel = viewModel;
            DataContext = this.viewModel;
            InitializeComponent();
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

    public class SaveShortcutsViewModel
    {
        public SaveShortcutsViewModel(string defaultName)
        {
            this.ShortcutsName = defaultName;
        }

        public string ShortcutsName { get; set; }
    }

}
