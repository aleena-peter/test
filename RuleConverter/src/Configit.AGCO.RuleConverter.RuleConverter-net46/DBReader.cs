using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using Configit.AGCO.Prototype.RuleConverter.Data;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class DBReader {
    private readonly string _categoriesConnectionString;
    private readonly string _rulesBasketsConnectionString;
    private readonly string _rulesConnectionString;
    private readonly string _validityConnectionString;


    public DBReader( string connectionString ) : this( connectionString, connectionString, connectionString, connectionString ) { }

    public DBReader( string categoriesConnectionString,
      string validityConnectionString,
      string rulesConnectionString,
      string rulesBasketsConnectionString ) {
      _categoriesConnectionString = categoriesConnectionString;
      _validityConnectionString = validityConnectionString;
      _rulesConnectionString = rulesConnectionString;
      _rulesBasketsConnectionString = rulesBasketsConnectionString;
    }

    public List<Characteristic> GetCharacteristicValuesForModelAndVariant( string model, string variant ) {
      var valueDictionary = new Dictionary<string, Characteristic>();
      using ( var connection = new SqlCeConnection( _validityConnectionString ) ) {
        connection.Open();
        using ( var cmd =
          new SqlCeCommand(
            $"SELECT CHAR_PROD, VAR, CSTIC, VALUE FROM ZTVC_MF_S_VALID WHERE CHAR_PROD = '{model}' and var='{variant}'",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var characteristic = reader.GetString( 2 );
            var value = reader.GetString( 3 );
            if ( valueDictionary.ContainsKey( characteristic ) )
              valueDictionary[characteristic].Values.Add( value );
            else
              valueDictionary[characteristic] = new Characteristic( characteristic, new List<string> { value } );
          }
        }
      }
      return valueDictionary.Values.ToList();
    }

    public List<Characteristic> GetCharacteristicValuesForModel( string model ) {
      var valueDictionary = new Dictionary<string, Characteristic>();
      using ( var connection = new SqlCeConnection( _validityConnectionString ) ) {
        connection.Open();
        using ( var cmd =
          new SqlCeCommand(
            $"SELECT distinct(VALUE), cstic FROM ZTVC_MF_S_VALID WHERE CHAR_PROD = '{model}' ",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var value = reader.GetString( 0 );
            var characteristic = reader.GetString( 1 );
            if ( valueDictionary.ContainsKey( characteristic ) )
              valueDictionary[characteristic].Values.Add( value );
            else
              valueDictionary[characteristic] = new Characteristic( characteristic, new List<string> { value } );
          }
        }
      }
      return valueDictionary.Values.ToList();
    }

    public Data.Model GetModelByMaterial( string materialNbr ) {
      using ( var connection = new SqlCeConnection( _categoriesConnectionString ) ) {
        connection.Open();
        var model = new Data.Model();
        using ( var cmd =
          new SqlCeCommand(
            $"SELECT mr.MODEL, mr.MODEL_RANGE, grp.PRODUCT_GROUP, b.BRAND FROM YTVC_XX_MOD_MRA mr JOIN YTVC_XX_MRA_GRP grp on grp.MODEL_RANGE = mr.MODEL_RANGE JOIN YTVC_XX_GRP_BRD b on b.PRODUCT_GROUP = grp.PRODUCT_GROUP WHERE mr.MATNR = '{materialNbr}'",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            model.Matnr = materialNbr;
            model.Name = reader.GetString( 0 );
            model.ModelRange = reader.GetString( 1 );
            model.ProdcutGroup = reader.GetString( 2 );
            model.Brand = reader.GetString( 3 );
          }
        }
        using ( var cmd = new SqlCeCommand( $"SELECT VERSION, VAR FROM YTVC_XX_VER_MOD WHERE MODEL = '{model.Name}'",
          connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var version = reader.GetString( 0 );
            var variant = reader.GetString( 1 );
            model.Versions.Add( new ModelVersion( version, variant ) );
          }
        }
        return model;
      }
    }

    public Dictionary<string, string> GetHierarchyMapping() {
      var hierarchy = new Dictionary<string, string>();
      using ( var connection = new SqlCeConnection( _categoriesConnectionString ) ) {
        connection.Open();
        using ( var cmd =
          new SqlCeCommand(
            $"select distinct(version) from YTVC_XX_VER_MOD",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var version = reader.GetString( 0 );
            hierarchy[version] = "GEN_S_VERSION";
          }
        }
        using ( var cmd =
          new SqlCeCommand(
            $"select distinct(model) from YTVC_XX_MOD_MRA",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var model = reader.GetString( 0 );
            hierarchy[model] = "GEN_S_MODEL";
          }
        }
        using ( var cmd =
          new SqlCeCommand(
            $"select distinct(model_range) from YTVC_XX_MOD_MRA",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var model_range = reader.GetString( 0 );
            hierarchy[model_range] = "GEN_S_MODEL_RANGE";
          }
        }

        return hierarchy;
      }
    }

    public Category GetCategories( string materialName ) {
      var category = new Category( materialName );
      using ( var connection = new SqlCeConnection( _categoriesConnectionString ) ) {
        connection.Open();
        using ( var cmd =
          new SqlCeCommand(
            $"select Model,MODEL_RANGE from YTVC_XX_MOD_MRA where matnr = '{materialName}';",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            category.Model = reader.GetString( 0 );
            category.ModelRange = reader.GetString( 1 );
          }
        }
        using ( var cmd =
          new SqlCeCommand(
            $"select version, var from YTVC_XX_VER_MOD where model = '{category.Model}'",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() )
            category.Versions.Add(
              new ModelVersion( reader.GetString( 0 ), reader.GetString( 1 ) )
            );
        }
        using ( var cmd =
          new SqlCeCommand(
            $"select Product_group from YTVC_XX_MRA_GRP where Model_range = '{category.ModelRange}'",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() )
            category.ProductGroup = reader.GetString( 0 );
        }
        using ( var cmd =
          new SqlCeCommand(
            $"select Brand from YTVC_XX_GRP_BRD where product_group = '{category.ProductGroup}'",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() )
            category.Brand = reader.GetString( 0 );
        }

        return category;
      }
    }

    public List<ExtractedRule> GetRules( Data.Model model ) {
      var versions = model.Versions.Select( v => "'" + v.Name + "'" ).Aggregate( ( s, e ) => s + "," + e );
      var variants = model.Versions.Select( v => "'" + v.Variant + "'" ).Aggregate( ( s, e ) => s + "," + e );
      var hierarchyElements = $"'{model.Name}', {versions}, {variants}";
      using ( var connection = new SqlCeConnection( _rulesBasketsConnectionString ) ) {
        var dic = new Dictionary<string, ExtractedRule>();
        connection.Open();
        using ( var cmd = new SqlCeCommand(
          $"select counter, char_prod, dep_type, char_in, value_in, char_out, value_out, char_if, value_if from ZTVC_MF_S_RULE where char_prod in ({hierarchyElements})",
          connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var counter = reader.GetString( 0 );
            var char_prod = reader.GetString( 1 );
            var dep_type = reader.GetString( 2 );
            var char_in = reader.GetString( 3 );
            var value_in = reader.GetString( 4 );
            var char_out = reader.GetString( 5 );
            var value_out = reader.GetString( 6 );
            var char_if = reader.GetString( 7 );
            var value_if = reader.GetString( 8 );
            if ( dic.TryGetValue( counter, out var extractedRule ) ) {
              extractedRule.OutAssignments.Add( new Assignment( char_out, value_out ) );
            }
            else {
              var rule = new ExtractedRule( counter ) {
                InAssignment = new Assignment( char_in, value_in ),
                ConditionAssignment = new Assignment( char_if, value_if ),
                DependencyType = dep_type,
                HierarchyElement = char_prod,
                OutAssignments = new List<Assignment> { new Assignment( char_out, value_out ) }
              };
              dic.Add( counter, rule );
            }
          }
        }
        return dic.Values.ToList();
      }
    }

    public List<Basket> GetBaskets( Data.Model model ) {
      var versions = model.Versions.Select( v => "'" + v.Name + "'" ).Aggregate( ( s, e ) => s + "," + e );
      var variants = model.Versions.Select( v => "'" + v.Variant + "'" ).Aggregate( ( s, e ) => s + "," + e );
      var hierarchyElements = $"'{model.Name}', {versions}, {variants}";
      Debug.WriteLine( hierarchyElements );
      using ( var connection = new SqlCeConnection( _rulesBasketsConnectionString ) ) {
        var dic = new Dictionary<string, Basket>();
        connection.Open();

        using ( var cmd = new SqlCeCommand(
          $"select counter, char_prod, char_in, value_in, char_out, value_out, char_if, value_if from ZTVC_MF_S_RULE where char_prod in ({hierarchyElements})",
          connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var counter = reader.GetString( 0 );
            var char_prod = reader.GetString( 1 );
            var char_in = reader.GetString( 2 );
            var value_in = reader.GetString( 3 );
            var char_out = reader.GetString( 4 );
            var value_out = reader.GetString( 5 );
            var char_if = reader.GetString( 6 );
            var value_if = reader.GetString( 7 );
            if ( dic.TryGetValue( counter, out var extractedRule ) ) {
              extractedRule.IncludedElements.Add( new Assignment( char_out, value_out ) );
            }
            else {
              var rule = new Basket( counter ) {
                Package = new Assignment( char_in, value_in ),
                ConditionAssignment = new Assignment( char_if, value_if ),
                HierarchyElement = char_prod,
                IncludedElements = new List<Assignment> { new Assignment( char_out, value_out ) }
              };
              dic.Add( counter, rule );
            }
          }
        }
        return dic.Values.ToList();
      }
    }

    public List<DefaultAssignment> GetDefaults( Data.Model model ) {
      var versions = model.Versions.Select( v => "'" + v.Name + "'" ).Aggregate( ( s, e ) => s + "," + e );
      var variants = model.Versions.Select( v => "'" + v.Variant + "'" ).Aggregate( ( s, e ) => s + "," + e );
      var hierarchyElements = $"'{model.Name}', {versions}, {variants}";
      Debug.WriteLine( hierarchyElements );
      using ( var connection = new SqlCeConnection( _rulesConnectionString ) ) {
        var defaults = new List<DefaultAssignment>();
        connection.Open();
        var sql = $"select var, country, cstic, value from ZTVC_MF_S_DEF where char_prod in ({hierarchyElements})";
        Debug.WriteLine( sql );
        using ( var cmd = new SqlCeCommand( sql,
          connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            var variant = reader.GetString( 0 );
            var country = reader.GetString( 1 );
            var cstic = reader.GetString( 2 );
            var value = reader.GetString( 3 );
            var defaultAssignment =
              new DefaultAssignment( model.Name, variant, country, new Assignment( cstic, value ) );
            defaults.Add( defaultAssignment );
          }
        }
        return defaults;
      }
    }
  }
}