using System;
using System.Collections.Generic;
using System.Text;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class DefaultAssignment {

    public string Model { get; }
    public string Variant { get; }
    public string Country { get; }
    public Assignment Assignment { get; }

    public DefaultAssignment( string model, string variant, string country, Assignment assignment ) {
      Model = model;
      Variant = variant;
      Country = country;
      Assignment = assignment;
    }


  }
}
