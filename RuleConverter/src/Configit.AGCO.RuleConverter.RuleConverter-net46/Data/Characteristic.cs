using System.Collections.Generic;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class Characteristic {
    public string Name { get; }
    public List<string> Values { get; }

    public Characteristic( string name, List<string> values = null) {
      Name = name;
      Values = values ?? new List<string>();
    }
  }
}