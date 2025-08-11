using System.Timers;
using Chip8.components;
using Chip8.util;

public class Chip8Emu
{
    private readonly Memory _memory = new();

    private readonly Display _display = new();

    // program counter, points to current instruction in memory, possible values 0 - 4096
    public short pc = 0;

    // points to locations in memory
    private ushort _instruction_register = 0;

    // general purpose variable registers, V0 - VF
    private readonly byte[] registers = new byte[16];

    // holds call stack address information
    private readonly Stack<short> _stack = new();

    private DelayTimer _delayTimer = new();

    private System.Timers.Timer _cpuTimer;

    private byte _soundTimer = 255;

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
        pc = 0x200;
        _memory.SetMemory(0x200, bytes);
    }

    public void RunCycle()
    {
        byte[] instruction = FetchInstruction();
        ExecuteInstruction(instruction);
    }

    private byte[] FetchInstruction()
    {
        byte[] bytes = _memory.ReadMemory(pc, 2);
        pc += 2;
        return bytes;
    }

    private void ExecuteInstruction(byte[] instruction)
    {
        switch (instruction[0] >> 4)
        {
            case 0x0:
                ClearScreen();
                break;
            case 0x1:
                Jump(instruction);
                break;
            case 0x6:
                SetRegister(instruction);
                break;
            case 0x7:
                Add(instruction);
                break;
            case 0xA:
                SetInstructionRegister(instruction);
                break;
            case 0xD:
                Draw();
                break;
            default:
                Console.WriteLine($"{Convert.ToHexString(instruction)}");
                break;
        }
    }

    private void Add(byte[] instruction)
    {
        int register = instruction[0] & 0b00001111;
        registers[register] += instruction[1];
        Console.WriteLine($"add {register} 0x{instruction[1]:X4}");
    }

    private void SetRegister(byte[] instruction)
    {
        int register = instruction[0] & 0b00001111;
        registers[register] = instruction[1];
        Console.WriteLine($"set {register} 0x{instruction[1]:X4}");
    }

    public void Jump(byte[] instruction)
    {
        pc = (short)(((instruction[0] & 0b1111) << 8) + instruction[1]);
        Console.WriteLine($"jump 0x{pc:X4}");
    }

    private void ClearScreen()
    {
        Console.WriteLine("cls");
        _display.InitGrid();
    }

    private void SetInstructionRegister(byte[] instruction)
    {
        _instruction_register = (ushort)(((instruction[0] & 0b1111) << 8) + instruction[1]);
        Console.WriteLine($"set_i 0x{_instruction_register:X4}");
    }

    private static void Draw()
    {
        Console.WriteLine("draw");
    }

    private void ExecuteCycle(Object? source, ElapsedEventArgs e)
    {
        RunCycle();
    }

    private static string HexStr(byte[] arr)
    {
        return Convert.ToHexString(arr);
    }

}