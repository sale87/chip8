using Chip8.components;

namespace Chip8.Tests.components;

public class MemoryTest
{
    private readonly Memory _memory;

    public MemoryTest()
    {
        _memory = new();
    }

    [Fact]
    public void TestMemoryLength()
    {
        Assert.Equal(4096, _memory.ReadAll().Length);
    }

    [Fact]
    public void TestReadMemory()
    {
        Assert.Equal(0x00, _memory.ReadMemory(0xF0, 1)[0]);
    }

    [Fact]
    public void TestSetMemory()
    {
        short addr = 0xFF;
        byte data = 0xF0;
        _memory.SetMemory(addr, data);
        Assert.Equal(data, _memory.ReadMemory(addr, 1)[0]);
    }

        [Fact]
    public void TestSetMemoryArray()
    {
        short addr = 0xFF;
        byte[] data = {0xF0, 0xA1};
        _memory.SetMemory(addr, data);
        Assert.Equal(data, _memory.ReadMemory(addr, 2));
    }
}