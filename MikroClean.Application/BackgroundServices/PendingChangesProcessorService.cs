using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MikroClean.Application.Interfaces;

namespace MikroClean.Application.BackgroundServices
{
    public class PendingChangesProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PendingChangesProcessorService> _logger;
        private readonly TimeSpan _interval;
        private readonly LogLevel _cycleLogLevel;

        public PendingChangesProcessorService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<PendingChangesProcessorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var intervalRaw = configuration["MikroTik:PendingChangesProcessor:IntervalSeconds"];
            var intervalSeconds = int.TryParse(intervalRaw, out var parsedInterval) ? parsedInterval : 30;
            _interval = TimeSpan.FromSeconds(Math.Max(5, intervalSeconds));

            var levelRaw = configuration["MikroTik:PendingChangesProcessor:LogLevel"];
            _cycleLogLevel = Enum.TryParse<LogLevel>(levelRaw, true, out var parsedLevel)
                ? parsedLevel
                : LogLevel.Information;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Pending changes processor iniciado con intervalo de {IntervalSeconds}s", _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                var startedAt = DateTime.UtcNow;
                try
                {
                    _logger.Log(_cycleLogLevel, "[PendingSync] Inicio de ciclo a las {StartedAt:O}", startedAt);

                    using var scope = _scopeFactory.CreateScope();
                    var mikroTikService = scope.ServiceProvider.GetRequiredService<IMikroTikService>();
                    await mikroTikService.ProcessPendingChangesAsync(stoppingToken);

                    var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
                    _logger.Log(_cycleLogLevel, "[PendingSync] Fin de ciclo. Duración: {ElapsedMs} ms", Math.Round(elapsedMs, 2));
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ejecutando procesamiento de pending changes");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Pending changes processor detenido");
        }
    }
}
