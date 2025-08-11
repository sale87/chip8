using Chip8.components;
using Chip8.util;
using SDL3;

namespace Chip8
{
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

        // used to simulate CPU clock
        private System.Timers.Timer _cpuTimer = new(1000.0 / 700); // ~ 700 times per second

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
                    SystemInstruction(instruction);
                    break;
                case 0x1:
                    Jump(instruction);
                    break;
                case 0x2:
                    Call(instruction);
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
                case 0x8:
                    LogicOrArithmeticInstruction(instruction);
                    break;
                case 0xA:
                    SetIndexRegister(instruction);
                    break;
                case 0xD:
                    Draw(instruction);
                    break;
                default:
                    throw new Exception($"Unkown instruction: {Convert.ToHexString(instruction)}");
            }
        }

        private void SystemInstruction(byte[] instruction)
        {
            switch (instruction[1])
            {
                case 0xE0:
                    ClearScreen();
                    break;
                case 0xEE:
                    Return(instruction);
                    break;
                default:
                    throw new Exception($"Unkown instruction: {Convert.ToHexString(instruction)}");
            }
        }

        private void ClearScreen()
        {
            // 00E0 - clear screen
            if (DEBUG) Console.WriteLine("cls");
            _display.InitGrid();
        }

        public void Return(byte[] instruction)
        {
            // 00EE - return from subroutine
            pc = _stack.Pop();
            if (DEBUG) Console.WriteLine($"return 0x{pc:X4}");
        }

        public void Jump(byte[] instruction)
        {
            // 1NNN - set PC to NNN
            pc = (short)NNN(instruction);
            if (DEBUG) Console.WriteLine($"jump 0x{pc:X4}");
        }

        public void Call(byte[] instruction)
        {
            // 2NNN - Call routine at NNN
            _stack.Push(pc);
            pc = (short)NNN(instruction);
            if (DEBUG) Console.WriteLine($"call 0x{pc:X4}");
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

        private void LogicOrArithmeticInstruction(byte[] instruction)
        {
            // 8XYI - Logic or arithmetic instruction involving vX and vY registers
            int vX = SecondNibble(instruction[0]);
            int vY = FirstNibble(instruction[1]);
            int i = SecondNibble(instruction[1]);

            switch (i)
            {
                case 0x0:
                    // 8XY0: Set vX to vY
                    registers[vX] = registers[vY];
                    if (DEBUG) Console.WriteLine($"v{vX:X} = v{vY:X}");
                    break;
                case 0x1:
                    // 8XY1: Binary OR
                    registers[vX] |= registers[vY];
                    if (DEBUG) Console.WriteLine($"v{vX:X} |= v{vY:X}");
                    break;
                case 0x2:
                    // 8XY1: Binary AND
                    registers[vX] &= registers[vY];
                    if (DEBUG) Console.WriteLine($"v{vX:X} &= v{vY:X}");
                    break;
                case 0x3:
                    // 8XY3: Logical XOR
                    registers[vX] ^= registers[vY];
                    if (DEBUG) Console.WriteLine($"v{vX:X} ^= v{vY:X}");
                    break;
                case 0x4:
                    // 8XY4: Add vY to vX and set to vX
                    registers[0xF] = 0;
                    if ((int)registers[vX] + (int)registers[vY] > 255)
                    {
                        // in case of overflow set vF to 1
                        registers[0xF] = 1;
                    }
                    registers[vX] += registers[vY];
                    if (DEBUG) Console.WriteLine($"v{vX:X} += v{vY:X}");
                    break;
                case 0x5:
                    // 8XY5: Subtract vY from vX and set to vX
                    registers[0xF] = (byte)((registers[vX] > registers[vY]) ? 1 : 0);
                    registers[vX] -= registers[vY];
                    if (DEBUG) Console.WriteLine($"v{vX:X} = v{vX:X} - v{vY:X}");
                    break;
                case 0x7:
                    // 8XY7: Subtract vX from vY and set to vX
                    registers[0xF] = (byte)((registers[vY] > registers[vX]) ? 1 : 0);
                    registers[vX] = (byte)(registers[vY] - registers[vX]);
                    if (DEBUG) Console.WriteLine($"v{vX:X} = v{vY:X} - v{vX:X}");
                    break;
                case 0x6:
                    // 8XY6: Shift vX to the right
                    registers[0xF] = (byte)(registers[vX] & 0b1);
                    registers[vX] >>= 1;
                    if (DEBUG) Console.WriteLine($"v{vX:X} >>= 1");
                    break;
                case 0xE:
                    // 8XY6: Shift vX to the left
                    registers[0xF] = (byte)(registers[vX] & 0b10000000);
                    registers[vX] <<= 1;
                    if (DEBUG) Console.WriteLine($"v{vX:X} <<= 1");
                    break;
            }

            _index_register = NNN(instruction);
            if (DEBUG) Console.WriteLine($"set_i 0x{_index_register:X4}");
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
            int vX = SecondNibble(instruction[0]);
            int vY = FirstNibble(instruction[1]);
            int n = SecondNibble(instruction[1]);
            int x = registers[vX] % Display.WIDTH;
            int y = registers[vY] % Display.HEIGHT;

            if (DEBUG) Console.WriteLine($"draw v{vX:X}, v{vY:X}, {n}");

            byte[] sprite = _memory.ReadMemory((short)_index_register, (short)n);
            registers[0xf] = 0;

            for (int yi = 0; yi < n; yi++)
            {
                int effectiveY = y + yi;
                if (effectiveY >= Display.HEIGHT)
                {
                    break;
                }
                for (int xi = 0; xi < 8; xi++)
                {
                    byte mask = (byte)(0b10000000 >> xi);
                    int effectiveX = x + xi;
                    if (effectiveX >= Display.WIDTH)
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
}