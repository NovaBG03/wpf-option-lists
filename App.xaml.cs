using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WpfApp.Data;
using SQLitePCL;
using WpfApp.Views;
using WpfApp.ViewModels;

namespace WpfApp;

public partial class App : Application
{
    private static IServiceProvider? ServiceProvider { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Batteries_V2.Init();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
        }

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var parentDirectory = new DirectoryInfo(baseDirectory).Parent?.Parent?.Parent?.FullName;
        var dbPath = Path.Combine(parentDirectory ?? baseDirectory, "app.db");
        Console.WriteLine($"db path: {dbPath}");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Filename={dbPath}"));

        services.AddTransient<UserFormViewModel>();
        services.AddTransient<MainWindow>();
    }
}
