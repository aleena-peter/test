using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configit.AGCO.Prototype.KMATImporter {
  class MaterialLoader {

    private const string MaterialDBPath = @"App\Materials\materials.sdf";

    public List<Material> LoadMaterials( string PackagePath ) {
      var materials = new List<Material>();
      string connectionString = $"Data Source=|DataDirectory|{Path.Combine( PackagePath, MaterialDBPath )};Max Database Size=512";
      using ( var connection = new SqlCeConnection( connectionString ) ) {
        connection.Open();
        using ( SqlCeCommand cmd =
          new SqlCeCommand(
            $"SELECT MaterialDefinition_id FROM materialproperty WHERE Name = 'IsConfigurable' and value = 'True'",
            connection ) ) {
          var reader = cmd.ExecuteReader();
          while ( reader.Read() ) {
            int id = reader.GetInt32( 0 );
            var vtFilePath = GetVTFilePath( id, connection );
            var materialNbr = GetMaterialNbr( id, connection );
            materials.Add( new Material( materialNbr, vtFilePath ) );
          }
        }
      }
      return materials;
    }

    private string GetMaterialNbr( int id, SqlCeConnection connection ) {
      return GetValue( "ExternalId", id, connection );
    }

    private string GetVTFilePath( int id, SqlCeConnection connection ) {
      return GetValue( "VTFile", id, connection );
    }

    private string GetValue( string name, int id, SqlCeConnection connection ) {
      using ( SqlCeCommand cmd2 =
        new SqlCeCommand( $"SELECT value from materialproperty where MaterialDefinition_id = {id} and Name = '{name}'",
          connection ) ) {
        return (string) cmd2.ExecuteScalar();
      }
    }
  }
}
