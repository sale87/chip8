using SDL3;
namespace Chip8.components
{
    public class Display
    {
        public const int WIDTH = 64;
        public const int HEIGHT = 32;
        private const int displayScale = 20;
        private const int windowWidth = WIDTH * displayScale;
        private const int windowHeight = HEIGHT * displayScale;
        private const int singlePixel = displayScale;

        private readonly bool[,] displayGrid;

        private nint renderer = 0;
        private nint window = 0;

        public Display()
        {
            this.displayGrid = InitGrid();
        }

        public bool[,] InitGrid()
        {
            return new bool[Display.WIDTH, Display.HEIGHT];
        }

        public bool GetPixel(int x, int y)
        {
            return displayGrid[x, y];
        }


        public void SetPixel(int x, int y, bool v)
        {
            displayGrid[x, y] = v;
        }

        public void Draw()
        {
            if (window == 0 || renderer == 0)
                CreateWindow();

            DrawBackground(renderer);
            DrawGrid(renderer);
            DrawPixels(renderer);

            SDL.RenderPresent(renderer);
        }

        public void Close()
        {
            SDL.DestroyRenderer(renderer);
            SDL.DestroyWindow(window);
            SDL.Quit();
        }

        private void CreateWindow()
        {
            if (!SDL.Init(SDL.InitFlags.Video))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
                throw new Exception("Failed to initialize SDL.");
            }

            if (!SDL.CreateWindowAndRenderer(
                "SDL3 Create Window",
                windowWidth,
                windowHeight,
                0,
                out window,
                out renderer))
            {
                SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
                throw new Exception("Failed to create new Window.");
            }
        }

        private void DrawPixels(nint renderer)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    if (displayGrid[x, y])
                    {
                        DrawPixel(
                            renderer, x, y,
                            (x - 1 > 0) && displayGrid[x - 1, y],
                            (x + 1 < WIDTH) && displayGrid[x + 1, y],
                            (y - 1 > 0) && displayGrid[x, y - 1],
                            (y + 1 < HEIGHT) && displayGrid[x, y + 1]
                        );
                    }
                }
            }
        }

        private static void DrawPixel(
            nint renderer, int x, int y, bool borderLeft, bool borderRight, bool borderTop, bool borderBottom
            )
        {
            SDL.SetRenderDrawColor(renderer, 0x34, 0x68, 0x56, 255);
            SDL.FRect rect = new()
            {
                X = x * displayScale,
                Y = y * displayScale,
                W = singlePixel,
                H = singlePixel
            };
            SDL.RenderFillRect(renderer, rect);

            SDL.SetRenderDrawColor(renderer, 0x88, 0xc0, 0x70, 255);
            if (borderLeft)
            {
                SDL.RenderLine(renderer, rect.X, rect.Y, rect.X, rect.Y + singlePixel);
            }
            if (borderRight)
            {
                SDL.RenderLine(renderer, rect.X + singlePixel, rect.Y, rect.X + singlePixel, rect.Y + singlePixel);
            }
            if (borderTop)
            {
                SDL.RenderLine(renderer, rect.X, rect.Y, rect.X + singlePixel, rect.Y);
            }
            if (borderBottom)
            {
                SDL.RenderLine(renderer, rect.X, rect.Y + singlePixel, rect.X + singlePixel, rect.Y + singlePixel);
            }

        }

        private static void DrawGrid(nint renderer)
        {
            SDL.SetRenderDrawColor(renderer, 0x88, 0xc0, 0x70, 255);
            for (int x = 0; x < WIDTH; x++)
            {
                SDL.RenderLine(renderer, x * displayScale, 0, x * displayScale, windowHeight);
            }
            for (int x = 0; x < HEIGHT; x++)
            {
                SDL.RenderLine(renderer, 0, x * displayScale, windowWidth, x * displayScale);
            }
        }

        private static void DrawBackground(nint renderer)
        {
            SDL.SetRenderDrawColor(renderer, 0xe0, 0xf8, 0xd0, (byte)SDL.AlphaOpaque);
            SDL.RenderClear(renderer);
        }

    }
}