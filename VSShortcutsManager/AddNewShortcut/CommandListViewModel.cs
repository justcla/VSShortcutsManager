using System;
using System.Collections.Generic;
using System.Linq;

namespace VSShortcutsManager.AddNewShortcut
{
    public class CommandListViewModel
    {
        private List<CommandList> data;

        public CommandListViewModel(IEnumerable<Command> allCommands)
        {
            this.data = GetCommandDisplayData(allCommands);
        }

        private static List<CommandList> GetCommandDisplayData(IEnumerable<Command> allCommands)
        {
            List<CommandList> displayData = new List<CommandList>();
            foreach (Command eachCommand in allCommands)
            {
                // Filter out commands with no name
                if (string.IsNullOrEmpty(eachCommand.CanonicalName))
                {
                    continue;
                }

                // Add the display item to the list
                CommandList item = new CommandList()
                {
                    DisplayName = eachCommand.DisplayName,
                    CommandName = eachCommand.CanonicalName
                };
                displayData.Add(item);
            }

            return displayData;
        }

        public IEnumerable<CommandList> DataSource => data.OrderBy(o => o.CommandName);
    }

    public class CommandList
    {
        public string DisplayName { get; set; }
        public string CommandName { get; set; }
    }

}
