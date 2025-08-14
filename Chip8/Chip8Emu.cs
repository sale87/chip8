using Chip8.components;
using Chip8.util;

namespace Chip8
{
    public class Chip8Emu
    {
        private const bool DebugRaw = false;
        private const bool DebugInstructions = false;

        private readonly Memory _memory = new();

        private readonly Display _display = new();

        private readonly Keyboard _keyboard = new();

        private readonly DelayTimer _delayTimer = new();

        private readonly Clock _clock;

        // program counter, points to current instruction in memory, possible values 0 - 4096
        private short _pc;

        // points to locations in memory
        private short _indexRegister;

        // general purpose variable registers, V0 - VF
        private readonly byte[] _registers = new byte[16];

        // holds call stack address information
        private readonly Stack<short> _stack = new();

        public Chip8Emu()
        {
            Font.Load(_memory);
            _clock = new Clock(RunCycle, false);
        }

        public void Run(string path)
        {
            LoadRom(path);
            _delayTimer.StartTimer();
            _clock.Running = true;
            MainLoop.Run(_display, _keyboard);
            _display.Close();
        }

        private void LoadRom(string path)
        {
            var bytes = File.ReadAllBytes(path);
            _pc = 0x200;
            _memory.SetMemory(0x200, bytes);
        }

        private void RunCycle()
        {
            byte[] instruction = FetchInstruction();
            ExecuteInstruction(instruction);
        }

        private byte[] FetchInstruction()
        {
            var bytes = _memory.ReadMemory(_pc, 2);
            _pc += 2;
            return bytes;
        }

        private void ExecuteInstruction(byte[] instruction)
        {
            PrintDebugRaw(instruction);

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
                    SkipIfVXeqVy(instruction);
                    break;
                case 0x9:
                    SkipIfVXneqVy(instruction);
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
                    throw new Exception($"Unknown instruction: {Convert.ToHexString(instruction)}");
            }
        }

        private static void PrintDebugRaw(byte[] instruction)
        {
            if (!DebugRaw) return;
            // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected
            Console.Out.WriteLine($"----\n{HexStr(instruction)}");
#pragma warning restore CS0162 // Unreachable code detected
        }

        private void SystemInstruction(byte[] instruction)
        {
            switch (instruction[1])
            {
                case 0xE0:
                    ClearScreen();
                    break;
                case 0xEE:
                    Return();
                    break;
                default:
                    throw new Exception($"Unknown instruction: {Convert.ToHexString(instruction)}");
            }
        }

        private void ClearScreen()
        {
            // 00E0 - clear screen
            _display.InitGrid();
            Debug("cls");
        }

        private void Return()
        {
            // 00EE - return from subroutine
            _pc = _stack.Pop();
            Debug($"return 0x{_pc:X4}");
        }

        private void Jump(byte[] instruction)
        {
            // 1NNN - set PC to NNN
            _pc = NNN(instruction);
            Debug($"jump 0x{_pc:X4}");
        }

        private void Call(byte[] instruction)
        {
            // 2NNN - Call routine at NNN
            _stack.Push(_pc);
            _pc = NNN(instruction);
            Debug($"call 0x{_pc:X4}");
        }

        private void SkipIfVXeqN(byte[] instruction)
        {
            // 3XNN - if vX == NN, skip next instruction
            var vX = SecondNibble(instruction[0]);
            if (_registers[vX] == instruction[1])
            {
                _pc += 2;
            }
            Debug($"skip_if v{vX:X} == 0x{instruction[1]:X2}");
        }

        private void SkipIfVXneqN(byte[] instruction)
        {
            // 4XNN - if vX != NN, skip next instruction
            var vX = SecondNibble(instruction[0]);
            if (_registers[vX] != instruction[1])
            {
                _pc += 2;
            }
            Debug($"skip_if v{vX:X} != 0x{instruction[1]:X2}");
        }

        private void SkipIfVXeqVy(byte[] instruction)
        {
            // 5XY0 - if VX == VY, skip next instruction
            var vX = SecondNibble(instruction[0]);
            var vY = FirstNibble(instruction[1]);
            if (_registers[vX] == _registers[vY])
            {
                _pc += 2;
            }
            Debug($"skip_if v{vX:X} == v{vY:X}");
        }

        private void SkipIfVXneqVy(byte[] instruction)
        {
            // 9XY0 - if VX != VY, skip next instruction
            var vX = SecondNibble(instruction[0]);
            var vY = FirstNibble(instruction[1]);
            if (_registers[vX] != _registers[vY])
            {
                _pc += 2;
            }
            Debug($"skip_if v{vX:X} != v{vY:X}");
        }

