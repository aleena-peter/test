using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.AGCO.Prototype.RuleConverter_net46;
using Configit.Model.Serialization;
using NLog;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class SalesValidityRuleConverter {
    private readonly DBReader _dbReader;
    private readonly ProductModel _model;
    private readonly Dictionary<string, VariableInfo> _variableLookup;
    private static readonly Logger Log = LogManager.GetLogger( "RuleParser" );

    public SalesValidityRuleConverter( DBReader dbReader, ProductModel model ) {
      _dbReader = dbReader;
      _model = model;
      _variableLookup = PMUtils.GetVariables( _model.ProductModelStructure );
    }

    public Relation CreateSalesValidityRelation( string modelName, List<Characteristic> characteristics, Dictionary<string, Dictionary<string, Characteristic>> variantCharacteristics ) {
      var rel = new Rel( $"SalesValidity_{modelName}" );
      rel.Columns.Add( _variableLookup["GEN_S_MODEL"].FullyQualifiedName );
      rel.Columns.Add( _variableLookup["GEN_S_VARIANT"].FullyQualifiedName );

      var variants = variantCharacteristics.Keys.ToList();
      // initialize a row for every variant
      foreach ( var variant in variants ) {
        rel.Rows.Add( variant, new List<string> { $"\"{modelName}\"", $"\"{variant}\"" } );
      }
      var lookup = characteristics.ToDictionary( c => c.Name );
      foreach ( var variableInfo in _variableLookup.Values ) {
        if ( lookup.ContainsKey( variableInfo.Name ) ) {
          rel.Columns.Add( variableInfo.FullyQualifiedName );
          foreach ( var variant in variants ) {
            if ( variantCharacteristics[variant].TryGetValue( variableInfo.Name, out var characteristic ) ) {
              rel.Rows[variant].Add( "=" + characteristic.Values.Select( v => $"\"{v}\"" ).Aggregate( ( r, l ) => $"{r},{l}" ) );
            }
            else {
              rel.Rows[variant].Add( "=\"N.A.\"" );
            }
          }
        }
        else {
          if ( variableInfo.Name != "GEN_S_MODEL" && variableInfo.Name != "GEN_S_VARIANT" && variableInfo.Name != "GEN_S_VERSION" &&
               variableInfo.Name != "GEN_S_BRAND" && variableInfo.Name != "GEN_S_PRODUCT_GROUP" && variableInfo.Name != "GEN_S_MODEL_RANGE" && variableInfo.Name != "GEN_S_MODEL" ) {

            var variablesDeleted = PMUtils.DeleteVariables( _model.ProductModelStructure, variableInfo.Name );
            if ( variablesDeleted == 0 ) {
              Log.Warn( $"variable '{variableInfo.Name}' was not deleted" );
            }
            else if ( variablesDeleted > 1 ) {
              Log.Warn( $"muultiple variables '{variableInfo.Name}' were deleted" );
            }
            //var variablePropertyValues = variableInfo.Variable.PropertyValues;
            //var showProperty = variablePropertyValues.FirstOrDefault( pv => pv.Name.ToLower() == "show" );
            //if ( showProperty == null ) {
            //  variablePropertyValues.Add( new PropertyValue( "Show", false ) );
            //}

          }
        }
      }
      var relation = PMUtils.ToRelation( rel );
      relation.PropertyValues.Add( new PropertyValue( "Enabled", true ) );
      return relation;
    }



  }
}

