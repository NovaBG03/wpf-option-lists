namespace WpfApp.Model;

public abstract class BaseOption
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public override string ToString() => Name;
}
