
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class BasketParser {
    private readonly ParseUtils _parseUtils;
    public BasketParser( Dictionary<string, string> hierarchy, ProductModel pm ) {
      _parseUtils = new ParseUtils( hierarchy, pm );
    }

    public List<ConvertedRule> ConvertBaskets( List<Basket> baskets ) {
      var generateRules = new List<ConvertedRule>( baskets.Count );
      foreach ( var basket in baskets ) {
        var variable = basket.Package.Characteristic;
        var value = basket.Package.Value;
        if ( !_parseUtils.ValueExists( variable, value ) ) {
          System.Console.Error.WriteLine( $"Value {value} not found in {variable}, skipping rule on counter {basket.Counter}" );
          continue;
        }

        // create a precondition (if part) that includes the hierarchy and the IN assignment
        var characteristic = _parseUtils.FindSelectionCriteriaCharacteristic( basket.HierarchyElement );
        if ( characteristic == null ) {
          continue;
        }
        var precondition = _parseUtils.GeneratePrecondition( characteristic, basket.HierarchyElement );
        var condition = _parseUtils.GenerateEquals( variable, value );
        var expression = _parseUtils.CreateIncludeExpression( basket.IncludedElements );
        generateRules.Add( new ConvertedRule( $"basket_{basket.Counter}",
          $"if {precondition} and {condition} then {expression}" ) );
      }
      return generateRules;
    }
  }
}
