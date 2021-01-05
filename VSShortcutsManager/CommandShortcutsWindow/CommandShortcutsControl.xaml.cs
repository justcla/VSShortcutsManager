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

            this.contentControl.Content = new CommandShortcutsTree();
        }

    }

}
