using Chip8.components;
using Chip8.util;

public class Chip8Emu
{
    private readonly Memory _memory = new();

    // display size is 64x32, so we are storing 32 64bit numbers
    private readonly ulong[] _display = new ulong[32];

    // program counter, points to current instruction in memory, possible values 0 - 4096
    private short _pc = 0;

    // points to locations in memory
    private ushort _instruction_register = 0;

    // holds call stack address information
    private readonly Stack<short> _stack = new();

    private DelayTimer _delayTimer = new();

    private byte SoundTimer = 255;

    // general purpose variable registers, V0 - VF
    private readonly byte[] Registers = new byte[16];

    public Chip8Emu()
    {
        Font.Load(_memory);
    }

}