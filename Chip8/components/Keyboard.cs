namespace Chip8.components
{
    class Keyboard
    {
        private readonly byte[] keys = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        public void KeyPressed(byte key)
        {
            keys[key] = 1;
        }

        public void KeyReleased(byte key)
        {
            keys[key] = 0;
        }

        public bool IsKeyPressed(byte key)
        {
            return keys[key] == 1;
        }

        public bool IsAnyKeyPressed()
        {
            return keys.Any((x) => x == 1);
        }

        public short GetPressedKey()
        {
            for (short i = 0; i < 16; i++)
            {
                if (keys[i] == 1)
                {
                    return i;
                }
            }
            return -1;
        }

    }
}