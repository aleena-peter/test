using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class Category {
    public string MaterialName { get; }
    public string Brand { get; set; }
    public string ProductGroup { get; set; }
    public string ModelRange { get; set; }
    public string Model { get; set; }
    public List<ModelVersion> Versions { get; set; }

    public Category(string materialName) {
      MaterialName = materialName;
      Versions = new List<ModelVersion>();
    }

  }
}
