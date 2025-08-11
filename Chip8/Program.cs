internal class Program
{
    private static void Main(string[] args)
    {
        Chip8Emu c8 = new();
        c8.LoadRom("../../../../roms/IBM Logo.ch8");
        // c8.LoadRom("../../../../roms/1-chip8-logo.ch8");
        // c8.LoadRom("../../../../roms/2-ibm-logo.ch8");
        // c8.LoadRom("../../../../roms/3-corax+.ch8");
        c8.Start();
    }
}