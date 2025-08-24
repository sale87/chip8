using Chip8.components;
using Chip8.ui;
using Raylib_cs;
using rlImGui_cs;

namespace Chip8.util
{
    internal static class MainLoop
    {
        // Initialize window
        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 900;

        private static DebugInterface _debugInterface = null!;

        private static readonly Dictionary<byte, KeyboardKey> Keys = new Dictionary<byte, KeyboardKey>
        {
            { 0x1, KeyboardKey.One },
            { 0x2, KeyboardKey.Two },
            { 0x3, KeyboardKey.Three },
            { 0xC, KeyboardKey.Four },
            { 0x4, KeyboardKey.Q },
            { 0x5, KeyboardKey.W },
            { 0x6, KeyboardKey.E },
            { 0xD, KeyboardKey.R },
            { 0x7, KeyboardKey.A },
            { 0x8, KeyboardKey.S },
            { 0x9, KeyboardKey.D },
            { 0xE, KeyboardKey.F },
            { 0xA, KeyboardKey.Z },
            { 0x0, KeyboardKey.X },
            { 0xB, KeyboardKey.C },
            { 0xF, KeyboardKey.V },
        };

        public static void Run(Display display, Keyboard keyboard, Cpu cpu, Action reboot)
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "CHIP-8 Emulator - Raylib + ImGui");
            Raylib.SetTargetFPS(60);

            // Initialize ImGui
            rlImGui.Setup();
            Console.WriteLine("ImGui initialized successfully");
            _debugInterface = new DebugInterface();

            while (!Raylib.WindowShouldClose())
            {
                // Update

                // Draw
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DarkGray);

                // Draw CHIP-8 display
                display.Draw();

                // Begin ImGui frame
                rlImGui.Begin();

                // Draw ImGui interface
                _debugInterface.Render(display, cpu, reboot);

                // End ImGui frame
                rlImGui.End();

                ProcessInput(keyboard);

                Raylib.EndDrawing();
            }

            // Cleanup
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }

        private static void ProcessInput(Keyboard keyboard)
        {
            foreach (var (chipKeyboardKey, key) in Keys)
            {
                if (Raylib.IsKeyDown(key))
                {
                    keyboard.KeyPressed(chipKeyboardKey);
                }

                if (Raylib.IsKeyUp(key))
                {
                    keyboard.KeyReleased(chipKeyboardKey);
                }
            }
        }
    }
}