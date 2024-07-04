using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Services
{
    public class HeartbeatService
    {
        private readonly TimeSpan _timeoutInterval;
        private readonly Action _onTimeout;
        private bool _isRunning;
        private DateTime _lastHeartbeatTime;
        private Timer _timer;

        public HeartbeatService(TimeSpan timeoutInterval, Action onTimeout)
        {
            _timeoutInterval = timeoutInterval;
            _onTimeout = onTimeout;
            _isRunning = false;
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _lastHeartbeatTime = DateTime.UtcNow;
                _timer = new Timer(CheckHeartbeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _timer?.Dispose();
        }

        private void CheckHeartbeat(object state)
        {
            if (DateTime.UtcNow - _lastHeartbeatTime > _timeoutInterval)
            {
                _onTimeout?.Invoke();
            }
        }

        public void UpdateHeartbeatTime()
        {
            _lastHeartbeatTime = DateTime.UtcNow;
        }
    }
}

