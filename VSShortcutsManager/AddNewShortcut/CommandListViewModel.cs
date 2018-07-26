﻿using System;
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
        public CommandListViewModel(IServiceProvider serviceProvider)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                data = new List<CommandList>();
                VSShortcutQueryEngine queryEngine = new VSShortcutQueryEngine(serviceProvider);
                var allCommands = await queryEngine.GetAllCommandsAsync();
                foreach (var eachCommand in allCommands)
                {
                    if (!string.IsNullOrEmpty(eachCommand.CanonicalName))
                    {
                        CommandList item = new CommandList() { DisplayName = eachCommand.DisplayName, CommandName = eachCommand.CanonicalName };
                        data.Add(item);
                    }
                }
            });
        }


        public IEnumerable<CommandList> DataSource => data.OrderBy(o => o.CommandName);
    }

    public class CommandList
    {
        public string DisplayName { get; set; }
        public string CommandName { get; set; }
    }

}
