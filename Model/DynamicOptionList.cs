namespace WpfApp.Model;

public class DynamicOptionList() : BaseOptionList<DynamicOption>([])
{
    public new void SetOptions(IEnumerable<DynamicOption> options)
    {
        base.SetOptions(options);
    }
}
