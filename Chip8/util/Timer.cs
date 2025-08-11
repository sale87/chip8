using System.Timers;

class Timer
{
    public static System.Timers.Timer MakeTimer(Action action)
    {
        System.Timers.Timer _cpuTimer = new System.Timers.Timer(1000.0 / 700); // ~ 700 times per second        
        _cpuTimer.Elapsed += (Object? source, ElapsedEventArgs e) => { action(); };
        _cpuTimer.AutoReset = false;
        _cpuTimer.Enabled = true;
        return _cpuTimer;
    }

}