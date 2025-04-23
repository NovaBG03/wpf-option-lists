using System.Collections.ObjectModel;
using WpfApp.Domain.Enums;

namespace WpfApp.Model;

public static class SystemDefinedOptions
{
    public static readonly IReadOnlyCollection<SystemOption<Gender>> Genders =
        CreateReadOnlyCollection<Gender>();

    public static readonly IReadOnlyCollection<SystemOption<ProgrammingLanguage>> ProgrammingLanguages =
        CreateReadOnlyCollection<ProgrammingLanguage>();

    private static ReadOnlyCollection<SystemOption<TEnum>> CreateReadOnlyCollection<TEnum>()
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().Select(DefaultConvertEnumToSystemOption).ToList().AsReadOnly();

    private static SystemOption<TEnum> DefaultConvertEnumToSystemOption<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        new()
        {
            Id = -(int)(object)value - 1,
            SystemId = value,
            Name = value.ToString()
        };
}
