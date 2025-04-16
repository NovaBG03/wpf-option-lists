namespace WpfApp.Domain.Entities;

public class Technology : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public List<User> Users { get; set; } = [];
}
