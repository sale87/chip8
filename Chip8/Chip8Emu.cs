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
        
        // Embedded IBM Logo ROM data (IBM Logo.ch8)
        private static readonly byte[] DefaultIbmLogoRom = [
            0x00, 0xe0, 0xa2, 0x2a, 0x60, 0x0c, 0x61, 0x08, 0xd0, 0x1f, 0x70, 0x09, 0xa2, 0x39, 0xd0, 0x1f,
            0xa2, 0x48, 0x70, 0x08, 0xd0, 0x1f, 0x70, 0x04, 0xa2, 0x57, 0xd0, 0x1f, 0x70, 0x08, 0xa2, 0x66,
            0xd0, 0x1f, 0x70, 0x08, 0xa2, 0x75, 0xd0, 0x1f, 0x12, 0x28, 0xff, 0x00, 0xff, 0x00, 0x3c, 0x00,
            0x3c, 0x00, 0x3c, 0x00, 0x3c, 0x00, 0xff, 0x00, 0xff, 0xff, 0x00, 0xff, 0x00, 0x38, 0x00, 0x3f,
            0x00, 0x3f, 0x00, 0x38, 0x00, 0xff, 0x00, 0xff, 0x80, 0x00, 0xe0, 0x00, 0xe0, 0x00, 0x80, 0x00,
            0x80, 0x00, 0xe0, 0x00, 0xe0, 0x00, 0x80, 0xf8, 0x00, 0xfc, 0x00, 0x3e, 0x00, 0x3f, 0x00, 0x3b,
            0x00, 0x39, 0x00, 0xf8, 0x00, 0xf8, 0x03, 0x00, 0x07, 0x00, 0x0f, 0x00, 0xbf, 0x00, 0xfb, 0x00,
            0xf3, 0x00, 0xe3, 0x00, 0x43, 0xe0, 0x00, 0xe0, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80,
            0x00, 0xe0, 0x00, 0xe0
        ];

        public void Run(string? path = null)
        {
            _romPath = path ?? string.Empty;
            InitializeComponents();
            Boot();
            MainLoop.Run(_display, _keyboard, _cpu, Boot, LoadNewRom);
        }
        
        private void LoadNewRom(string romPath)
        {
            Console.WriteLine($"LoadNewRom: Setting ROM path to: '{romPath}'");
            _romPath = romPath;
            Boot(); // Reboot with the new ROM
        }

        private void InitializeComponents()
        {
            _memory = new Memory();
            _display = new Display();
            _keyboard = new Keyboard();
            _cpu = new Cpu(_memory, _keyboard, _display);
        }

        private void Boot()
        {
            // Stop CPU first to prevent execution during reset
            _cpu.Stop();
            
            // Small delay to ensure CPU stops completely
            Thread.Sleep(10);
            
            // Reset all components instead of creating new ones
            _memory.Reset();
            _display.InitGrid();
            _cpu.Reset();
            
            // Reload font and ROM
            Font.Load(_memory);
            Console.WriteLine($"Boot: Loading ROM from path: '{_romPath}'");
            LoadRom(_romPath);
            
            // Start CPU after everything is loaded
            _cpu.Start();
        }

        private void LoadRom(string path)
        {
            try
            {
                byte[] bytes;
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("LoadRom: Using embedded IBM Logo ROM");
                    // Use embedded IBM Logo ROM if no path provided
                    bytes = DefaultIbmLogoRom;
                    // Set execution speed to 2 Hz for IBM Logo demo
                    _cpu.SetExecutionSpeed(2);
                }
                else
                {
                    Console.WriteLine($"LoadRom: Loading ROM file: '{path}'");
                    if (!File.Exists(path))
                    {
                        Console.WriteLine($"LoadRom: ERROR - File not found: '{path}'");
                        // Fall back to embedded ROM
                        bytes = DefaultIbmLogoRom;
                        _cpu.SetExecutionSpeed(2);
                    }
                    else
                    {
                        bytes = File.ReadAllBytes(path);
                        // Set execution speed to 700 Hz for regular ROMs
                        _cpu.SetExecutionSpeed(700);
                        Console.WriteLine($"LoadRom: Successfully loaded {bytes.Length} bytes from '{path}'");
                    }
                }
                _memory.SetMemory(0x200, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadRom: Exception loading ROM: {ex.Message}");
                // Fall back to embedded ROM
                var bytes = DefaultIbmLogoRom;
                _cpu.SetExecutionSpeed(2);
                _memory.SetMemory(0x200, bytes);
            }
        }
        
    }
}