using Chip8.components;
using Chip8.util;

namespace Chip8
{
    public class Chip8Emu
    {
        private readonly bool DEBUG_RAW = false;
        private readonly bool DEBUG = false;

        private readonly Memory _memory = new();

        private readonly Display _display = new();

        private readonly Keyboard _keyboard = new();

        private readonly DelayTimer _delayTimer = new();

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

        public Chip8Emu()
        {
            Font.Load(_memory);
        }

        public void Run(string path)
        {
            LoadRom(path);
            _cpuTimer = Timer.MakeTimer(RunCycle);
            _delayTimer.StartTimer();
            MainLoop.Run(_display, _keyboard);
            _display.Close();
        }

        private void LoadRom(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            pc = 0x200;
            _memory.SetMemory(0x200, bytes);
        }

        internal void RunCycle()
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
                case 0xB:
                    JumpWithOffset(instruction);
                    break;
                case 0xD:
                    Draw(instruction);
                    break;
                case 0xE:
                    EInstruction(instruction);
                    break;
                case 0xF:
                    FInstruction(instruction);
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

        private void Return(byte[] instruction)
        {
            // 00EE - return from subroutine
            pc = _stack.Pop();
            if (DEBUG) Console.WriteLine($"return 0x{pc:X4}");
        }

        private void Jump(byte[] instruction)
        {
            // 1NNN - set PC to NNN
            pc = (short)NNN(instruction);
            if (DEBUG) Console.WriteLine($"jump 0x{pc:X4}");
        }

        private void Call(byte[] instruction)
        {
            // 2NNN - Call routine at NNN
            _stack.Push(pc);
            pc = (short)NNN(instruction);
            if (DEBUG) Console.WriteLine($"call 0x{pc:X4}");
        }

        private void SkipIfVXeqN(byte[] instruction)
        {
            // 3XNN - if vX == NN, skip next instruction
            int vX = SecondNibble(instruction[0]);
            if (registers[vX] == instruction[1])
            {
                pc += 2;
            }
            if (DEBUG) Console.WriteLine($"skip_if v{vX:X} == 0x{instruction[1]:X2}");
        }

        private void SkipIfVXneqN(byte[] instruction)
        {
            // 4XNN - if vX != NN, skip next instruction
            int vX = SecondNibble(instruction[0]);
            if (registers[vX] != instruction[1])
            {
                pc += 2;
            }
            if (DEBUG) Console.WriteLine($"skip_if v{vX:X} != 0x{instruction[1]:X2}");
        }

        private void SkipIfVXeqVY(byte[] instruction)
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

        private void SkipIfVXneqVY(byte[] instruction)
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
                    registers[0xF] = 0;
                    if (DEBUG) Console.WriteLine($"v{vX:X} |= v{vY:X}");
                    break;
                case 0x2:
                    // 8XY1: Binary AND
                    registers[vX] &= registers[vY];
                    registers[0xF] = 0;
                    if (DEBUG) Console.WriteLine($"v{vX:X} &= v{vY:X}");
                    break;
                case 0x3:
                    // 8XY3: Logical XOR
                    registers[vX] ^= registers[vY];
                    registers[0xF] = 0;
                    if (DEBUG) Console.WriteLine($"v{vX:X} ^= v{vY:X}");
                    break;
                case 0x4:
                    // 8XY4: Add vY to vX and set to vX
                    byte pre8XY4 = registers[vX];
                    registers[vX] += registers[vY];
                    // in case of overflow set vF to 1
                    registers[0xF] = (byte)((pre8XY4 > registers[vX]) ? 1 : 0);
                    if (DEBUG) Console.WriteLine($"v{vX:X} += v{vY:X}");
                    break;
                case 0x5:
                    // 8XY5: Subtract vY from vX and set to vX
                    byte pre8XY5 = registers[vX];
                    registers[vX] -= registers[vY];
                    // in case of underflow set vF to 1
                    registers[0xF] = (byte)((pre8XY5 < registers[vX]) ? 0 : 1);
                    if (DEBUG) Console.WriteLine($"v{vX:X} = v{vX:X} - v{vY:X}");
                    break;
                case 0x6:
                    // 8XY6: Shift vX to the right
                    byte pre8XY6 = registers[vX];
                    registers[vX] >>= 1;
                    registers[0xF] = (byte)(pre8XY6 & 0b1);
                    if (DEBUG) Console.WriteLine($"v{vX:X} >>= 1");
                    break;
                case 0x7:
                    // 8XY7: Subtract vX from vY and set to vX
                    byte pre8XY7 = registers[vX];
                    registers[vX] = (byte)(registers[vY] - registers[vX]);
                    registers[0xF] = (byte)((pre8XY7 > registers[vY]) ? 0 : 1);
                    if (DEBUG) Console.WriteLine($"v{vX:X} = v{vY:X} - v{vX:X}");
                    break;
                case 0xE:
                    // 8XYE: Shift vX to the left
                    byte pre8XYE = registers[vX];
                    registers[vX] <<= 1;
                    registers[0xF] = (byte)((pre8XYE & 0b10000000) >> 7);
                    if (DEBUG) Console.WriteLine($"v{vX:X} <<= 1");
                    break;
            }

