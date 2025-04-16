using System.Windows;
using WpfApp.Data;
using WpfApp.Domain.Entities;
using WpfApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using WpfApp.Model;
using System.Windows.Input;
using WpfApp.Infrastructure;
using WpfApp.Views;

namespace WpfApp.ViewModels;

public class UserFormViewModel : BaseViewModel
{
    private static readonly IList<SystemOption<Gender>> SystemDefinedGenderOptions = Enum.GetValues<Gender>().Select(
        x => new SystemOption<Gender>
        {
            Id = (int)x,
            SystemId = x,
            DisplayTextValue = x.ToString()
        }).ToList().AsReadOnly();

    private static readonly IList<SystemOption<ProgrammingLanguage>> SystemDefinedProgrammingLanguageOptions = Enum
        .GetValues<ProgrammingLanguage>().Select(x => new SystemOption<ProgrammingLanguage>
        {
            Id = (int)x,
            SystemId = x,
            DisplayTextValue = x.ToString()
        }).ToList().AsReadOnly();

    private readonly ApplicationDbContext _dbContext;
    private User _currentUser = new();

    private IEnumerable<BaseOption> _genderOptions = [];
    private BaseOption? _selectedGenderOption;

    private IEnumerable<BaseOption> _activityOptions = [];
    private BaseOption? _selectedActivityOption;

    private IEnumerable<BaseOption> _technologyOptions = [];
    private BaseOption? _selectedTechnologyOption;

    public User CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public IEnumerable<BaseOption> GenderOptions
    {
        get => _genderOptions;
        private set => SetProperty(ref _genderOptions, value);
    }

