namespace WpfApp.Model;

public class SystemOption<TEnum> : BaseOption
    where TEnum : struct, Enum
{
    public required TEnum SystemId { get; init; } 
}
