using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configit.AGCO.Prototype.KMATImporter;
using Configit.AGCO.Prototype.RuleConverter;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.Model.Serialization;
using NLog;

namespace Configit.AGCO.Prototype.RuleConverterApp {
  class MaterialConverter {

    private static string SapTablesConnectionString = "Data Source=|DataDirectory|Resources/sap_tables.sdf;Max Database Size=512";

    private readonly Logger Log;

    public string ModelBinDir { get; }

    public MaterialConverter( Logger log, string modelBinDir ) {
      Log = log;
      ModelBinDir = modelBinDir;
    }

    internal void ImportMaterialsAndRules( PackageInfo packageInfo ) {
      var importedKmats = ImportKMATs( packageInfo );
      GenerateRules( importedKmats );
    }

    internal void AddDefaults( string VtPackagePath, string materialNbr ) {
      var dbReader = new DBReader( SapTablesConnectionString );
      var model = dbReader.GetModelByMaterial( materialNbr );
      var pm = LoadProductModel( @"C:\Users\robin\Google Drive\Documents\Customers\AGCO\Data\Models\With Rules\MF7726D6_AGCO.pmx" );
      var defaults = dbReader.GetDefaults( model );
      var hierarchyMapping = dbReader.GetHierarchyMapping();
      var defaultConverter = new DefaultConverter( hierarchyMapping, pm, VtPackagePath );
      defaultConverter.ConvertDefaults( defaults );
    }

    private List<ImportedKMATInfo> ImportKMATs( PackageInfo packageInfo ) {
      var materialImporter = new MaterialImporter( packageInfo, ModelBinDir );
      var materials = packageInfo.GetMaterials();
      Log.Debug( $"Importing {string.Join( ", ", materials )}" );

      var importedKMATs = ImportKMATs( materials, materialImporter ).ToList();
      return importedKMATs;
    }

    private IEnumerable<ImportedKMATInfo> ImportKMATs( List<Material> materials, MaterialImporter materialImporter ) {
      foreach ( var material in materials ) {
        string pmxFile = null;
        try {
          pmxFile = ImportModel( material, materialImporter );
        }
        catch ( ImportException importException ) {
          Log.Error( importException.Message );
          Log.Warn( $"Skipping Material {material.MaterialNbr}, there was an error during import (see logs)" );
        }

        if ( pmxFile != null ) {
          yield return new ImportedKMATInfo( pmxFile, material.MaterialNbr );
        }
      }
    }

    private string ImportModel( Material material, MaterialImporter materialImporter ) {
      try {
        Log.Info( $"Importing model {materialImporter}" );
        var vtFileName = material.VtPath.Split( '\\' ).Last();
        var productModelName = Path.GetFileNameWithoutExtension( vtFileName );

        var modelFile = materialImporter.CreateModelFile( productModelName );
        Log.Info( $"Created {modelFile}" );

        materialImporter.ImportKMAT( modelFile, productModelName );
        Log.Info( $"Imported KMAT {modelFile}" );

        var pmxFile = materialImporter.CreatePMX( modelFile );
        Log.Info( $"Created PMX {pmxFile}" );
        return pmxFile;
      }
      catch ( ImportException importException ) {
        Log.Error( importException.Message );
        Log.Warn( $"Skipping Material {material.MaterialNbr}, there was an error during import (see logs)" );
        return null;
      }

    }

    private void GenerateRules( List<ImportedKMATInfo> importedKmat ) {
      importedKmat.ForEach( kmat => {
        GenerateRulesAndWriteToFile( kmat.MaterialNbr, kmat.PmxFilePath );
      } );
    }

