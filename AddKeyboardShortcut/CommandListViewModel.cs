using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AddKeyboardShortcut {
  public class CommandListViewModel {

    private List<CommandList> data;

    public CommandListViewModel() {
      data = new List<CommandList>();
      string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      var itemsInMyDocuments = Directory.GetDirectories(userFolder).Concat(Directory.GetFiles(userFolder));
      foreach (var item in itemsInMyDocuments) {
        data.Add(new CommandList() { Name = item });
      }
    }

    public IEnumerable<CommandList> DataSource {
      get { return data; }
    }
  }

  public class CommandList {
    public string Name { get; set; }
  }

}
