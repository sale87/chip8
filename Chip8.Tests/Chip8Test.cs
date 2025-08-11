namespace Chip8.Tests;

public class Chip8Test
{
    private Chip8Emu _c8;

    public Chip8Test()
    {
        _c8 = new Chip8Emu();
    }

    [Fact]
    public void TestJumpInstruction()
    {
        byte[] instruction = { 0x12, 0x28 };
        Assert.Equal(0, _c8.pc);
        _c8.Jump(instruction);
        Assert.Equal(0x228, _c8.pc);
    }
}