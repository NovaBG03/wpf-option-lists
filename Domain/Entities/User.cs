using WpfApp.Domain.Enums;

namespace WpfApp.Domain.Entities;

public class User : BaseEntity
{
    public Gender? Gender { get; set; }

    public int? FavouriteActivityId { get; set; }

    public Activity? FavouriteActivity { get; set; }

    public int? FavouriteTechnologyId { get; set; }

    public Technology? FavouriteTechnology { get; set; }

    public ProgrammingLanguage? FavouriteProgrammingLanguage { get; set; }
}
