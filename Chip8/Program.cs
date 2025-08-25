namespace Chip8;

internal static class Program
{
    private static void Main()
    {
        Chip8Emu c8 = new();
        // Run with embedded IBM Logo ROM by default (no ROM path provided)
        c8.Run();
    }
}