    public BaseOption? SelectedGenderOption
    {
        get => _selectedGenderOption;
        set
        {
            if (SetProperty(ref _selectedGenderOption, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public IEnumerable<BaseOption> ActivityOptions
    {
        get => _activityOptions;
        private set => SetProperty(ref _activityOptions, value);
    }


    public BaseOption? SelectedActivityOption
    {
        get => _selectedActivityOption;
        set
        {
            if (SetProperty(ref _selectedActivityOption, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public IEnumerable<BaseOption> TechnologyOptions
    {
        get => _technologyOptions;
        private set => SetProperty(ref _technologyOptions, value);
    }

    public BaseOption? SelectedTechnologyOption
    {
        get => _selectedTechnologyOption;
        set
        {
            if (SetProperty(ref _selectedTechnologyOption, value))
            {
                CommandManager.InvalidateRequerySuggested();
                if (value is SystemOption<ProgrammingLanguage> { SystemId: ProgrammingLanguage.Java })
                {
                    MessageBox.Show($"Java!?", "Are you sure?", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    public ICommand AddNewActivityCommand { get; }
    public ICommand AddNewTechnologyCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SelectRandomUserCommand { get; }

    public UserFormViewModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        AddNewActivityCommand = new RelayCommand(ExecuteAddNewActivity);
        AddNewTechnologyCommand = new RelayCommand(ExecuteAddNewTechnology);
        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
        SelectRandomUserCommand = new RelayCommand(ExecuteSelectRandomUser);

        LoadData();
        InitializeSelections();
    }

    private void LoadData()
    {
        GenderOptions = SystemDefinedGenderOptions;
        LoadActivityOptions();
        LoadTechnologyOptions();
    }

    private void LoadActivityOptions()
    {
        ActivityOptions = _dbContext.Activities.AsNoTracking().Select(x => new DynamicOption()
        {
            Id = x.Id,
            DisplayTextValue = x.Name,
        }).ToList();
    }

    private void LoadTechnologyOptions()
    {
        TechnologyOptions = SystemDefinedProgrammingLanguageOptions.Concat<BaseOption>(_dbContext.Technologies
            .AsNoTracking().Select(x => new DynamicOption()
            {
                Id = x.Id,
                DisplayTextValue = x.Name,
            })).ToList();
    }

    private void InitializeSelections()
    {
        SelectedGenderOption = GenderOptions.FirstOrDefault(opt =>
            opt is SystemOption<Gender> sysOpt && sysOpt.SystemId == CurrentUser.Gender);
        SelectedActivityOption = ActivityOptions.FirstOrDefault(opt => opt.Id == CurrentUser.FavouriteActivityId);
        SelectedTechnologyOption = TechnologyOptions.FirstOrDefault(opt =>
        {
            if (opt is SystemOption<ProgrammingLanguage> sysOpt)
            {
                return sysOpt.SystemId == CurrentUser.FavouriteProgrammingLanguage;
            }
            return opt.Id == CurrentUser.FavouriteTechnologyId;
        });
    }

    private void ExecuteAddNewActivity()
    {
        var dialog = new NewItemDialog("Add New Activity")
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true) return;

        var newItemName = dialog.ItemName.ToLower();
        if (string.IsNullOrWhiteSpace(newItemName)) return;

        var newActivity = new Activity { Name = newItemName };
        _dbContext.Activities.Add(newActivity);

        try
        {
            _dbContext.SaveChanges();
            LoadActivityOptions();
            SelectedActivityOption = ActivityOptions.FirstOrDefault(opt => opt.DisplayTextValue == newItemName);
        }
        catch (DbUpdateException ex)
        {
            _dbContext.ChangeTracker.Clear();
            if (ex.InnerException != null &&
                ex.InnerException.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"An activity named '{newItemName}' already exists.", "Duplicate Activity",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(
                    $"An error occurred while saving the new activity: {ex.InnerException?.Message ?? ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            _dbContext.ChangeTracker.Clear();
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ExecuteAddNewTechnology()
    {
        var dialog = new NewItemDialog("Add New Technology")
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true) return;

        var newItemName = dialog.ItemName.ToLower();
        if (string.IsNullOrWhiteSpace(newItemName)) return;

        var isSystemDefined = SystemDefinedProgrammingLanguageOptions
            .Any(opt => string.Equals(opt.DisplayTextValue, newItemName, StringComparison.OrdinalIgnoreCase));

        if (isSystemDefined)
        {
            MessageBox.Show($"'{newItemName}' is a system-defined programming language and cannot be added again.",
                "System Language", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var newTechnology = new Technology { Name = newItemName };
        _dbContext.Technologies.Add(newTechnology);

        try
        {
            _dbContext.SaveChanges();
            LoadTechnologyOptions();
            SelectedTechnologyOption = TechnologyOptions.FirstOrDefault(opt =>
                opt is DynamicOption dynOpt && dynOpt.DisplayTextValue == newItemName);
        }
        catch (DbUpdateException ex)
        {
            _dbContext.ChangeTracker.Clear();
            if (ex.InnerException != null &&
                ex.InnerException.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"A technology named '{newItemName}' already exists.", "Duplicate Technology",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(
                    $"An error occurred while saving the new technology: {ex.InnerException?.Message ?? ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            _dbContext.ChangeTracker.Clear();
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private bool CanExecuteSave()
    {
        return SelectedGenderOption != null &&
               SelectedActivityOption != null &&
               SelectedTechnologyOption != null;
    }

    private void ExecuteSave()
    {
        if (!CanExecuteSave()) return;

        if (SelectedGenderOption is SystemOption<Gender> genderOpt)
        {
            CurrentUser.Gender = genderOpt.SystemId;
        }

        if (SelectedActivityOption != null)
        {
            CurrentUser.FavouriteActivityId = SelectedActivityOption.Id;
        }

        if (SelectedTechnologyOption != null)
        {
            switch (SelectedTechnologyOption)
            {
                case SystemOption<ProgrammingLanguage> techSysOpt:
                {
                    CurrentUser.FavouriteProgrammingLanguage = techSysOpt.SystemId;
                    CurrentUser.FavouriteTechnology = null;
                    CurrentUser.FavouriteTechnologyId = null;
                    break;
                }
                case DynamicOption techDynOpt:
                    CurrentUser.FavouriteProgrammingLanguage = null;
                    CurrentUser.FavouriteTechnologyId = techDynOpt.Id;
                    break;
            }
        }

        try
        {
            if (CurrentUser.Id == 0)
            {
                _dbContext.Users.Add(CurrentUser);
            }
            else
            {
                _dbContext.Users.Update(CurrentUser);
            }

            _dbContext.SaveChanges();
            _dbContext.ChangeTracker.Clear();
            
            CurrentUser = new User();
            InitializeSelections();
            CommandManager.InvalidateRequerySuggested();
            
            MessageBox.Show("User data saved successfully!", "Success", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (DbUpdateException ex)
        {
            _dbContext.ChangeTracker.Clear();
            MessageBox.Show($"An error occurred while saving data: {ex.InnerException?.Message ?? ex.Message}",
                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            _dbContext.ChangeTracker.Clear();
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ExecuteSelectRandomUser()
    {
        try
        {
            var userIds = _dbContext.Users.Select(u => u.Id).ToList();
            if (userIds.Count == 0)
            {
                MessageBox.Show("No users found in the database.", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var random = new Random();
            var randomId = userIds[random.Next(userIds.Count)];

            var randomUser = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == randomId);

            if (randomUser != null)
            {
                CurrentUser = randomUser;
                InitializeSelections();
                CommandManager.InvalidateRequerySuggested();
            }
            else
            {
                MessageBox.Show($"Could not find user with ID {randomId}.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while selecting a random user: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
