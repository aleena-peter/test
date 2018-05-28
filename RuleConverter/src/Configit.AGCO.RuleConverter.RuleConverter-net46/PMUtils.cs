using System.Collections.Generic;
using System.Linq;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter_net46 {
  public static class PMUtils {
    public static Dictionary<string, VariableInfo> GetVariables( PMGroup group,
      Dictionary<string, VariableInfo> variables = null,
      string groupName = "" ) {
      if ( variables == null )
        variables = new Dictionary<string, VariableInfo>();
      // variables frm linked models don't have a name, we need to skip them
      foreach ( var variable in group.Variables.Where( v => v.Name != null ) ) {
        var variableFullName = string.IsNullOrEmpty( groupName ) ? $"{variable.Name}" : $"{groupName}.{variable.Name}";
        variables.Add( variable.Name, new VariableInfo( variableFullName, variable ) );
      }
      foreach ( var subGroup in group.Groups ) {
        var subGroupName = string.IsNullOrEmpty( groupName ) ? $"{subGroup.Name}" : $"{groupName}.{subGroup.Name}";
        GetVariables( subGroup, variables, subGroupName );
      }
      return variables;
    }

    public static int DeleteVariables( PMGroup pm, string variableName) {
      var count = pm.Variables.RemoveAll( v => v.Name == variableName );
      foreach ( var grp in pm.Groups ) {
        count += DeleteVariables( grp, variableName );
      }
      return count;
    }

    public static Relation ToRelation( Rel rel ) {
      var relation = new Relation( rel.Name, null, EnabledFlag.False );
      foreach ( var col in rel.Columns )
        relation.RelationColumns.Add( new RelationColumn( col ) );
      foreach ( var relRow in rel.Rows )
        relation.AddRelationRow( relRow.Value.ToArray() );
      return relation;
    }
  }
}