using System.Timers;

internal class Program
{
    private static void Main(string[] args)
    {
        Chip8Emu c8 = new();
        c8.LoadRom("../../../../roms/IBM Logo.ch8");
        for (int i = 0; i < 132; i++)
        {
            c8.RunCycle();    
        }
    }
}