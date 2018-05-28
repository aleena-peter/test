using System;
using System.Collections.Generic;
using System.Text;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class Basket {
    public Basket( string counter ) {
      Counter = counter;
    }

    public string Counter { get; }
    public Assignment Package { get; set; }
    public Assignment ConditionAssignment { get; set; }
    public string HierarchyElement { get; set; }
    public List<Assignment> IncludedElements { get; set; }
  }
}
