namespace Chip8.components
{
    public class Memory
    {
        // 4kb of memory
        private readonly byte[] _memory = new byte[4096];

        public void SetMemory(short addr, byte b)
        {
            if (addr is < 0 or > 4096)
            {
                throw new ArgumentOutOfRangeException($"Cannot set value to address {addr}.");
            }
            _memory[addr] = b;
        }

        public void SetMemory(short addr, byte[] bytes)
        {
            foreach (var b in bytes)
            {
                _memory[addr] = b;
                addr++;
            }
        }

        public byte[] ReadMemory(short start, short length)
        {
            if (start is < 0 or > 4096)
            {
                throw new ArgumentOutOfRangeException($"Invalid start read value {start}. It must be between 0 and 4096.");
            }
            if (length is < 0 or > 4096)
            {
                throw new ArgumentOutOfRangeException($"Invalid length read value {length}. It must be between 0 and 4096.");
            }
            if (start + length > 4096)
            {
                throw new ArgumentOutOfRangeException($"Invalid parameters: Stck:{start} l:{length}. Max read adress is 4096.");
            }
            var res = new byte[length];
            for (var i = 0; i < length; i++)
            {
                res[i] = _memory[start + i];
            }
            return res;
        }

        public byte[] ReadAll()
        {
            return _memory;
        }
    }
}