        private void SetRegister(byte[] instruction)
        {
            // 6XNN - set the register VX to the value NN
            var vX = SecondNibble(instruction[0]);
            _registers[vX] = instruction[1];
            Debug($"set v{vX:X} 0x{instruction[1]:X2}");
        }

        private void Add(byte[] instruction)
        {
            // 7XNN - add the value NN to VX
            var vX = SecondNibble(instruction[0]);
            _registers[vX] += instruction[1];
            Debug($"add v{vX:X} 0x{instruction[1]:X2}");
        }

        private void LogicOrArithmeticInstruction(byte[] instruction)
        {
            // 8XYI - Logic or arithmetic instruction involving vX and vY registers
            var vX = SecondNibble(instruction[0]);
            var vY = FirstNibble(instruction[1]);
            var i = SecondNibble(instruction[1]);

            switch (i)
            {
                case 0x0:
                    // 8XY0: Set vX to vY
                    _registers[vX] = _registers[vY];
                    Debug($"v{vX:X} = v{vY:X}");
                    break;
                case 0x1:
                    // 8XY1: Binary OR
                    _registers[vX] |= _registers[vY];
                    _registers[0xF] = 0;
                    Debug($"v{vX:X} |= v{vY:X}");
                    break;
                case 0x2:
                    // 8XY1: Binary AND
                    _registers[vX] &= _registers[vY];
                    _registers[0xF] = 0;
                    Debug($"v{vX:X} &= v{vY:X}");
                    break;
                case 0x3:
                    // 8XY3: Logical XOR
                    _registers[vX] ^= _registers[vY];
                    _registers[0xF] = 0;
                    Debug($"v{vX:X} ^= v{vY:X}");
                    break;
                case 0x4:
                    // 8XY4: Add vY to vX and set to vX
                    var pre8Xy4 = _registers[vX];
                    _registers[vX] += _registers[vY];
                    // in case of overflow set vF to 1
                    _registers[0xF] = (byte)(pre8Xy4 > _registers[vX] ? 1 : 0);
                    Debug($"v{vX:X} += v{vY:X}");
                    break;
                case 0x5:
                    // 8XY5: Subtract vY from vX and set to vX
                    var pre8Xy5 = _registers[vX];
                    _registers[vX] -= _registers[vY];
                    // in case of underflow set vF to 1
                    _registers[0xF] = (byte)((pre8Xy5 < _registers[vX]) ? 0 : 1);
                    Debug($"v{vX:X} = v{vX:X} - v{vY:X}");
                    break;
                case 0x6:
                    // 8XY6: Shift vX to the right
                    var pre8Xy6 = _registers[vX];
                    _registers[vX] >>= 1;
                    _registers[0xF] = (byte)(pre8Xy6 & 0b1);
                    Debug($"v{vX:X} >>= 1");
                    break;
                case 0x7:
                    // 8XY7: Subtract vX from vY and set to vX
                    var pre8Xy7 = _registers[vX];
                    _registers[vX] = (byte)(_registers[vY] - _registers[vX]);
                    _registers[0xF] = (byte)((pre8Xy7 > _registers[vY]) ? 0 : 1);
                    Debug($"v{vX:X} = v{vY:X} - v{vX:X}");
                    break;
                case 0xE:
                    // 8XYE: Shift vX to the left
                    var pre8Xye = _registers[vX];
                    _registers[vX] <<= 1;
                    _registers[0xF] = (byte)((pre8Xye & 0b10000000) >> 7);
                    Debug($"v{vX:X} <<= 1");
                    break;
            }
        }

        private void SetIndexRegister(byte[] instruction)
        {
            // ANNN - set the index register I to the value NNN
            _indexRegister = NNN(instruction);
            Debug($"set_i 0x{_indexRegister:X4}");
        }

        private void JumpWithOffset(byte[] instruction)
        {
            // 2NNN - set PC to v0 + NNN
            _pc = (short)(_registers[0] + NNN(instruction));
            Debug($"jump 0x{_pc:X4}");
        }

