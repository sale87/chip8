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

        private void OnTimedEvent(Object? source, ElapsedEventArgs e)
        {
            _delayTimer--; // as byte is unsigned once value reaches 0 next value would be 255
        }

    }
}