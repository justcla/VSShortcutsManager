//using CommandTable;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandTable
{
    internal class CommandTableFactory
    {
        internal ICommandTable CreateCommandTableFromHost(IServiceProvider serviceProvider, object fromCTM)
        {
            ICommandTable commandTable = new CommandTable();
            return commandTable;
            //throw new NotImplementedException();
        }
    }
}