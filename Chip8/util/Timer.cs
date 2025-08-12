class Timer
{
    public static System.Timers.Timer MakeTimer(Action action)
    {
        System.Timers.Timer _cpuTimer = new(1000.0 / 700); // ~ 700 times per second      
        _cpuTimer.Elapsed += (source, e) =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                ThreadPool.QueueUserWorkItem(_ => { throw new Exception("Exception on timer.", exception); });
            }
        };
        _cpuTimer.AutoReset = false;
        _cpuTimer.Enabled = true;
        return _cpuTimer;
    }

}