using System.Numerics;
using Raylib_cs;

namespace Chip8.components;

public class Display
{
    // CHIP-8 display constants
    public const int Width = 64;
    public const int Height = 32;
    private const int DisplayScale = 10;

    public Vector3 PixelColor = new(0.58f, 0.58f, 0.58f);
    public Vector3 BackgroundColor = new(0, 0, 0);
    public Vector3 GridColor = new(0.19f, 0.19f, 0.19f);

    public bool[,] DisplayGrid = new bool[Width, Height];

    private bool _hasChanges = true;

    public void InitGrid()
    {
        _hasChanges = true;
        DisplayGrid = new bool[Width, Height];
    }

    public bool GetPixel(int x, int y)
    {
        return DisplayGrid[x, y];
    }

    public void SetPixel(int x, int y, bool v)
    {
        _hasChanges = true;
        DisplayGrid[x, y] = v;
    }

    public void Draw()
    {
        if (!_hasChanges) return;

        // Calculate display position (left side of screen)
        const int displayPixelWidth = Width * DisplayScale;
        const int displayPixelHeight = Height * DisplayScale;
        var displayX = (Raylib.GetScreenWidth() - displayPixelWidth) / 2;
        var displayY = (Raylib.GetScreenHeight() - displayPixelHeight) / 2;

        // Draw border around display
        Raylib.DrawRectangleLines(
            displayX - 2,
            displayY - 2,
            displayPixelWidth + 4,
            displayPixelHeight + 4,
            Color.White
        );

        Raylib.DrawRectangle(
            displayX,
            displayY,
            Width * DisplayScale,
            Height * DisplayScale,
            new Color(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z)
        );

        // Draw each pixel
        var fgColor = new Color(PixelColor.X, PixelColor.Y, PixelColor.Z);
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (DisplayGrid[x, y])
                {
                    Raylib.DrawRectangle(
                        displayX + x * DisplayScale,
                        displayY + y * DisplayScale,
                        DisplayScale,
                        DisplayScale,
                        fgColor
                    );
                }
            }
        }

        var gColor = new Color(GridColor.X, GridColor.Y, GridColor.Z);
        for (var x = 0; x < Width; x++)
        {
            Raylib.DrawLine(
                displayX + x * DisplayScale,
                displayY,
                displayX + x * DisplayScale,
                displayY + Height * DisplayScale,
                gColor
            );
        }

        for (var y = 0; y < Height; y++)
        {
            Raylib.DrawLine(
                displayX,
                displayY + y * DisplayScale,
                displayX + Width * DisplayScale,
                displayY + y * DisplayScale,
                gColor
            );
        }
    }
}