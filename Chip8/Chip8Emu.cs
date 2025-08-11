using Chip8.components;
using Chip8.util;
using SDL3;

public class Chip8Emu
{
    private readonly bool DEBUG_RAW = true;
    private readonly bool DEBUG = true;

    private readonly Memory _memory = new();

    private readonly Display _display = new();

    // program counter, points to current instruction in memory, possible values 0 - 4096
    public short pc = 0;

    // points to locations in memory
    private short _index_register = 0;

    // general purpose variable registers, V0 - VF
    private readonly byte[] registers = new byte[16];

    // holds call stack address information
    private readonly Stack<short> _stack = new();

    private DelayTimer _delayTimer = new();

    private System.Timers.Timer _cpuTimer;

    private byte _soundTimer = 255;

    // SDL things
    private static bool loop = true;
    private static readonly uint FPS = 60;
    private static readonly uint FRAME_TARGET_TIME = 1000 / FPS;
    private static ulong lastFameTime = 0;


    public Chip8Emu()
    {
        Font.Load(_memory);
    }

    public void LoadRom(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        pc = 0x200;
        _memory.SetMemory(0x200, bytes);
    }

    public void Start()
    {
        _cpuTimer = Timer.MakeTimer(RunCycle);
        // main SDL loop
        while (loop)
        {
            Wait();
            ProcessInput();
            _display.Draw();
        }
        _display.Close();
    }

    private static double Wait()
    {
        uint timeToWait = FRAME_TARGET_TIME - (uint)(SDL.GetTicks() - lastFameTime);
        if (timeToWait > 0 && timeToWait <= FRAME_TARGET_TIME)
        {
            SDL.Delay(timeToWait);
        }
        double deltaTime = (SDL.GetTicks() - lastFameTime) / 1000.0;
        lastFameTime = SDL.GetTicks();
        return deltaTime;
    }

    private static void ProcessInput()
    {
        while (SDL.PollEvent(out var e))
        {
            SDL.EventType type = (SDL.EventType)e.Type;
            if (type == SDL.EventType.Quit)
            {
                loop = false;
            }

            if (type == SDL.EventType.KeyDown)
            {
                SDL.Keycode key = e.Key.Key;
                if (key == SDL.Keycode.Escape || key == SDL.Keycode.Q)
                {
                    loop = false;
                }
            }
        }
    }

    public void RunCycle()
    {
        byte[] instruction = FetchInstruction();
        ExecuteInstruction(instruction);
        _cpuTimer.Close();
        _cpuTimer.Dispose();
        _cpuTimer = Timer.MakeTimer(RunCycle);
    }

    private byte[] FetchInstruction()
    {
        byte[] bytes = _memory.ReadMemory(pc, 2);
        pc += 2;
        return bytes;
    }

    private void ExecuteInstruction(byte[] instruction)
    {
        if (DEBUG_RAW) Console.Out.WriteLine($"----\n{HexStr(instruction)}");

        switch (instruction[0] >> 4)
        {
            case 0x0:
                ClearScreen();
                break;
            case 0x1:
                Jump(instruction);
                break;
            case 0x3:
                SkipIfVXeqN(instruction);
                break;
            case 0x4:
                SkipIfVXneqN(instruction);
                break;
            case 0x5:
                SkipIfVXeqVY(instruction);
                break;
            case 0x9:
                SkipIfVXneqVY(instruction);
                break;
            case 0x6:
                SetRegister(instruction);
                break;
            case 0x7:
                Add(instruction);
                break;
            case 0xA:
                SetIndexRegister(instruction);
                break;
            case 0xD:
                Draw(instruction);
                break;
            default:
                Console.WriteLine($"{Convert.ToHexString(instruction)}");
                throw new Exception("Unkown instruction");
        }
    }

    private void ClearScreen()
    {
        // 00E0 - clear screen
        if (DEBUG) Console.WriteLine("cls");
        _display.InitGrid();
    }

    public void Jump(byte[] instruction)
    {
        // 1NNN - set PC to NNN
        pc = (short)NNN(instruction);
        if (DEBUG) Console.WriteLine($"jump 0x{pc:X4}");
    }

