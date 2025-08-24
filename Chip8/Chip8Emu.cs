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
        private string _romPath = null!;

        public void Run(string path)
        {
            _romPath = path;
            Boot();
            MainLoop.Run(_display, _keyboard, _cpu, Boot);
        }

        private void Boot()
        {
            _memory = new Memory();
            _display = new Display();
            _keyboard = new Keyboard();
            _cpu = new Cpu(_memory, _keyboard, _display);
            Font.Load(_memory);
            LoadRom(_romPath);
            _cpu.Start();
        }

        private void LoadRom(string path)
        {
            var bytes = File.ReadAllBytes(path);
            _memory.SetMemory(0x200, bytes);
        }
        
    }
}