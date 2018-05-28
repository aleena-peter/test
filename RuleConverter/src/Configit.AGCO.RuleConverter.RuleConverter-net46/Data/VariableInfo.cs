using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class VariableInfo {
    public string Name => Variable.Name;
    public string FullyQualifiedName { get; }
    public Variable Variable { get; }

    public VariableInfo( string fullyQualifiedName, Variable variable ) {
      FullyQualifiedName = fullyQualifiedName;
      Variable = variable;
    }
  }
}
