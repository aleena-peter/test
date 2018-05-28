namespace Configit.AGCO.Prototype.KMATImporter {
  public class Material {
    public string MaterialNbr { get; }
    public string VtPath { get; }

    public Material(string materialNbr, string VTPath) {
      MaterialNbr = materialNbr;
      VtPath = VTPath;
    }
  }
}