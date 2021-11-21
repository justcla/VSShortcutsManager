//using CommandTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandTable
{
    internal interface ICommandTable
    {
        //IEnumerable<Command> GetCommands();
        Task<IEnumerable<Command>> GetCommands();
    }
}