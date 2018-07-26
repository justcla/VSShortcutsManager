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
    public class ScopeListViewModel
    {

        private List<ScopeList> data;

        //Done for Sample. to be replaced with acutal code with list of all commands
        public ScopeListViewModel(IServiceProvider serviceProvider)
        {

            data = new List<ScopeList>();
            VSShortcutQueryEngine queryEngine = new VSShortcutQueryEngine(serviceProvider);
            var allCommands = queryEngine.GetAllBindingScopesAsync().Result;
            foreach (var eachCommand in allCommands)
            {
                data.Add(new ScopeList() { Name = eachCommand.Name, Guid = eachCommand.Guid.ToString() });
            }

        }


        public IEnumerable<ScopeList> DataSource
        {
            get { return data; }
        }
    }

    public class ScopeList
    {
        public string Name { get; set; }
        public string Guid { get; set; }
    }

}
