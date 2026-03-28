using Feedpipe.Core.Services;
using Feedpipe.Models;
using Microsoft.Extensions.Options;

namespace Feedpipe.Worker;

public class Worker(
    IFeedFetcher fetcher,
    IFeedWriter writer,
    IOptions<AppSettings> settings,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Feed pipeline starting");

            foreach (var feed in settings.Value.Feeds)
            {
                var items = await fetcher.FetchAsync(feed.Url);
                if (items.Count > 0)
                {
                    await writer.WriteAsync(items, feed.Name);
                }
            }

            logger.LogInformation("Feed pipeline complete. Next run in 5 minutes");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
