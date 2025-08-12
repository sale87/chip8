using Chip8.components;

namespace Chip8.Tests.components;

public class DelayTimerTest
{
    private DelayTimer _delayTimer;

    public DelayTimerTest()
    {
        _delayTimer = new();
    }

    [Fact]
    public void TestTimerStartsWith255()
    {
        Assert.Equal(255, _delayTimer.GetValue());
    }

    [Fact]
    public void TestTimerIsNotStartedAutomatically()
    {
        Assert.Equal(255, _delayTimer.GetValue());
        Thread.Sleep(200);
        Assert.Equal(255, _delayTimer.GetValue());
    }

    [Fact]
    public void TestWhenStartedItSubstractsValue()
    {
        Assert.Equal(255, _delayTimer.GetValue());
        _delayTimer.StartTimer();
        Thread.Sleep(200);
        Assert.NotEqual(255, _delayTimer.GetValue());
    }

}