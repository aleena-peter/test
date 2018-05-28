namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class ModelVersion {
    public ModelVersion( string name, string variant ) {
      Name = name;
      Variant = variant;
    }

    public string Name { get; }
    public string Variant { get; }

  }
}