using WpfApp.Domain.Enums;

namespace WpfApp.Model;

public class MixedOptionList<TEnum> : BaseOptionList<BaseOption> where TEnum : struct, Enum
{
    public static MixedOptionList<ProgrammingLanguage> ForProgrammingLanguages() =>
        new(SystemOptionList<ProgrammingLanguage>.ForProgrammingLanguages());

    private readonly SystemOptionList<TEnum> _systemOptionList;

    public new IReadOnlyCollection<BaseOption> Options =>
        _systemOptionList.Options.Concat(base.Options).ToList().AsReadOnly();

    public new BaseOption? SelectedOption
    {
        get => base.SelectedOption ?? _systemOptionList.SelectedOption;
        set
        {
            switch (value)
            {
                case null:
                    _systemOptionList.SelectedOption = null;
                    base.SelectedOption = null;
                    break;
                case SystemOption<TEnum> systemOption:
                    _systemOptionList.SelectedOption = systemOption;
                    base.SelectedOption = null;
                    break;
                case DynamicOption dynamicOption:
                    _systemOptionList.SelectedOption = null;
                    base.SelectedOption = dynamicOption;
                    break;
                default:
                    throw new ArgumentException(
                        $"The specified option type is not supported by ${nameof(MixedOptionList<TEnum>)}.",
                        nameof(value));
            }
        }
    }

    private MixedOptionList(SystemOptionList<TEnum> systemOptionList) : base([])
    {
        _systemOptionList = systemOptionList;
    }

    public void SetOptions(IEnumerable<DynamicOption> options)
    {
        var optionList = options.ToList();
        if (optionList.Any(opt => _systemOptionList.Options.Any(sysOpt => opt.Id == sysOpt.Id)))
        {
            throw new ArgumentException(
                "Option with duplicate ID detected. Cannot add options with IDs that already exist in the system options.");
        }

        base.SetOptions(optionList);
    }

    /// <returns>
    /// True if the selection was successful (option was found or null was passed),
    /// false if the ID doesn't match any option and the selection is cleared.
    /// </returns>
    public override bool SelectOption(int? id)
    {
        _systemOptionList.SelectedOption = null;
        return base.SelectOption(id) || _systemOptionList.SelectOption(id);
    }

    /// <returns>
    /// True if the selection was successful (option was found or null was passed),
    /// false if the systemId doesn't match any option and the selection is cleared.
    /// </returns>
    public bool SelectOption(TEnum? systemId)
    {
        if (systemId is null)
        {
            ClearSelection();
            return true;
        }

        base.SelectedOption = null;
        return _systemOptionList.SelectOption(systemId);
    }

    public void ClearSelection()
    {
        base.SelectedOption = null;
        _systemOptionList.SelectedOption = null;
    }

    public bool ContainsSystemOptionWithName(string name) =>
        _systemOptionList.ContainsOptionWithName(name);

    public bool ContainsDynamicOptionWithName(string name) =>
        base.ContainsOptionWithName(name);

    public override bool ContainsOptionWithName(string name) =>
        ContainsSystemOptionWithName(name) || ContainsDynamicOptionWithName(name);
    
    public bool IsSelected(TEnum systemId) => _systemOptionList.IsSelected(systemId);
}
