using SDL3;
namespace Chip8.components;

public class Display
{
    public const int Width = 64;
    public const int Height = 32;
    private const int DisplayScale = 15;
    private const int WindowWidth = Width * DisplayScale;
    private const int WindowHeight = Height * DisplayScale;
    private const int SinglePixel = DisplayScale;

    private readonly byte[] _bgColor = [0, 0, 0, 255];
    private readonly byte[] _fgColor = [150, 150, 150, 255];
    private readonly byte[] _gridColor = [20, 20, 20, 255];

    private bool[,] _displayGrid = new bool[Width, Height];

    private nint _renderer;
    private nint _window;

    private bool _hasChanges = true;

    public void InitGrid()
    {
        _hasChanges = true;
        _displayGrid = new bool[Display.Width, Height];
    }

    public bool GetPixel(int x, int y)
    {
        return _displayGrid[x, y];
    }

    public void SetPixel(int x, int y, bool v)
    {
        _hasChanges = true;
        _displayGrid[x, y] = v;
    }

    public void Draw()
    {
        if (!_hasChanges)
        {
            return;
        }
        if (_window == 0 || _renderer == 0)
            CreateWindow();

        DrawBackground(_renderer);
        DrawGrid(_renderer);
        DrawPixels(_renderer);

        SDL.RenderPresent(_renderer);
    }

    public void Close()
    {
        SDL.DestroyRenderer(_renderer);
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }

    private void CreateWindow()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            throw new Exception("Failed to initialize SDL.");
        }

        if (SDL.CreateWindowAndRenderer(
                "Chip8",
                WindowWidth,
                WindowHeight,
                0,
                out _window,
                out _renderer)) return;
        SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
        throw new Exception("Failed to create new Window.");
    }

    private void DrawPixels(nint renderer)
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                if (_displayGrid[x, y])
                {
                    DrawPixel(
                        renderer, x, y,
                        x - 1 > 0 && _displayGrid[x - 1, y],
                        x + 1 < Width && _displayGrid[x + 1, y],
                        y - 1 > 0 && _displayGrid[x, y - 1],
                        y + 1 < Height && _displayGrid[x, y + 1]
                    );
                }
            }
        }
    }

    private void DrawPixel(
        nint renderer, int x, int y, bool borderLeft, bool borderRight, bool borderTop, bool borderBottom
    )
    {
        SDL.SetRenderDrawColor(renderer, _fgColor[0], _fgColor[1], _fgColor[2], _fgColor[3]);
        SDL.FRect rect = new()
        {
            X = x * DisplayScale,
            Y = y * DisplayScale,
            W = SinglePixel,
            H = SinglePixel
        };
        SDL.RenderFillRect(renderer, rect);

        SDL.SetRenderDrawColor(renderer, _gridColor[0], _gridColor[1], _gridColor[2], _gridColor[3]);
        if (borderLeft)
        {
            SDL.RenderLine(renderer, rect.X, rect.Y, rect.X, rect.Y + SinglePixel);
        }
        if (borderRight)
        {
            SDL.RenderLine(renderer, rect.X + SinglePixel, rect.Y, rect.X + SinglePixel, rect.Y + SinglePixel);
        }
        if (borderTop)
        {
            SDL.RenderLine(renderer, rect.X, rect.Y, rect.X + SinglePixel, rect.Y);
        }
        if (borderBottom)
        {
            SDL.RenderLine(renderer, rect.X, rect.Y + SinglePixel, rect.X + SinglePixel, rect.Y + SinglePixel);
        }

    }

    private void DrawGrid(nint renderer)
    {
        SDL.SetRenderDrawColor(renderer, _gridColor[0], _gridColor[1], _gridColor[2], _gridColor[3]);
        for (var x = 0; x < Width; x++)
        {
            SDL.RenderLine(renderer, x * DisplayScale, 0, x * DisplayScale, WindowHeight);
        }
        for (var x = 0; x < Height; x++)
        {
            SDL.RenderLine(renderer, 0, x * DisplayScale, WindowWidth, x * DisplayScale);
        }
    }

    private void DrawBackground(nint renderer)
    {
        SDL.SetRenderDrawColor(renderer, _bgColor[0], _bgColor[1], _bgColor[2], _bgColor[3]);
        SDL.RenderClear(renderer);
    }

}