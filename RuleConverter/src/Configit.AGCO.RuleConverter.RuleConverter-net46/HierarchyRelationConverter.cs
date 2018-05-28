using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.AGCO.Prototype.RuleConverter_net46;
using Configit.Model.Serialization;

namespace Configit.AGCO.Prototype.RuleConverter {
  public class HierarchyRelationConverter {
    private readonly ProductModel _model;
    private readonly Dictionary<string, VariableInfo> _variableLookup;

    public HierarchyRelationConverter( DBReader dbReader, ProductModel model ) {
      _model = model;
      _variableLookup = PMUtils.GetVariables( _model.ProductModelStructure );
    }

    public Relation CreateHierarchyRelation( Category category ) {
      var rel = new Rel( "CategoriesMapping" );
      rel.Columns.Add( _variableLookup["GEN_S_VARIANT"].FullyQualifiedName );
      rel.Columns.Add( _variableLookup["GEN_S_VERSION"].FullyQualifiedName );
      rel.Columns.Add( _variableLookup["GEN_S_BRAND"].FullyQualifiedName );
      rel.Columns.Add( _variableLookup["GEN_S_PRODUCT_GROUP"].FullyQualifiedName );
      rel.Columns.Add( _variableLookup["GEN_S_MODEL_RANGE"].FullyQualifiedName );
      rel.Columns.Add( _variableLookup["GEN_S_MODEL"].FullyQualifiedName );

      foreach ( var version in category.Versions ) {
        rel.Rows.Add( version.Name, new List<string>() { version.Variant, version.Name, category.Brand, category.ProductGroup, category.ModelRange, category.Model } );
      }
      return PMUtils.ToRelation( rel );
    }
  }
}
