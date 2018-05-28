namespace Configit.AGCO.Prototype.RuleConverter.Data {
  public class Assignment {
    public Assignment( string characteristic, string value ) {
      Value = value;
      Characteristic = characteristic;
    }

    public string Value { get; }
    public string Characteristic { get; }
  }
}