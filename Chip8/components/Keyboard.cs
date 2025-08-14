namespace Chip8.components
{
    internal class Keyboard
    {
        private readonly byte[] _keys = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        public void KeyPressed(byte key)
        {
            _keys[key] = 1;
        }

        public void KeyReleased(byte key)
        {
            _keys[key] = 0;
        }

        public bool IsKeyPressed(byte key)
        {
            return _keys[key] == 1;
        }

        public bool IsAnyKeyPressed()
        {
            return _keys.Any((x) => x == 1);
        }

        public short GetPressedKey()
        {
            for (short i = 0; i < 16; i++)
            {
                if (_keys[i] == 1)
                {
                    return i;
                }
            }
            return -1;
        }

    }
}