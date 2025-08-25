using System.Diagnostics;

namespace Chip8.util
{
    public class Clock
    {
        private readonly Action _tickAction;
        private readonly Stopwatch _stopwatch;

        private long _lastTick;
        private long _ticksPerExecution;

        // Default to 700Hz execution speed
        public int ExecutionSpeed { get; private set; } = 700;

        public Clock(Action tickAction, bool running = true)
        {
            _stopwatch = Stopwatch.StartNew();
            _tickAction = tickAction;
            SetExecutionSpeed(ExecutionSpeed); // Initialize with default speed

            Running = running;
            Task.Run(Loop);
        }

        public bool Running { get; set; }

        public void SetExecutionSpeed(int hz)
        {
            ExecutionSpeed = Math.Max(1, Math.Min(1000, hz)); // Clamp between 1Hz and 1kHz  
            _ticksPerExecution = TimeSpan.TicksPerSecond / ExecutionSpeed;
        }

        private void Loop()
        {
            while (true)
            {
                if (_stopwatch.ElapsedTicks - _lastTick > _ticksPerExecution)
                {
                    _lastTick = _stopwatch.ElapsedTicks;
                    if (Running)
                    {
                        try
                        {
                            _tickAction();
                        }
                        catch (Exception exception)
                        {
                            ThreadPool.QueueUserWorkItem(_ =>
                                throw new Exception("Instruction exec exception.", exception));
                        }
                    }
                }

                var timeUntilNextTick = _ticksPerExecution - (_stopwatch.ElapsedTicks - _lastTick);
                if (timeUntilNextTick > 0)
                {
                    Thread.Sleep(TimeSpan.FromTicks(timeUntilNextTick));
                }
                else
                {
                    Thread.Sleep(1); // Small sleep to prevent 100% CPU usage
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}