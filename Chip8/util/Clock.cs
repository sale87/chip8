using System.Diagnostics;

namespace Chip8.util
{
    public class Clock
    {
        // = (TimeSpan.TicksPerSecond / 500)
        private const int TicksPer500Hz = 20_000;

        private readonly Action _tick500Hz;
        private readonly Stopwatch _stopwatch;

        private long _last500Hz;

        public Clock(Action tick500Hz, bool running = true)
        {
            _stopwatch = Stopwatch.StartNew();
            _tick500Hz = tick500Hz;

            Running = running;
            Task.Run(Loop);
        }

        public bool Running { get; set; }

        private void Loop()
        {
            while (true)
            {
                if (_stopwatch.ElapsedTicks - _last500Hz > TicksPer500Hz)
                {
                    _last500Hz = _stopwatch.ElapsedTicks;
                    if (Running)
                    {
                        try
                        {
                            _tick500Hz();
                        }
                        catch (Exception exception)
                        {
                            ThreadPool.QueueUserWorkItem(_ =>
                                throw new Exception("Instruction exec exception.", exception));
                        }
                    }
                }

                var sleepFor = TimeSpan.FromTicks(Math.Min(0, _stopwatch.ElapsedTicks - _last500Hz));
                Thread.Sleep(sleepFor);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}