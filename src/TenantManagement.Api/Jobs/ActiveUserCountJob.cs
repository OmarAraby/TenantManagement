using Hangfire;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Api.Jobs;

public sealed class ActiveUserCountJob
{
    private readonly ITenantStats _tenantStats;
    private readonly ILogger<ActiveUserCountJob> _logger;

    public ActiveUserCountJob(ITenantStats tenantStats, ILogger<ActiveUserCountJob> logger)
    {
        _tenantStats = tenantStats;
        _logger = logger;
    }

    // The job runs outside any request
    [DisableConcurrentExecution(timeoutInSeconds: 60)]  
    [AutomaticRetry(Attempts = 2)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var counts = await _tenantStats.GetActiveUserCountsAsync(cancellationToken);

        foreach (var count in counts)
        {
            _logger.LogInformation(
                "Tenant {TenantName} has {ActiveUserCount} active user(s).",
                count.TenantName,
                count.ActiveUserCount);
        }
    }
}
