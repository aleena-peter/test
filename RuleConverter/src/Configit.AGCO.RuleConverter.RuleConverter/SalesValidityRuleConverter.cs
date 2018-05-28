using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class SalesValidityRuleConverter {
    private readonly DBReader _dbReader;
    private readonly ProductModel _model;

    public SalesValidityRuleConverter( DBReader dbReader, ProductModel model ) {
      _dbReader = dbReader;
      _model = model;
    }

    public void CreateSalesValidityRelation( string modelName, List<Characteristic> characteristics, Dictionary<string, Dictionary<string, Characteristic>> variantCharacteristics ) {
      var generalGroup = GetGeneralGroup();
      var rel = new Rel( $"SalesValidity_{modelName}" );

      rel.Columns.Add( $"{generalGroup.Name}.GEN_S_MODEL" );
      rel.Columns.Add( $"{generalGroup.Name}.GEN_S_VARIANT" );

      var variants = variantCharacteristics.Keys.ToList();
      // initialize a row for every variant
      foreach ( var variant in variants ) {
        rel.Rows.Add( variant, new List<string> { $"\"{modelName}\"", $"\"{variant}\"" } );
      }
      var lookup = characteristics.ToDictionary( c => c.Name );
      foreach ( var variable in GetVariables()) {
        if ( lookup.ContainsKey( variable.Name ) ) {
          rel.Columns.Add( $"{generalGroup.Name}.{variable.Name}" );
          foreach ( var variant in variants ) {
            if ( variantCharacteristics[variant].TryGetValue( variable.Name, out var characteristic ) ) {
              rel.Rows[variant].Add( "=" + characteristic.Values.Select( v => $"\"{v}\"" ).Aggregate( ( r, l ) => $"{r},{l}" ) );
            }
            else {
              rel.Rows[variant].Add( "=\"N.A.\"" );
            }
          }
        }
        else {
          if ( variable.Name != "GEN_S_MODEL" && variable.Name != "GEN_S_VARIANT" ) {
            variable.PropertyValues.Add( new PropertyValue( "Show", false ) );
          }
        }
      }
      var relation = ToRelation( rel );
      relation.PropertyValues.Add( new PropertyValue( "Enabled", true ) );
      generalGroup.AddRelation( relation );
    }

    private List<Variable> GetVariables() {
      return GetGeneralGroup().Variables;
    }

    private Group GetGeneralGroup() {
      return _model.ProductModelStructure.Groups[0];
    }

    private Relation ToRelation( Rel rel ) {
      Relation relation = new Relation( rel.Name, null, EnabledFlag.False );
      foreach ( var col in rel.Columns ) {
        relation.RelationColumns.Add( new RelationColumn( col ) );
      }
      foreach ( var relRow in rel.Rows ) {
        relation.AddRelationRow( relRow.Value.ToArray() );
      }
      return relation;
    }

  }
}

