using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configit.AGCO.Prototype.KMATImporter {
  public class ImportedKMATInfo {
    public string PmxFilePath { get; }
    public string MaterialNbr { get; }

    public ImportedKMATInfo(string pmxFilePath, string materialNbr) {
      PmxFilePath = pmxFilePath;
      MaterialNbr = materialNbr;
    }
  }
}
