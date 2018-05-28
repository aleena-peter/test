using System.Collections.Generic;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class Rel {
    public string Name { get; set; }
    public List<string> Columns { get; }
    public Dictionary<string, List<string>> Rows { get; }

    public Rel( string name ) {
      Name = name;
      Rows = new Dictionary<string, List<string>>();
      Columns = new List<string>();
    }
  }
}