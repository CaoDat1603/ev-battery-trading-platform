using Auction.Application.Contracts;
using Auction.Domain.Abstractions;
using Auction.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Auction.Application.Services
{
    public class AuctionEndBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuctionEndBackgroundService> _logger;

        public AuctionEndBackgroundService(IServiceProvider serviceProvider,
                                           ILogger<AuctionEndBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuctionEndBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var auctionCommand = scope.ServiceProvider.GetRequiredService<IAuctionCommand>();
                    var auctionRepo = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();

                    var now = DateTimeOffset.UtcNow;

                    _logger.LogInformation("Checking auctions at: {Time}", now);

                    var auctions = await auctionRepo
                        .GetActiveAuctionsEndingBeforeAsync(now, stoppingToken);

                    int count = auctions.Count();

                    _logger.LogInformation(
                       "Heartbeat: {Count} auctions ending before {Time}",
                        count,
                        now
                    );

                    foreach (var auction in auctions)
                    {
                        _logger.LogInformation("Ending auction {AuctionId}", auction.AuctionId);
                        await auctionCommand.UpdateAuctionStatusAsync(
                            auction.AuctionId,
                            AuctionStatus.Ended,
                            stoppingToken
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AuctionEndBackgroundService");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
