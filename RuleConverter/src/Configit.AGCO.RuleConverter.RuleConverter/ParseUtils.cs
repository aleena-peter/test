using System;
using System.Collections.Generic;
using System.Linq;
using Configit.AGCO.Prototype.RuleConverter.Data;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class ParseUtils {

    private readonly Dictionary<string, string> Hierarchy;

    public ParseUtils( Dictionary<string, string> hierarchy ) {
      Hierarchy = hierarchy;
    }

    public bool ValueExists( string variable, string value) {
      return true; // TODO
      //var variables = pm.ProductModelStructure.Groups[0].Variables;
      //var variable = variables.FirstOrDefault(v => v.Name == variable.ToString());
      //return variable != null && variable.
      //return _variablesToValuesDictionary[variable.ToString()].Contains( value.ToString() );
    }

    /// <summary>
    /// The precondition makes sure the rule is only applied when the correct hierarchy element is selected
    /// </summary>
    /// <param name="characteristic"></param>
    /// <param name="selectionCriteria"></param>
    /// <returns></returns>
    public string GeneratePrecondition( string characteristic, string selectionCriteria ) {

      return GenerateEquals( characteristic, selectionCriteria );
    }

    /// <summary>
    /// The selection criteria can be any of the hierarchy elements (variant, version, model range, model).
    /// This method tries to find the charachteristic for the selection criteria.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string FindSelectionCriteriaCharacteristic( string value ) {
      if ( Hierarchy.TryGetValue( value, out var chraracteristic ) ) {
        return chraracteristic;
      }
      else {
        Console.WriteLine( $"Could not find hierarchy element {value} , skipping rule" );
        return null;
      }
    }


    public string GenerateEquals( string variable, string value ) {
      var variableName = variable.ToString();
      return $"[{GetVariableFullName( variableName )}] = \"{value}\"";
    }

    public string GetVariableFullName( string name ) {
      return $"General.{name}";
    }

    public string CreateExcludeExpression( List<Assignment> outChars ) {
      return outChars.Select( oc => GenerateNotAssignment( oc.Characteristic, oc.Value ) ).Aggregate( ( l, r ) => $"{l} or {r}" );
    }

    public string CreateIncludeExpression( List<Assignment> outChars ) {
      return outChars.Select( oc => GenerateEquals( oc.Characteristic, oc.Value ) ).Aggregate( ( l, r ) => $"{l} or {r}" );
      //return counter.Select( r => GenerateEquals( charOut, valueOut ) ).Aggregate( ( l, r ) => $"{l} or {r}" );
    }

    /// <summary>
    /// Some rules have two preconditions, this methods add the second precondition
    /// </summary>
    /// <param name="ifChacateristicValue"></param>
    /// <returns></returns>
    public string CreateSecondCondition( Assignment ifChacateristicValue ) {
      return GenerateEquals( ifChacateristicValue.Characteristic, ifChacateristicValue.Value );

    }

    public string GenerateNotAssignment( object variable, object value ) {
      return $"[{GetVariableFullName( variable.ToString() )}] <> \"{value}\"";
    }
  }
}
