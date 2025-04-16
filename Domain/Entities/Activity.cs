namespace WpfApp.Domain.Entities;

public class Activity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public List<User> Users { get; set; } = [];
}
