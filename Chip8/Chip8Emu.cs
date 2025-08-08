

public class Chip8Emu
{
    const short FONT_START_ADDR = 0x50;

    readonly byte[] font = [
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    ];

    // 4kb of memory
    public byte[] Memory { get; } = new byte[4096];

    // display size is 64x32, so we are storing 32 64bit numbers
    private readonly ulong[] display = new ulong[32];

    // program counter, points to current instruction in memory, possible values 0 - 4096
    private short pc = 0;

    // points to locations in memory
    private ushort iRegister = 0;

    // holds call stack information
    private Stack<short> s = new Stack<short>();

    private byte delayTimer = 255;

    private byte soundTimer = 255;

    // general purpose variable registers, V0 - VF
    private readonly byte[] registers = new byte[16];

    public Chip8Emu()
    {
        loadFont();
    }

    private void loadFont()
    {
        short addr = FONT_START_ADDR;
        foreach (var b in font)
        {
            SetMemory(addr, b);
            addr++;
        }
    }

    public void SetMemory(short addr, byte b)
    {
        if (addr < 0 || addr > 4096)
        {
            throw new ArgumentOutOfRangeException($"Cannot set value to address {addr}.");
        }
        this.Memory[addr] = b;
    }

    public byte[] ReadMemory(short start, short length)
    {
        if (start < 0 || start > 4096)
        {
            throw new ArgumentOutOfRangeException($"Invalid start read value {start}. It must be between 0 and 4096.");
        }
        if (length < 0 || length > 4096)
        {
            throw new ArgumentOutOfRangeException($"Invalid length read value {length}. It must be between 0 and 4096.");
        }
        if (start + length > 4096)
        {
            throw new ArgumentOutOfRangeException($"Invalid parameters: s:{start} l:{length}. Max read adress is 4096.");
        }
        byte[] res = new byte[length];
        for (int i = 0; i < length; i++)
        {
            res[i] = Memory[start + i];
        }
        return res;
    }
}