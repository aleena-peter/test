using System.Collections.Generic;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class Model {
    public Model() {
      Versions = new List<ModelVersion>();
    }

    public Model( string brand, string prodcutGroup, string modelRange, string name, string matnr, List<ModelVersion> versions = null ) {
      Brand = brand;
      ProdcutGroup = prodcutGroup;
      ModelRange = modelRange;
      Name = name;
      Matnr = matnr;
      Versions = versions ?? new List<ModelVersion>();
    }

    public string Brand { get; set; }
    public string ProdcutGroup { get; set; }
    public string ModelRange { get; set; }
    public string Name { get; set; }
    public string Matnr { get; set; }
    public IList<ModelVersion> Versions { get; }

  }
}
