using System.Timers;
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

    private System.Timers.Timer _cpuTimer;

    private byte SoundTimer = 255;

    // general purpose variable registers, V0 - VF
    private readonly byte[] Registers = new byte[16];

    public Chip8Emu()
    {
        Font.Load(_memory);
        _cpuTimer = new System.Timers.Timer(1000.0 / 700); // ~ 700 times per second        
        _cpuTimer.Elapsed += ExecuteCycle;
        _cpuTimer.AutoReset = true;
    }

    public void LoadRom(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        _pc = 0x200;
        _memory.SetMemory(0x200, bytes);
    }

    public void RunCycle()
    {
        byte[] instruction = FetchInstruction();
        DecodeInstruction(instruction);
        ExecuteInstruction();
    }

    private byte[] FetchInstruction()
    {
        byte[] bytes = _memory.ReadMemory(_pc, 2);
        _pc += 2;
        return bytes;
    }

    private void DecodeInstruction(byte[] instruction)
    {
        switch (instruction[0] >> 4)
        {
            case 0x0:
                Console.WriteLine("00E0 (clear screen)");
                break;
            case 0x1:
                Console.WriteLine("1NNN (jump)");
                break;                
            case 0x6:
                Console.WriteLine("6XNN (set register VX)");
                break;                
            case 0x7:
                Console.WriteLine("7XNN (add value to register VX)");
                break;                
            case 0xA:
                Console.WriteLine("ANNN (set index register I)");
                break;                
            case 0xD:
                Console.WriteLine("DXYN (display/draw)");
                break;                
            default:
                break;
        }
        Console.WriteLine($"{Convert.ToHexString(instruction)}");
    }

    private void ExecuteInstruction()
    {
        
    }

    private void ExecuteCycle(Object? source, ElapsedEventArgs e)
    {
        RunCycle();
    }

}