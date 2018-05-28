using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configit.AGCO.Prototype.KMATImporter {
  public class PackageInfo {
    public string PackagePath { get; private set; }
    private readonly MaterialLoader _materialLoader;

    public PackageInfo( string packagePath ) {
      PackagePath = packagePath;
      _materialLoader = new MaterialLoader();
    }



    public List<Material> GetMaterials() {
      return _materialLoader.LoadMaterials( PackagePath );
    }
  }
}
