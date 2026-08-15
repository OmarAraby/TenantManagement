using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string SettingsFileName = "appsettings.json";
    private const string ApiProjectRelativePath = "src/TenantManagement.Api";

    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var connectionString = configuration.GetConnectionString(DependencyInjection.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{DependencyInjection.ConnectionStringName}' was not found in {SettingsFileName}.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options, new NoTenantContext());
    }

    private static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(ResolveSettingsDirectory())
            .AddJsonFile(SettingsFileName, optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string ResolveSettingsDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SettingsFileName)))
            {
                return directory.FullName;
            }

            var apiProject = Path.Combine(directory.FullName, ApiProjectRelativePath);

            if (File.Exists(Path.Combine(apiProject, SettingsFileName)))
            {
                return apiProject;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate {SettingsFileName}. Run EF Core commands from the repository root or from a project directory beneath it.");
    }

    private sealed class NoTenantContext : ITenantContext
    {
        public Guid? TenantId => null;

        public bool HasTenant => false;
    }
}