            if (DEBUG) Console.WriteLine($"set_i 0x{_index_register:X4}");
        }

        private void SetIndexRegister(byte[] instruction)
        {
            // ANNN - set the index register I to the value NNN
            _index_register = NNN(instruction);
            if (DEBUG) Console.WriteLine($"set_i 0x{_index_register:X4}");
        }

        private void JumpWithOffset(byte[] instruction)
        {
            // 2NNN - set PC to v0 + NNN
            pc = (short)(registers[0] + NNN(instruction));
            if (DEBUG) Console.WriteLine($"jump 0x{pc:X4}");
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
                    else if (pixelOn)
                    {
                        _display.SetPixel(effectiveX, effectiveY, pixelOn);
                    }
                }
            }
        }

        private void EInstruction(byte[] instruction)
        {
            int vX = SecondNibble(instruction[0]);
            switch (instruction[1])
            {                
                case 0xA1:
                    if (!_keyboard.IsKeyPressed(registers[vX]))
                    {
                        pc += 2;
                    }
                    break;
                case 0x9E:
                    if (_keyboard.IsKeyPressed(registers[vX]))
                    {
                        pc += 2;
                    }
                    break;
                default:
                    throw new Exception($"Unkown instruction: {Convert.ToHexString(instruction)}");
            }
        }

        private void FInstruction(byte[] instruction)
        {
            int vX = SecondNibble(instruction[0]);
            switch (instruction[1])
            {
                case 0x07:
                    registers[vX] = _delayTimer.GetValue();
                    break;
                case 0x15:
                    _delayTimer.SetValue(registers[vX]);
                    break;
                case 0x33:
                    // convert vX to decimal MNP and then
                    // store M in memory[I], N in memory[I + 1], and P in memory[I + 2]
                    byte val = registers[vX];
                    _memory.SetMemory((short)(_index_register + 2), (byte)(val % 10));
                    val /= 10;
                    _memory.SetMemory((short)(_index_register + 1), (byte)(val % 10));
                    val /= 10;
                    _memory.SetMemory((short)_index_register, (byte)(val % 10));
                    break;
                case 0x55:
                    // store values from V0 - VX to memory[I] - memory[I + X]
                    for (int i = 0; i <= vX; i++)
                    {
                        _memory.SetMemory(_index_register++, registers[i]);
                    }
                    break;
                case 0x65:
                    // load values from memory[I] - memory[I + X] to V0 - VX 
                    for (int i = 0; i <= vX; i++)
                    {
                        registers[i] = _memory.ReadMemory(_index_register++, 1)[0];
                    }
                    break;
                case 0x0A:
                    // wait for keypress
                    while (_keyboard.GetPressedKey() == -1)
                    {
                        Thread.Sleep(1000 / 700);
                    }
                    registers[vX] = (byte)_keyboard.GetPressedKey();
                    // wait for key release
                    while (_keyboard.IsKeyPressed(registers[vX]))
                    {
                        Thread.Sleep(1000 / 700);
                    }
                    break;
                case 0x1E:
                    // add value from vX to I
                    _index_register += registers[vX];
                    break;
                default:
                    throw new Exception($"Unkown instruction: {Convert.ToHexString(instruction)}");
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