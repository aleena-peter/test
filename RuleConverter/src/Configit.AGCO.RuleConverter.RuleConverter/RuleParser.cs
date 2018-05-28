using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class RuleParser {
    private readonly ParseUtils _parseUtils;

    public RuleParser( Dictionary<string, string> hierarchy ) {
      _parseUtils = new ParseUtils( hierarchy );
    }

    public List<ConvertedRule> ConvertRules( List<ExtractedRule> rules, ProductModel model ) {
      var generateRules = new List<ConvertedRule>( rules.Count );
      foreach ( var rule in rules ) {
        var variable = rule.InAssignment.Characteristic;
        var value = rule.InAssignment.Value;
        if ( !_parseUtils.ValueExists( variable, value ) ) {
          System.Console.Error.WriteLine( $"Value {value} not found in {variable}, skipping rule on counter {rule.Counter}" );
          continue;
        }

        // create a precondition (if part) that includes the hierarchy and the IN assignment
        var characteristic = _parseUtils.FindSelectionCriteriaCharacteristic( rule.HierarchyElement );
        if ( characteristic == null ) {
          continue;
        }
        var precondition = _parseUtils.GeneratePrecondition( characteristic, rule.HierarchyElement );
        var condition = _parseUtils.GenerateEquals( variable, value );
        string expression;

        // **** description received from AGCO
        //01 is include.
        //02 and 32 can be treated as the same as an exclude, if the input is specified / true.
        //03 is an include with 2 inputs.
        //04 excludes if the first 2 inputs are specified
        //05 is a bit tricky. If the first is specified and the second is not specified then the third is allowed / included.
        //06 is the exclude version of 05.
        //33 is if the first is not specified, and the second is, then the output or third value is excluded. 

        //01           R Std. rules types             +N.A.       +
        //02           R Std. rules types             +N.A.       -
        //03           R Std. rules types             + + +
        //04           R Std. rules types             + +-
        //05           R Std. rules types             +-+
        //06           R Std. rules types             +- -
        //32           R Std. rules types             +N.A.       -
        //33           R Std. rules types             -+-
        var dependencyType = rule.DependencyType;
        switch ( dependencyType ) {
          case "01":
            expression = _parseUtils.CreateIncludeExpression( rule.OutAssignments );
            break;
          case "02":
          case "32":
            expression = _parseUtils.CreateExcludeExpression( rule.OutAssignments );
            break;
          case "03":
            var condition2 = _parseUtils.CreateSecondCondition( rule.ConditionAssignment );
            condition = $"{condition} and {condition2}";
            expression = _parseUtils.CreateIncludeExpression( rule.OutAssignments );
            break;
          case "04":
            condition2 = _parseUtils.CreateSecondCondition( rule.ConditionAssignment );
            condition = $"{condition} and {condition2}";
            expression = _parseUtils.CreateExcludeExpression( rule.OutAssignments );
            break;
          case "05":
            condition2 = _parseUtils.CreateSecondCondition( rule.ConditionAssignment );
            condition = $"{condition} and not {condition2}";
            expression = _parseUtils.CreateIncludeExpression( rule.OutAssignments );
            break;
          case "06":
            condition2 = _parseUtils.CreateSecondCondition( rule.ConditionAssignment );
            condition = $"{condition} and not {condition2}";
            expression = _parseUtils.CreateExcludeExpression( rule.OutAssignments );
            break;
          case "31":
            condition = $"not {condition}";
            expression = _parseUtils.CreateExcludeExpression( rule.OutAssignments );
            break;
          case "33":
            condition2 = _parseUtils.CreateSecondCondition( rule.ConditionAssignment );
            condition = $"not {condition} and {condition2}";
            expression = _parseUtils.CreateExcludeExpression( rule.OutAssignments );
            break;
          case "35":
            condition2 = _parseUtils.CreateSecondCondition( rule.ConditionAssignment );
            condition = $"not {condition} and not {condition2}";
            expression = _parseUtils.CreateExcludeExpression( rule.OutAssignments );
            break;
          default:
            throw new InvalidDataException( $"Unknown dependency type: {dependencyType}" );
        }
        generateRules.Add( new ConvertedRule( $"rule_{rule.Counter}", $"if {precondition} and {condition} then {expression}" ) );
      }
      return generateRules;
    }


  }

  public class ConvertedRule {
    public string Name { get; }
    public string Expression { get; }

    public ConvertedRule( string name, string expression ) {
      this.Expression = expression;
      this.Name = name;
    }
  }
}
