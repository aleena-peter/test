using System;
using System.Collections.Generic;
using System.Linq;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.AGCO.Prototype.RuleConverter_net46;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class ParseUtils {

    private readonly Dictionary<string, string> Hierarchy;
    private Dictionary<string, VariableInfo> _variableLookup;

    public ParseUtils( Dictionary<string, string> hierarchy, Dictionary<string, VariableInfo> variableLookup ) {
      Hierarchy = hierarchy;
      _variableLookup = variableLookup;
    }
    public ParseUtils( Dictionary<string, string> hierarchy, ProductModel pm ) {
      Hierarchy = hierarchy;
      _variableLookup = PMUtils.GetVariables( pm.ProductModelStructure );
    }

    public bool ValueExists( string variable, string value ) {
      return _variableLookup.ContainsKey( variable );
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
      return _variableLookup[name].FullyQualifiedName;
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
