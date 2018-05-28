using System;
using System.Collections.Generic;
using System.Text;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class ExtractedRule {
    public ExtractedRule( string counter ) {
      Counter = counter;
    }

    public string Counter { get; }
    public Assignment InAssignment { get; set; }
    public Assignment ConditionAssignment { get; set; }
    public string HierarchyElement { get; set; }
    public string DependencyType { get; set; }
    public List<Assignment> OutAssignments { get; set; }
  }
}