        private void Draw(byte[] instruction)
        {
            // DXYN - draw an N pixels tall sprite 
            // from the memory location that I index register is holding to the screen, 
            // at the horizontal X coordinate in VX and the Y coordinate in VY
            var vX = SecondNibble(instruction[0]);
            var vY = FirstNibble(instruction[1]);
            var n = SecondNibble(instruction[1]);
            var x = _registers[vX] % Display.Width;
            var y = _registers[vY] % Display.Height;

            var sprite = _memory.ReadMemory(_indexRegister, (short)n);
            _registers[0xf] = 0;

            for (var yi = 0; yi < n; yi++)
            {
                var effectiveY = y + yi;
                if (effectiveY >= Display.Height)
                {
                    break;
                }
                for (var xi = 0; xi < 8; xi++)
                {
                    var mask = (byte)(0b10000000 >> xi);
                    var effectiveX = x + xi;
                    if (effectiveX >= Display.Width)
                    {
                        break;
                    }
                    var pixelOn = (sprite[yi] & mask) != 0;
                    var previousValue = _display.GetPixel(effectiveX, effectiveY);
                    if (previousValue && pixelOn)
                    {
                        _registers[0xf] = 1;
                        _display.SetPixel(effectiveX, effectiveY, false);
                    }
                    else if (pixelOn)
                    {
                        _display.SetPixel(effectiveX, effectiveY, pixelOn);
                    }
                }
            }
            
            // Simulate 60 FPS drawing 
            Thread.Sleep(1000/60);

            Debug($"draw v{vX:X}, v{vY:X}, {n}");
        }

        private void EInstruction(byte[] instruction)
        {
            var vX = SecondNibble(instruction[0]);
            switch (instruction[1])
            {
                case 0xA1:
                    if (!_keyboard.IsKeyPressed(_registers[vX]))
                    {
                        _pc += 2;
                    }
                    Debug($"is_not_key_pressed v{vX:X}");
                    break;
                case 0x9E:
                    if (_keyboard.IsKeyPressed(_registers[vX]))
                    {
                        _pc += 2;
                    }
                    Debug($"is_key_pressed v{vX:X}");
                    break;
                default:
                    throw new Exception($"Unknown instruction: {Convert.ToHexString(instruction)}");
            }
        }

        private void FInstruction(byte[] instruction)
        {
            var vX = SecondNibble(instruction[0]);
            switch (instruction[1])
            {
                case 0x07:
                    _registers[vX] = _delayTimer.GetValue();
                    Debug($"set v{vX:X} delay_timer");
                    break;
                case 0x15:
                    _delayTimer.SetValue(_registers[vX]);
                    Debug($"set delay_timer v{vX:X}");
                    break;
                case 0x33:
                    // convert vX to decimal MNP and then
                    // store M in memory[I], N in memory[I + 1], and P in memory[I + 2]
                    var val = _registers[vX];
                    _memory.SetMemory((short)(_indexRegister + 2), (byte)(val % 10));
                    val /= 10;
                    _memory.SetMemory((short)(_indexRegister + 1), (byte)(val % 10));
                    val /= 10;
                    _memory.SetMemory(_indexRegister, (byte)(val % 10));
                    Debug($"convert_to_decimal v{vX:X}");
                    break;
                case 0x55:
                    // store values from V0 - VX to memory[I] - memory[I + X]
                    for (var i = 0; i <= vX; i++)
                    {
                        _memory.SetMemory(_indexRegister++, _registers[i]);
                    }
                    Debug($"store_to_memory v0 v{vX:X}");
                    break;
                case 0x65:
                    // load values from memory[I] - memory[I + X] to V0 - VX 
                    for (var i = 0; i <= vX; i++)
                    {
                        _registers[i] = _memory.ReadMemory(_indexRegister++, 1)[0];
                    }
                    Debug($"load_from_memory v0 v{vX:X}");
                    break;
                case 0x0A:
                    // wait for keypress
                    _clock.Running = false;
                    while (_keyboard.GetPressedKey() == -1)
                    {
                        Thread.Sleep(2);
                    }
                    var pressedKey = (byte)_keyboard.GetPressedKey();
                    // wait for key release
                    while (_keyboard.IsKeyPressed(pressedKey))
                    {
                        Thread.Sleep(2);
                    }
                    _registers[vX] = pressedKey;
                    _clock.Running = true;
                    Debug($"key_press v{vX:X}");
                    break;
                case 0x1E:
                    // add value from vX to I
                    _indexRegister += _registers[vX];
                    Debug($"add index_register v{vX:X}");
                    break;
                default:
                    throw new Exception($"Unknown instruction: {Convert.ToHexString(instruction)}");
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

        // ReSharper disable once InconsistentNaming
        private static short NNN(byte[] instruction)
        {
            return (short)((SecondNibble(instruction[0]) << 8) + instruction[1]);
        }

        private static string HexStr(byte[] arr)
        {
            return Convert.ToHexString(arr);
        }

        private static void Debug(string message)
        {
            if (!DebugInstructions)
                return;

            // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected
            Console.Out.WriteLine($"{DateTime.Now:HH:mm:ss.ffffff} {message}");
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}