    private void GenerateRulesAndWriteToFile( string materialName, string pmxFile ) {
      Log.Info( $"Generating rules for {materialName}" );
      var dbReader = new DBReader( SapTablesConnectionString );
      var model = dbReader.GetModelByMaterial( materialName );
      if ( model.Matnr == null ) {
        Log.Warn( $"No data available for model {materialName}, skipping " );
        return;
      }

      var pm = LoadProductModel( pmxFile );
      pm.Name = materialName;

      var hierarchyMapping = dbReader.GetHierarchyMapping();

      var categoryRelation = CreateCategoryRelation( dbReader, materialName, pm );
      var salesValidityRelation = CreateSalesValidityRelation( dbReader, model, pm );
      var modelRules = ConvertRules( hierarchyMapping, dbReader, model, pm );
      var basketRules = ConvertBaskets( hierarchyMapping, dbReader, model, pm );
      var importedRules = new Group( "ImportedRules", PrivateFlag.False, EnabledFlag.True );

      var ruleGroup = new Group( "StandardRules", PrivateFlag.False, EnabledFlag.True );
      ruleGroup.Rules.AddRange( modelRules );
      importedRules.AddGroup( ruleGroup );

      var basketGrp = new Group( "Baskets", PrivateFlag.False, EnabledFlag.True );
      basketGrp.Rules.AddRange( basketRules );
      importedRules.AddGroup( basketGrp );

      importedRules.Relations.Add( salesValidityRelation );
      importedRules.Relations.Add( categoryRelation );

      pm.ProductModelStructure.Groups.RemoveAll( g => g.Name == "ImportedRules" );
      pm.ProductModelStructure.Groups.Add( importedRules );

      var newPmx = WriteProductModelToFile( materialName, pm );
      Log.Info( $"Generated file {newPmx}" );
    }


    private Relation CreateSalesValidityRelation( DBReader dbReader, RuleConverter.Data.Model model, ProductModel pm ) {
      var salesValidityRuleConverter = new SalesValidityRuleConverter( dbReader, pm );
      var characteristicsForModel = dbReader.GetCharacteristicValuesForModel( model.Name );
      var dictionary = new Dictionary<string, Dictionary<string, Characteristic>>();
      foreach ( ModelVersion version in model.Versions ) {
        var characteristics = dbReader.GetCharacteristicValuesForModelAndVariant( model.Name, version.Variant );
        dictionary.Add( version.Variant, characteristics.ToDictionary( c => c.Name ) );
      }
      return salesValidityRuleConverter.CreateSalesValidityRelation( model.Name, characteristicsForModel, dictionary );
    }

    private Relation CreateCategoryRelation( DBReader dbReader, string materialName, ProductModel productModel ) {
      var categories = dbReader.GetCategories( materialName );
      var hierarchyRelationConverter = new HierarchyRelationConverter( dbReader, productModel );
      return hierarchyRelationConverter.CreateHierarchyRelation( categories );
    }

    private IEnumerable<Rule> ConvertBaskets( Dictionary<string, string> hierarchyMapping, DBReader dbReader, RuleConverter.Data.Model model, ProductModel pm ) {
      var basketParser = new BasketParser( hierarchyMapping, pm );
      var extractedBaskets = dbReader.GetBaskets( model );
      var convertedBaskets = basketParser.ConvertBaskets( extractedBaskets );
      return convertedBaskets.Select( r => new Rule( r.Name, r.Expression, null, EnabledFlag.True ) );
    }

    private IEnumerable<Rule> ConvertRules( Dictionary<string, string> hierarchyMapping,
      DBReader dbReader,
      RuleConverter.Data.Model model,
      ProductModel pm ) {
      var ruleParser = new RuleParser( hierarchyMapping, pm );
      var extractedRules = dbReader.GetRules( model );
      var convertedRules = ruleParser.ConvertRules( extractedRules );
      return convertedRules.Select( r => new Rule( r.Name, r.Expression, null, EnabledFlag.True ) );
    }

    private ProductModel LoadProductModel( string pmxFile ) {
      var pm = ModelWorkspace.CreateFromFile( pmxFile ).ProductModel;
      var variableProperties = pm.PropertyGroup.First( pg => pg.Name == "variables" ).Properties;
      var showProperty = variableProperties.FirstOrDefault( p => p.Name.ToLower() == "show" );
      if ( showProperty == null ) {
        variableProperties
          .Add( new Property( "Show", null, ProductModel.BOOL_TYPENAME ) );
      }

      return pm;
    }

    private string WriteProductModelToFile( string marialName, ProductModel pm ) {
      ModelWorkspace mws = new ModelWorkspace { ProductModel = pm };
      var newPmx = marialName + "_N.pmx";
      var directoryInfo = Directory.CreateDirectory( "ImportedModels" );
      var pmxPath = Path.Combine( directoryInfo.FullName, newPmx );
      mws.WriteToFile( pmxPath );
      return newPmx;
    }


  }
}
