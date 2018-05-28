using System.Collections.Generic;
using System.IO;
using System.Linq;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.AGCO.Prototype.RuleConverter_net46;
using Configit.Core.Capabilities.Defaults;
using Configit.Core.Model;
using Configit.Core.Model.Logic;
using Configit.Core.Model.Logic.Expression;
using Configit.Core.Model.ModelBuildException;
using Configit.Core.Model.VariableDefinitions;
using Configit.Core.Serialization;
using Configit.Model.Serialization;
using NLog;
using Assignment = Configit.Core.Model.VariableDefinitions.Assignment;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class DefaultConverter {
    private readonly ProductModel _pm;
    private readonly ParseUtils _parseUtils;
    private readonly List<IVariableDefinition> scopeVariables;
    private readonly Dictionary<string, VariableInfo> _variableLookup;
    private static readonly Logger Log = LogManager.GetLogger( "RuleParser" );


    //private const string srcPackage =
    //@"C:\Users\robin\Documents\customers\AGCO\Data\Models\With Rules\With Services - Simplified\MF7726D6_AGCO.vtpackage\";

    private readonly string vtPackagePath;// =
    //  @"C:\Users\robin\Documents\customers\AGCO\Data\Package\MF7726D6_AGCO.vtpackage\";

    private const string cameronPackage = @"C:\customers\AGCO\Models\MF7726D6_AGCO.vtpackage\";

    private const string pmxFile = "MF7726DVT_AGCO.pmx";
    private const string MODEL_VARIABLE = "General.GEN_S_MODEL";
    private const string COUNTRY_VARIABLE = "General.GEN_S_COUNTRY";
    private const string VARIANT_VARIABLE = "General.GEN_S_VARIANT";

    public DefaultConverter( Dictionary<string, string> hierarchy, ProductModel pm, string vtPackagePath ) {
      _pm = pm;
      this.vtPackagePath = vtPackagePath;
      _parseUtils = new ParseUtils( hierarchy, pm );
      var productModelStructure = _pm.ProductModelStructure;
      _variableLookup = PMUtils.GetVariables( productModelStructure );
      scopeVariables = new List<IVariableDefinition> {
        new StringVariable( MODEL_VARIABLE ),
        new StringVariable( COUNTRY_VARIABLE ),
        new StringVariable( VARIANT_VARIABLE )
      };
    }

    public void ConvertDefaults( List<DefaultAssignment> defaults ) {
      var packagedModelSerializer = new PackagedModelSerializer();
      Log.Info( $"Adding defaults to package {vtPackagePath}" );
      var existingPackagedModel = packagedModelSerializer.LoadAsync( vtPackagePath, "MF7726D6_AGCO" ).Result;
      var variables = existingPackagedModel.Variables.ModelVariables;

      var scopedDefaultDatas = CreateScopeDefaultData( defaults, variables );

      var newPackagedModel = existingPackagedModel.CopyAndInclude( scopedDefaultDatas.Cast<ICompiledData>().ToArray() );
      Log.Info( $"Saving package with defaults to {newPackagedModel}" );
      packagedModelSerializer.SaveAsync( vtPackagePath, new[] { newPackagedModel } ).Wait();
    }

    public List<ScopedDefaultData> CreateScopeDefaultData( List<DefaultAssignment> defaults, VariableCollection variables ) {
      var scopedDefaultDatas = new List<ScopedDefaultData>();
      foreach ( var defaultAssignmentGroup in defaults.GroupBy( d => d.Assignment.Characteristic ) ) {
        var variableName = defaultAssignmentGroup.Key;
        Log.Info( $"Adding default assignment for {variableName}" );
        string variableFullName = _variableLookup[variableName].FullyQualifiedName;

        var defaultModel = CreateDefaultModel( defaultAssignmentGroup, variableFullName, variables );
        //defaultModel.
        var compiler = new Core.Compile.Compilation.Compiler( defaultModel );
        var defaultSolveData = compiler.CompileNddSolve();

        scopedDefaultDatas.Add( new ScopedDefaultData( new[] { MODEL_VARIABLE, COUNTRY_VARIABLE, VARIANT_VARIABLE },
          defaultSolveData, null, new[] { variableFullName } ) );

      }
      return scopedDefaultDatas;
    }

    public LogicModel CreateDefaultModel( IGrouping<string, DefaultAssignment> defaultAssignmentGroup, string variableFullName, VariableCollection variables ) {
      var defaultModel = new LogicModel( "defaultModel", null, variables );

      foreach ( var defaultAssignment in defaultAssignmentGroup ) {
        if ( !ValuesInDomain( defaultAssignment, variables, variableFullName ) ) {
          Log.Warn( $"value not in domain: {COUNTRY_VARIABLE} = {defaultAssignment.Country}" );
          continue;
        }
        var defaultExpr = ExprBld.IfThen(
          ( ExprBld.Variables[MODEL_VARIABLE] == defaultAssignment.Model )
          .And( ExprBld.Variables[COUNTRY_VARIABLE] == defaultAssignment.Country )
          .And( ExprBld.Variables[VARIANT_VARIABLE] == defaultAssignment.Variant ),
          ExprBld.Variables[variableFullName] == defaultAssignment.Assignment.Value
        );

        var ruleDescription =
          $"{defaultAssignment.Model}.{defaultAssignment.Country}.{defaultAssignment.Variant}-{defaultAssignment.Assignment.Value}";
        Log.Trace( $"Adding default {ruleDescription}" );
        defaultModel.AddRule( ruleDescription, ruleDescription, defaultExpr );
      }
      return defaultModel;
    }

    private bool ValuesInDomain( DefaultAssignment defaultAssignment, VariableCollection variables, string variableFullName ) {
      return variables[COUNTRY_VARIABLE].IsInDomain( defaultAssignment.Country ) &&
             variables[MODEL_VARIABLE].IsInDomain( defaultAssignment.Model ) &&
             variables[VARIANT_VARIABLE].IsInDomain( defaultAssignment.Variant ) &&
             variables[variableFullName].IsInDomain( defaultAssignment.Assignment.Value );

    }
  }
}
