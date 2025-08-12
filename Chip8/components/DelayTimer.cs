using System.Timers;

namespace Chip8.components
{
    public class DelayTimer
    {
        private byte _delayTimer = 255;
        private System.Timers.Timer _dTimer;

        public DelayTimer()
        {
            _dTimer = new System.Timers.Timer(1000.0 / 60); // ~ 60 times per second
            _dTimer.Elapsed += OnTimedEvent;
            _dTimer.AutoReset = true;
        }

        public void StartTimer()
        {
            _dTimer.Start();
        }

        public byte GetValue()
        {
            return _delayTimer;
        }

        internal void SetValue(byte v)
        {
            lock (this)
            {
                _delayTimer = v;
            }
        }

        private void OnTimedEvent(Object? source, ElapsedEventArgs e)
        {
            lock (this)
            {
                if (_delayTimer > 0)
                {
                    SetValue((byte)(_delayTimer - 1));
                }
            }
        }
    }
}