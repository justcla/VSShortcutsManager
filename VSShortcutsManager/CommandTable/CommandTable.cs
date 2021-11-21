using CommandTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandTable
{
    class CommandTable : ICommandTable
    {
        public async Task<IEnumerable<Command>> GetCommands()
        {
            var commandList = new List<Command>();
            AddItemToList(commandList, new Guid(), 1, "Duplicate Code");
            AddItemToList(commandList, new Guid(), 2, "Expand Selection");
            AddItemToList(commandList, new Guid(), 3, "Reduce Selection");
            AddItemToList(commandList, new Guid(), 4, "Complete Statement");
            return commandList;
        }

        private static void AddItemToList(List<Command> commandList, Guid guid, int dWord, string buttonText, string commandWellText = "")
        {
            if (string.IsNullOrWhiteSpace(commandWellText))
            {
                commandWellText = buttonText;
            }
            ItemText itemText = new ItemText(buttonText, commandWellText);
            Command item = new Command(itemText, new ItemId(guid, dWord));
            commandList.Add(item);
        }

    }

}