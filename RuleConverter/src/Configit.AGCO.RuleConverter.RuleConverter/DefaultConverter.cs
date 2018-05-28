using System.Collections.Generic;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.Core.Capabilities.Defaults;
using Configit.Core.Model.Logic;
using Configit.Core.Model.Logic.Expression;
using Configit.Core.Serialization;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class DefaultConverter {
    private readonly ParseUtils _parseUtils;

    private const string robinPackage =
      @"C:\Users\robin\Google Drive\Documents\Customers\AGCO\Data\Models\With Rules\MF7726D6_AGCO.vtpackage";

    private const string cameronPackage = @"C:\customers\AGCO\Models\MF7726D6_AGCO.vtpackage";

    private const string pmxFile = "MF7726DVT_AGCO.pmx";

    public DefaultConverter( Dictionary<string, string> hierarchy ) {
      _parseUtils = new ParseUtils( hierarchy );
    }

    public void ConvertDefaults( List<DefaultAssignment> defaults ) {
      ProductModel pm = ModelWorkspace.CreateFromFile( pmxFile ).ProductModel;
      var pmProductModelStructure = pm.ProductModelStructure;
      foreach ( var defaultAssignment in defaults ) {
        //var characteristic = _parseUtils.FindSelectionCriteriaCharacteristic( defaultAssignment.HierarchyElement );
        //var precondition = _parseUtils.GeneratePrecondition( characteristic, defaultAssignment.HierarchyElement );

        var model = defaultAssignment.Model;
        var coutry = defaultAssignment.Country;
        var variant = defaultAssignment.Variant;

        var defaultModel = new LogicModel();
        defaultModel.AddRule( "", "", ExprBld.IfThen(
                                        ( ExprBld.Variables["Model"] == model )
                                        .And( ExprBld.Variables["coutry"] == coutry )
                                        .And( ExprBld.Variables["variant"] == variant ),
                                        ExprBld.Variables[defaultAssignment.Assignment.Characteristic] ) ==
                                      defaultAssignment.Assignment.Value );
        var compiler = new Core.Compile.Compilation.Compiler( defaultModel );
        var defaultSolveData = compiler.CompileNddSolve();
        var scopedDefaultData = new ScopedDefaultData( new[] { "model", "country", "variant" }, defaultSolveData );

        var packagedModelSerializer = new PackagedModelSerializer();
        var packagedModel = packagedModelSerializer.LoadAsync( cameronPackage ).Result.CopyAndInclude( scopedDefaultData );
        packagedModelSerializer.SaveAsync(cameronPackage,
          new[] { packagedModel } ).Wait();

      }
    }
  }
}