    public void SkipIfVXeqN(byte[] instruction)
    {
        // 3XNN - if vX == NN, skip next instruction
        int vX = SecondNibble(instruction[0]);
        if (registers[vX] == instruction[1])
        {
            pc += 2;
        }
        if (DEBUG) Console.WriteLine($"skip_if v{vX:X} == 0x{instruction[1]:X2}");
    }

    public void SkipIfVXneqN(byte[] instruction)
    {
        // 4XNN - if vX != NN, skip next instruction
        int vX = SecondNibble(instruction[0]);
        if (registers[vX] != instruction[1])
        {
            pc += 2;
        }
        if (DEBUG) Console.WriteLine($"skip_if v{vX:X} != 0x{instruction[1]:X2}");
    }

    public void SkipIfVXeqVY(byte[] instruction)
    {
        // 5XY0 - if VX == VY, skip next instruction
        int vX = SecondNibble(instruction[0]);
        int vY = FirstNibble(instruction[1]);
        if (registers[vX] == registers[vY])
        {
            pc += 2;
        }
        if (DEBUG) Console.WriteLine($"skip_if v{vX:X} == v{vY:X}");
    }

    public void SkipIfVXneqVY(byte[] instruction)
    {
        // 9XY0 - if VX != VY, skip next instruction
        int vX = SecondNibble(instruction[0]);
        int vY = FirstNibble(instruction[1]);
        if (registers[vX] != registers[vY])
        {
            pc += 2;
        }
        if (DEBUG) Console.WriteLine($"skip_if v{vX:X} != v{vY:X}");
    }

    private void SetRegister(byte[] instruction)
    {
        // 6XNN - set the register VX to the value NN
        int vX = SecondNibble(instruction[0]);
        registers[vX] = instruction[1];
        if (DEBUG) Console.WriteLine($"set v{vX:X} 0x{instruction[1]:X2}");
    }

    private void Add(byte[] instruction)
    {
        // 7XNN - add the value NN to VX
        int vX = SecondNibble(instruction[0]);
        registers[vX] += instruction[1];
        if (DEBUG) Console.WriteLine($"add v{vX:X} 0x{instruction[1]:X2}");
    }

    private void SetIndexRegister(byte[] instruction)
    {
        // ANNN - set the index register I to the value NNN
        _index_register = NNN(instruction);
        if (DEBUG) Console.WriteLine($"set_i 0x{_index_register:X4}");
    }

    private void Draw(byte[] instruction)
    {
        // DXYN - draw an N pixels tall sprite 
        // from the memory location that the I index register is holding to the screen, 
        // at the horizontal X coordinate in VX and the Y coordinate in VY
        int x_reg = SecondNibble(instruction[0]);
        int y_reg = FirstNibble(instruction[1]);
        int x = registers[x_reg] % Display.WIDTH;
        int y = registers[y_reg] % Display.HEIGHT;
        int n = SecondNibble(instruction[1]);

        if (DEBUG) Console.WriteLine($"draw v{x_reg:X}, v{y_reg:X}, {n}");

        byte[] sprite = _memory.ReadMemory((short)_index_register, (short)n);
        registers[0xf] = 0;

        for (int yi = 0; yi < n; yi++)
        {
            int effectiveY = y + yi - 1;
            if (effectiveY > Display.HEIGHT)
            {
                break;
            }
            for (int xi = 0; xi < 8; xi++)
            {
                byte mask = (byte)(0b10000000 >> xi);
                int effectiveX = x + xi - 1;
                if (effectiveX > Display.WIDTH)
                {
                    break;
                }
                var pixelOn = (sprite[yi] & mask) != 0;
                bool previousValue = _display.GetPixel(effectiveX, effectiveY);
                if (previousValue && pixelOn)
                {
                    registers[0xf] = 1;
                    _display.SetPixel(effectiveX, effectiveY, false);
                }
                else
                {
                    _display.SetPixel(effectiveX, effectiveY, pixelOn);
                }
            }
        }
    }

    private static int FirstNibble(byte halfInstruction)
    {
        return (halfInstruction & 0b11110000) >> 4;
    }

    private static int SecondNibble(byte halfInstruction)
    {
        return halfInstruction & 0b00001111;
    }

    private static short NNN(byte[] instruction)
    {
        return (short)((SecondNibble(instruction[0]) << 8) + instruction[1]);
    }

    private static string HexStr(byte[] arr)
    {
        return Convert.ToHexString(arr);
    }

}