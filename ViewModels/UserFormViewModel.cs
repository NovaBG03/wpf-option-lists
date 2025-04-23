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
    private readonly ApplicationDbContext _dbContext;
    private User _currentUser = new();

    private readonly SystemOptionList<Gender> _genderOptionList = SystemOptionList<Gender>.ForGender();

    private readonly DynamicOptionList _activityOptionList = new();

    private readonly MixedOptionList<ProgrammingLanguage> _technologyOptionList =
        MixedOptionList<ProgrammingLanguage>.ForProgrammingLanguages();

    public User CurrentUser
    {
        get => _currentUser;
        private set => SetProperty(ref _currentUser, value);
    }

    public IEnumerable<BaseOption> GenderOptions => _genderOptionList.Options;

    public SystemOption<Gender>? SelectedGenderOption
    {
        get => _genderOptionList.SelectedOption;
        set
        {
            _genderOptionList.SelectedOption = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<BaseOption> ActivityOptions => _activityOptionList.Options;

    public DynamicOption? SelectedActivityOption
    {
        get => _activityOptionList.SelectedOption;
        set
        {
            _activityOptionList.SelectedOption = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<BaseOption> TechnologyOptions => _technologyOptionList.Options;

    public BaseOption? SelectedTechnologyOption
    {
        get => _technologyOptionList.SelectedOption;
        set
        {
            _technologyOptionList.SelectedOption = value;
            OnPropertyChanged();
            if (_technologyOptionList.IsSelected(ProgrammingLanguage.Java))
            {
                MessageBox.Show($"Java!?", "Are you sure?", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        UpdateListSelections();
    }

    private void LoadData()
    {
        LoadActivityOptions();
        LoadTechnologyOptions();
    }

    private void LoadActivityOptions()
    {
        var activityOptions = _dbContext.Activities.AsNoTracking().Select(x => new DynamicOption()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToList();
        _activityOptionList.SetOptions(activityOptions);
        OnPropertyChanged(nameof(ActivityOptions));
        OnPropertyChanged(nameof(SelectedActivityOption));
    }

    private void LoadTechnologyOptions()
    {
        var technologyOptions = _dbContext.Technologies.AsNoTracking().Select(x => new DynamicOption()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToList();
        _technologyOptionList.SetOptions(technologyOptions);
        OnPropertyChanged(nameof(TechnologyOptions));
        OnPropertyChanged(nameof(SelectedTechnologyOption));
    }

    private void UpdateListSelections()
    {
        _genderOptionList.SelectOption(CurrentUser.Gender);
        OnPropertyChanged(nameof(SelectedGenderOption));

        _activityOptionList.SelectOption(CurrentUser.FavouriteActivityId);
        OnPropertyChanged(nameof(SelectedActivityOption));

        if (CurrentUser.FavouriteProgrammingLanguage is not null)
        {
            _technologyOptionList.SelectOption(CurrentUser.FavouriteProgrammingLanguage);
        }
        else if (CurrentUser.FavouriteTechnology is not null)
        {
            _technologyOptionList.SelectOption(CurrentUser.FavouriteTechnology.Id);
        }
        else
        {
            _technologyOptionList.ClearSelection();
        }

        OnPropertyChanged(nameof(SelectedTechnologyOption));
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
            _activityOptionList.SelectOption(newActivity.Id);
            OnPropertyChanged(nameof(SelectedActivityOption));
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

        if (_technologyOptionList.ContainsSystemOptionWithName(newItemName))
        {
            MessageBox.Show($"'{newItemName}' is a system-defined programming language and cannot be added again.",
                "System Language", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_technologyOptionList.ContainsDynamicOptionWithName(newItemName))
        {
            MessageBox.Show($"A technology named '{newItemName}' already exists.", "Duplicate Technology",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        var newTechnology = new Technology { Name = newItemName };
        _dbContext.Technologies.Add(newTechnology);

        try
        {
            _dbContext.SaveChanges();
            LoadTechnologyOptions();
            _technologyOptionList.SelectOption(newTechnology.Id);
            OnPropertyChanged(nameof(SelectedTechnologyOption));
        }
        catch (DbUpdateException ex)
        {
            _dbContext.ChangeTracker.Clear();
            if (ex.InnerException != null &&
                ex.InnerException.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"Can not save. A technology named '{newItemName}' already exists.",
                    "Duplicate Technology",
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

    private bool CanExecuteSave() =>
        SelectedGenderOption is not null &&
        SelectedActivityOption is not null &&
        SelectedTechnologyOption is not null;

    private void ExecuteSave()
    {
        if (!CanExecuteSave()) return;

        CurrentUser.Gender = SelectedGenderOption!.SystemId;

        CurrentUser.FavouriteActivity = null;
        CurrentUser.FavouriteActivityId = SelectedActivityOption!.Id;

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
                    CurrentUser.FavouriteTechnology = null;
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
            UpdateListSelections();
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

            var randomUser = _dbContext.Users.AsNoTracking()
                .Include(u => u.FavouriteActivity)
                .Include(u => u.FavouriteTechnology)
                .FirstOrDefault(u => u.Id == randomId);

            if (randomUser is not null)
            {
                CurrentUser = randomUser;
                UpdateListSelections();
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
