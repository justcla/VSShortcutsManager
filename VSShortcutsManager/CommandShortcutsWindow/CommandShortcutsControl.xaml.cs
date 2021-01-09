using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace VSShortcutsManager
{
    /// <summary>
    /// Interaction logic for CommandShortcutsControl.xaml
    /// </summary>
    public partial class CommandShortcutsControl : UserControl
    {
        public CommandShortcutsControl()
        {
            InitializeComponent();

            // Set the default initial opening layout and data
            // Runs only once per session - at the time of first opening the Command Shortcuts tool window
            // Note: This code should live somewhere else - as it will need to be called when flipping between tree and list views
            this.contentControl.Content = new CommandTreeView.CommandShortcutsTree();
            // The CommandShortcutsTree should be stored in local memory to easily flip back.
        }

    }

}
