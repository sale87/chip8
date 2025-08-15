using Chip8.components;
using Chip8.util;

namespace Chip8
{
    public class Chip8Emu
    {
        private Cpu _cpu = null!;
        private Memory _memory = null!;
        private Display _display = null!;
        private Keyboard _keyboard = null!;

        public void Run(string path)
        {
            _memory = new Memory();
            _display = new Display();
            _keyboard = new Keyboard();
            _cpu = new Cpu(_memory, _keyboard, _display);
            Font.Load(_memory);
            LoadRom(path);
            _cpu.Start();
            MainLoop.Run(_display, _keyboard);
        }

        private void LoadRom(string path)
        {
            var bytes = File.ReadAllBytes(path);
            _memory.SetMemory(0x200, bytes);
        }
        
    }
}