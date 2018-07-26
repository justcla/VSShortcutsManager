using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EnvDTE;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
namespace VSShortcutsManager.AddNewShortcut
{
    public class CommandListViewModel
    {

        private List<CommandList> data;

        //Done for Sample. to be replaced with acutal code with list of all commands
        public CommandListViewModel()
        {

            

        }

        public IEnumerable<CommandList> DataSource(IServiceProvider serviceProvider)
        {
            data = new List<CommandList>();
            VSShortcutQueryEngine queryEngine = new VSShortcutQueryEngine(serviceProvider);
            var allCommands = queryEngine.GetAllBindingScopesAsync().Result;
            foreach (var eachCommand in allCommands)
            {
                data.Add(new CommandList() { Name = eachCommand.Name });
            }
            return data;
        }
      
    }

    public class CommandList
    {
        public string Name { get; set; }
    }

}
