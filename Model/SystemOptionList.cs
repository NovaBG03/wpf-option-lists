using WpfApp.Domain.Enums;

namespace WpfApp.Model;

public class SystemOptionList<TEnum> : BaseOptionList<SystemOption<TEnum>> where TEnum : struct, Enum
{
    public static SystemOptionList<Gender> ForGender() =>
        new(SystemDefinedOptions.Genders);

    public static SystemOptionList<ProgrammingLanguage> ForProgrammingLanguages() =>
        new(SystemDefinedOptions.ProgrammingLanguages);

    private SystemOptionList(IEnumerable<SystemOption<TEnum>> options) : base(options)
    {
    }

    /// <returns>
    /// True if the selection was successful (option was found or null was passed),
    /// false if the systemId doesn't match any option and the selection is cleared.
    /// </returns>
    public bool SelectOption(TEnum? systemId)
    {
        if (systemId == null)
        {
            SelectedOption = null;
            return true;
        }

        var newSelectedOption = Options.FirstOrDefault(opt => opt.SystemId.Equals(systemId));
        SelectedOption = newSelectedOption;
        return newSelectedOption is not null;
    }

    public bool IsSelected(TEnum systemId) => systemId.Equals(SelectedOption?.SystemId);
}
