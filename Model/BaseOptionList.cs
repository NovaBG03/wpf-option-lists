namespace WpfApp.Model;

public abstract class BaseOptionList<TOption>(IEnumerable<TOption> options)
    where TOption : BaseOption
{
    private readonly List<TOption> _options = [..options];
    private TOption? _selectedOption;

    public IReadOnlyCollection<TOption> Options => _options.AsReadOnly();

    public TOption? SelectedOption
    {
        get => _selectedOption;
        set
        {
            if (value is null || Options.Contains(value))
            {
                _selectedOption = value;
            }
            else
            {
                throw new ArgumentException(
                    "The specified option is not available in the current options collection.",
                    nameof(value));
            }
        }
    }

    protected void SetOptions(IEnumerable<TOption> options)
    {
        var currentSelectedOption = SelectedOption;
        _options.Clear();
        _options.AddRange(options);
        _selectedOption = currentSelectedOption;
    }

    /// <returns>
    /// True if the selection was successful (option was found or null was passed),
    /// false if the ID doesn't match any option and the selection is cleared.
    /// </returns>
    public virtual bool SelectOption(int? id)
    {
        if (id is null)
        {
            _selectedOption = null;
            return true;
        }

        var newSelectedOption = _options.FirstOrDefault(opt => opt.Id == id);
        _selectedOption = newSelectedOption;
        return newSelectedOption is not null;
    }

    public virtual bool ContainsOptionWithName(string name) =>
        Options.Any(opt => string.Equals(opt.Name, name, StringComparison.OrdinalIgnoreCase));

    public bool IsSelected(TOption option) => option.Equals(SelectedOption);

    public bool IsSelected(int id) => id.Equals(SelectedOption?.Id);
}
