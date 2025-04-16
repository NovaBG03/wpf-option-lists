namespace WpfApp.Model;

public abstract class BaseOption
{
    public int Id { get; set; }
    public string DisplayTextValue { get; set; } = string.Empty;

    public override string ToString() => DisplayTextValue;
}
