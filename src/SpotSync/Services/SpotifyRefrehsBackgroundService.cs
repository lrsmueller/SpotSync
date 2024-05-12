
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using SpotSync.Data;
using SpotSync.Common;
using SpotifyAPI.Web.Http;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;

namespace SpotSync.Services;

public class SpotifyRefrehsBackgroundService : BackgroundService
{
    private readonly ILogger<SpotifyRefrehsBackgroundService> _logger;

    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    private readonly IOptions<SpotifyOptions> _spotifySettings ;

    private DateTime _nextRun;

    public SpotifyRefrehsBackgroundService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<SpotifyRefrehsBackgroundService> logger, IOptions<SpotifyOptions> spotifySettings)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _spotifySettings = spotifySettings;
        var now = DateTime.Now;

        _nextRun = new(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0);
        _logger.LogInformation($"{nameof(SpotifyRefrehsBackgroundService)} started; Next Run is at {_nextRun}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            using var timer = new PeriodicTimer(_nextRun.Subtract(DateTime.Now), TimeProvider.System);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunSync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"{nameof(SpotifyRefrehsBackgroundService)} is stopping.");
            }

            //if (_nextRun<=DateTime.Now)
            //{
            //    await RunSync(stoppingToken);
            //} else
            //{
            //    await Task.Delay(60000, stoppingToken);
            //}
        }
        while(!stoppingToken.IsCancellationRequested);
    }

    private async Task RunSync(CancellationToken cancellationToken =default)
    {
        var hour = _nextRun.Hour;
        var users = await GetUsers(hour,cancellationToken);
        _logger.LogInformation($"next Run Started. Hour: {hour}, Syning  {users.Count}");
        var configs = await GetSpotifyClientConfigs(users,cancellationToken);
        
        foreach(var config in configs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var _ = new SpotifyClient(config.Config);
            await SpotifyService.RefreshSelectedPlaylistAsync(_, config.User.SelectedPlaylist, cancellationToken);

        }

        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Users
            .Where(y=>users.Select(x=>x.Id).Contains(y.Id))
            .ExecuteUpdateAsync(str => str.SetProperty(b => b.LastRefresh, _nextRun), cancellationToken);
        _nextRun = _nextRun.AddHours(1);
        _logger.LogInformation($"Run Finished. Nex Run is at {_nextRun}");
    }

    private async Task<List<ApplicationUser>> GetUsers(int hour, CancellationToken cancellationToken =default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .Where(x=>x.RefreshHour == hour)
            //.Select(x=>new (x.Id,x.SelectedPlaylist))
            .ToListAsync(cancellationToken);
    }
     
    private async Task<List<UserSpotufyClientConfig>> GetSpotifyClientConfigs(IEnumerable<ApplicationUser> users, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var userIds = users.Select(x => x.Id);

        var tokens = await context.UserTokens
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(x=>x.Key,cancellationToken);

        List<UserSpotufyClientConfig> responses = new();
        foreach(var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var authorizationResponse = SpotifyToken.CreateAuthResponse(tokens[user.Id].ToList());
            var config = SpotifyClientConfig
                .CreateDefault()
                .WithHTTPLogger(new SimpleConsoleHTTPLogger())
                .WithAuthenticator(new AuthorizationCodeAuthenticator(_spotifySettings.Value.SpotifyClientId, _spotifySettings.Value.SpotifyClientSecret, authorizationResponse));

            responses.Add(new(config, user));
        }
        return responses;
    }

    public record UserSpotufyClientConfig(SpotifyClientConfig Config, ApplicationUser User);

}
