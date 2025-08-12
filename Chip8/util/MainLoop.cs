using Chip8.components;
using SDL3;

namespace Chip8.util
{
    class MainLoop
    {
        // SDL things
        private static bool loop = true;
        private static readonly uint FPS = 700;
        private static readonly uint FRAME_TARGET_TIME = 1000 / FPS;
        private static ulong lastFameTime = 0;

        public static void Run(Display _display, Keyboard _keyboard)
        {
            // main SDL loop
            while (loop)
            {
                Wait();
                ProcessInput(_keyboard);
                _display.Draw();
            }
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

        private static void ProcessInput(Keyboard keyboard)
        {
            while (SDL.PollEvent(out var e))
            {
                SDL.EventType type = (SDL.EventType)e.Type;
                if (type == SDL.EventType.Quit)
                {
                    loop = false;
                }

                if (type == SDL.EventType.KeyDown || type == SDL.EventType.KeyUp)
                {
                    SDL.Keycode key = e.Key.Key;
                    switch (key)
                    {
                        case SDL.Keycode.Escape:
                            loop = false;
                            break;
                        case SDL.Keycode.Alpha1:
                            ProcessKey(keyboard, type, 0x1);
                            break;
                        case SDL.Keycode.Alpha2:
                            ProcessKey(keyboard, type, 0x2);
                            break;
                        case SDL.Keycode.Alpha3:
                            ProcessKey(keyboard, type, 0x3);
                            break;
                        case SDL.Keycode.Alpha4:
                            ProcessKey(keyboard, type, 0xC);
                            break;
                        case SDL.Keycode.Q:
                            ProcessKey(keyboard, type, 0x4);
                            break;
                        case SDL.Keycode.W:
                            ProcessKey(keyboard, type, 0x5);
                            break;
                        case SDL.Keycode.E:
                            ProcessKey(keyboard, type, 0x6);
                            break;
                        case SDL.Keycode.R:
                            ProcessKey(keyboard, type, 0xD);
                            break;
                        case SDL.Keycode.A:
                            ProcessKey(keyboard, type, 0x7);
                            break;
                        case SDL.Keycode.S:
                            ProcessKey(keyboard, type, 0x8);
                            break;
                        case SDL.Keycode.D:
                            ProcessKey(keyboard, type, 0x9);
                            break;
                        case SDL.Keycode.F:
                            ProcessKey(keyboard, type, 0xE);
                            break;
                        case SDL.Keycode.Z:
                            ProcessKey(keyboard, type, 0xA);
                            break;
                        case SDL.Keycode.X:
                            ProcessKey(keyboard, type, 0x0);
                            break;
                        case SDL.Keycode.C:
                            ProcessKey(keyboard, type, 0xB);
                            break;
                        case SDL.Keycode.V:
                            ProcessKey(keyboard, type, 0xF);
                            break;
                    }
                }
            }
        }

        private static void ProcessKey(Keyboard keyboard, SDL.EventType type, byte k)
        {
            if (type == SDL.EventType.KeyDown)
            {
                keyboard.KeyPressed(k);
            }
            else
            {
                keyboard.KeyReleased(k);
            }
        }
    }
}