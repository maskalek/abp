using MyCompanyName.MyProjectName.Data;
using Serilog;
using Serilog.Events;
using Volo.Abp.FeatureManagement;
using Volo.Abp.PermissionManagement;

namespace MyCompanyName.MyProjectName;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        var loggerConfiguration = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console());

        if (IsMigrateDatabase(args))
        {
            loggerConfiguration.MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            await builder.AddApplicationAsync<MyProjectNameModule>();
            if (IsMigrateDatabase(args))
            {
                builder.Services.Configure<PermissionManagementOptions>(options =>
                {
                    options.IsDynamicPermissionStoreEnabled = false;
                    options.SaveStaticPermissionsToDatabase = false;
                });

                builder.Services.Configure<FeatureManagementOptions>(options =>
                {
                    options.IsDynamicFeatureStoreEnabled = false;
                    options.SaveStaticFeaturesToDatabase = false;
                });
            }
            var app = builder.Build();
            await app.InitializeApplicationAsync();

            if (IsMigrateDatabase(args))
            {
                await app.Services.GetRequiredService<MyProjectNameDbMigrationService>().MigrateAsync();
                return 0;
            }

            Log.Information("Starting MyCompanyName.MyProjectName.");
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
            {
                throw;
            }

            Log.Fatal(ex, "MyCompanyName.MyProjectName terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static bool IsMigrateDatabase(string[] args)
    {
        return args.Any(x => x.Contains("--migrate-database", StringComparison.OrdinalIgnoreCase));
    }
}
