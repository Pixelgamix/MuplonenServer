using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muplonen.Clients;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muplonen.Services
{
    /// <summary>
    /// Services that detects clients that have timed out.
    /// </summary>
    public class PlayerTimeoutDetectionService : IHostedService, IDisposable
    {
        private static readonly int _timeoutInMilliseconds = 30 * 1000; // 30 seconds
        private readonly IPlayerSessionManager _clientManager;
        private readonly ILogger<PlayerTimeoutDetectionService> _logger;
        private Timer? _timer;

        /// <summary>
        /// Creates a new <see cref="PlayerTimeoutDetectionService"/> instance.
        /// </summary>
        /// <param name="clientManager">The client manager.</param>
        /// <param name="logger">Logging.</param>
        public PlayerTimeoutDetectionService(
            IPlayerSessionManager clientManager,
            ILogger<PlayerTimeoutDetectionService> logger)
        {
            _clientManager = clientManager;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
                await _timer.DisposeAsync();

            _timer = new Timer(DetectAndDisconnectTimeoutClients, null, _timeoutInMilliseconds, _timeoutInMilliseconds);
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                await _timer.DisposeAsync();
                _timer = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Detects and disconnects clients that haven't sent a message in a long time.
        /// </summary>
        /// <param name="state">Unused.</param>
        private void DetectAndDisconnectTimeoutClients(object? state)
        {
            var now = DateTime.Now;
            foreach (var session in _clientManager.Clients.Values)
            {
                if ((now - session.Connection.LastMessageReceivedAt).TotalMilliseconds > _timeoutInMilliseconds)
                {
                    _logger.LogInformation("Session {0} timed out. Last message received at {1}. Disconnecting.", session.SessionId, session.Connection.LastMessageReceivedAt);
                    _ = session.Connection.Close();
                }
            }
        }

    }
}
