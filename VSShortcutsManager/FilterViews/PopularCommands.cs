using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSShortcutsManager
{
    public class PopularCommands
    {
        internal static List<string> GetPopularCommandNames()
        {
            var popularCmdNames = new List<string>();
            popularCmdNames.Add("Edit.Undo");
            popularCmdNames.Add("Edit.Redo");
            popularCmdNames.Add("Edit.Copy");
            popularCmdNames.Add("Edit.Cut");
            popularCmdNames.Add("Edit.Paste");
            return popularCmdNames;
        }
    }
}
