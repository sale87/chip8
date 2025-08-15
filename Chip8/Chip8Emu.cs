using Chip8.components;
using Chip8.util;

namespace Chip8
{
    public class Chip8Emu
    {
        private readonly Memory _memory = new();

        private readonly Display _display = new();

        private readonly Keyboard _keyboard = new();
        private readonly Cpu _cpu;
        
        public Chip8Emu()
        {
            _cpu = new Cpu(_memory, _keyboard, _display);
            Font.Load(_memory);
        }

        public void Run(string path)
        {
            LoadRom(path);
            _cpu.Start();
            MainLoop.Run(_display, _keyboard);
            _display.Close();
        }

        private void LoadRom(string path)
        {
            var bytes = File.ReadAllBytes(path);
            _memory.SetMemory(0x200, bytes);
        }

        
    }
}