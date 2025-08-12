using SDL3;
namespace Chip8.components
{
    public class Display
    {
        public const int WIDTH = 64;
        public const int HEIGHT = 32;
        private const int displayScale = 15;
        private const int windowWidth = WIDTH * displayScale;
        private const int windowHeight = HEIGHT * displayScale;
        private const int singlePixel = displayScale;

        private readonly byte[] BG_COLOR = [0, 0, 0, 255];
        private readonly byte[] FG_COLOR = [150, 150, 150, 255];
        private readonly byte[] GRID_COLOR = [20, 20, 20, 255];

        private bool[,] displayGrid = new bool[Display.WIDTH, Display.HEIGHT];
        private bool hasChanges = false;

        private nint renderer = 0;
        private nint window = 0;

        public void InitGrid()
        {
            hasChanges = true;
            displayGrid = new bool[Display.WIDTH, Display.HEIGHT];
        }

        public bool GetPixel(int x, int y)
        {
            return displayGrid[x, y];
        }

        public void SetPixel(int x, int y, bool v)
        {
            hasChanges = true;
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

        private void DrawPixel(
            nint renderer, int x, int y, bool borderLeft, bool borderRight, bool borderTop, bool borderBottom
            )
        {
            SDL.SetRenderDrawColor(renderer, FG_COLOR[0], FG_COLOR[1], FG_COLOR[2], FG_COLOR[3]);
            SDL.FRect rect = new()
            {
                X = x * displayScale,
                Y = y * displayScale,
                W = singlePixel,
                H = singlePixel
            };
            SDL.RenderFillRect(renderer, rect);

            SDL.SetRenderDrawColor(renderer, GRID_COLOR[0], GRID_COLOR[1], GRID_COLOR[2], GRID_COLOR[3]);
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

        private void DrawGrid(nint renderer)
        {
            SDL.SetRenderDrawColor(renderer, GRID_COLOR[0], GRID_COLOR[1], GRID_COLOR[2], GRID_COLOR[3]);
            for (int x = 0; x < WIDTH; x++)
            {
                SDL.RenderLine(renderer, x * displayScale, 0, x * displayScale, windowHeight);
            }
            for (int x = 0; x < HEIGHT; x++)
            {
                SDL.RenderLine(renderer, 0, x * displayScale, windowWidth, x * displayScale);
            }
        }

        private void DrawBackground(nint renderer)
        {
            SDL.SetRenderDrawColor(renderer, BG_COLOR[0], BG_COLOR[1], BG_COLOR[2], BG_COLOR[3]);
            SDL.RenderClear(renderer);
        }

    }
}