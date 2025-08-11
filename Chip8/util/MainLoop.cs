using Chip8.components;
using SDL3;

namespace Chip8.util
{
    class MainLoop
    {
        // SDL things
        private static bool loop = true;
        private static readonly uint FPS = 60;
        private static readonly uint FRAME_TARGET_TIME = 1000 / FPS;
        private static ulong lastFameTime = 0;

        public static void Run(Display _display)
        {
            // main SDL loop
            while (loop)
            {
                Wait();
                ProcessInput();
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
    }
}