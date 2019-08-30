using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Manages Rayman 2 button mapping through dinput.dll editing
    /// </summary>
    public static class R2ButtonMappingManager
    {
        #region Public Static Methods

        /// <summary>
        /// Loads the current button mapping
        /// </summary>
        /// <param name="dllFile">The dinput.dll path</param>
        /// <returns>The collection of the current mapping used</returns>
        public static async Task<HashSet<KeyMappingItem>> LoadCurrentMappingAsync(FileSystemPath dllFile)
        {
            var output = new HashSet<KeyMappingItem>();

            // Open the file in a new stream
            using (var stream = new FileStream(dllFile, FileMode.Open))
            {
                // Create the buffer to read into
                byte[] buffer = new byte[4];

                stream.Seek(CodeAdress + 14, SeekOrigin.Begin);
                await stream.ReadAsync(buffer, 0, 4);
                int originalKey = BitConverter.ToInt32(buffer, 0);

                // Get all key items
                while (originalKey > 0)
                {
                    stream.Seek(-14, SeekOrigin.Current);
                    await stream.ReadAsync(buffer, 0, 4);
                    var newKey = BitConverter.ToInt32(buffer, 0) & 0x000000FF;

                    output.Add(new KeyMappingItem(originalKey, newKey));

                    stream.Seek(29, SeekOrigin.Current);
                    await stream.ReadAsync(buffer, 0, 4);
                    originalKey = BitConverter.ToInt32(buffer, 0);
                }
            }

            return output;
        }

        /// <summary>
        /// Applies the specified button mapping
        /// </summary>
        /// <param name="dllFile">The dinput.dll path</param>
        /// <param name="items">The button mapping items to use</param>
        /// <returns>The task</returns>
        public static async Task ApplyMappingAsync(FileSystemPath dllFile, List<KeyMappingItem> items)
        {
            using (FileStream stream = new FileStream(dllFile, FileMode.Open, FileAccess.ReadWrite))
            {
                int i;

                stream.Seek(CodeAdress, SeekOrigin.Begin);

                // Write each key item
                for (i = 0; i < items.Count; i++)
                {
                    await stream.WriteAsync(_code1, 0, 4);

                    stream.WriteByte((byte)items[i].NewKey);

                    await stream.WriteAsync(_code2, 0, 9);

                    stream.WriteByte((byte)items[i].OriginalKey);

                    await stream.WriteAsync(_code3, 0, 4);

                    // Custom "jmp" instruction, you should always append to the final codef code
                    await stream.WriteAsync(BitConverter.GetBytes((items.Count - i - 1) * 23), 0, 4);
                }

                // End code
                await stream.WriteAsync(_codef, 0, 7);

                uint n = 0xFFFE4082;

                n -= (uint)(23 * i);

                // Custom "jmp" instruction, go back to where we were at the beginning
                await stream.WriteAsync(BitConverter.GetBytes(n), 0, 4);

                // Set the remaining bytes to 0 until the end of the file
                while (stream.Position < stream.Length)
                    stream.WriteByte(0);
            }
        }

        /// <summary>
        /// Converts a dinput key code to a <see cref="Key"/>
        /// </summary>
        /// <param name="key">The key code to convert</param>
        /// <returns>The <see cref="Key"/></returns>
        public static Key GetKey(int key)
        {
            switch (key)
            {
                case 0x01:
                    return Key.Escape;

                case 0x02:
                    return Key.D1;

                case 0x03:
                    return Key.D2;

                case 0x04:
                    return Key.D3;

                case 0x05:
                    return Key.D4;

                case 0x06:
                    return Key.D5;

                case 0x07:
                    return Key.D6;

                case 0x08:
                    return Key.D7;

                case 0x09:
                    return Key.D8;

                case 0x0A:
                    return Key.D9;

                case 0x0B:
                    return Key.D0;

                case 0x0C:
                    return Key.OemMinus;

                //case 0x0D:
                //    return "EQUALS";

                //case 0x0E:
                //    return "BACK";

                case 0x0F:
                    return Key.Tab;

                case 0x10:
                    return Key.Q;

                case 0x11:
                    return Key.W;

                case 0x12:
                    return Key.E;

                case 0x13:
                    return Key.R;

                case 0x14:
                    return Key.T;

                case 0x15:
                    return Key.Y;

                case 0x16:
                    return Key.U;

                case 0x17:
                    return Key.I;

                case 0x18:
                    return Key.O;

                case 0x19:
                    return Key.P;

                //case 0x1A:
                //    return "LBRACKET";

                //case 0x1B:
                //    return "RBRACKET";

                case 0x1C:
                    return Key.Return;

                case 0x1D:
                    return Key.LeftCtrl;

                case 0x1E:
                    return Key.A;

                case 0x1F:
                    return Key.S;

                case 0x20:
                    return Key.D;

                case 0x21:
                    return Key.F;

                case 0x22:
                    return Key.G;

                case 0x23:
                    return Key.H;

                case 0x24:
                    return Key.J;

                case 0x25:
                    return Key.K;

                case 0x26:
                    return Key.L;

                case 0x27:
                    return Key.OemSemicolon;

                //case 0x28:
                //    return "APOSTROPHE";

                //case 0x29:
                //    return "GRAVE";

                case 0x2A:
                    return Key.LeftShift;

                //case 0x2B:
                //    return "BACKSLASH";

                case 0x2C:
                    return Key.Z;

                case 0x2D:
                    return Key.X;

                case 0x2E:
                    return Key.C;

                case 0x2F:
                    return Key.V;

                case 0x30:
                    return Key.B;

                case 0x31:
                    return Key.N;

                case 0x32:
                    return Key.M;

                case 0x33:
                    return Key.OemComma;

                case 0x34:
                    return Key.OemPeriod;

                //case 0x35:
                //    return "SLASH";

                case 0x36:
                    return Key.RightShift;

                case 0x37:
                    return Key.Multiply;

                case 0x38:
                    return Key.LeftAlt;

                case 0x39:
                    return Key.Space;

                case 0x3A:
                    return Key.Capital;

                case 0x3B:
                    return Key.F1;

                case 0x3C:
                    return Key.F2;

                case 0x3D:
                    return Key.F3;

                case 0x3E:
                    return Key.F4;

                case 0x3F:
                    return Key.F5;

                case 0x40:
                    return Key.F6;

                case 0x41:
                    return Key.F7;

                case 0x42:
                    return Key.F8;

                case 0x43:
                    return Key.F9;

                case 0x44:
                    return Key.F10;

                case 0x45:
                    return Key.NumLock;

                case 0x46:
                    return Key.Scroll;

                case 0x47:
                    return Key.NumPad7;

                case 0x48:
                    return Key.NumPad8;

                case 0x49:
                    return Key.NumPad9;

                case 0x4A:
                    return Key.Subtract;

                case 0x4B:
                    return Key.NumPad4;

                case 0x4C:
                    return Key.NumPad5;

                case 0x4D:
                    return Key.NumPad6;

                case 0x4E:
                    return Key.Add;

                case 0x4F:
                    return Key.NumPad1;

                case 0x50:
                    return Key.NumPad2;

                case 0x51:
                    return Key.NumPad3;

                case 0x52:
                    return Key.NumPad0;

                case 0x53:
                    return Key.Decimal;

                case 0x57:
                    return Key.F11;

                case 0x58:
                    return Key.F12;

                case 0x64:
                    return Key.F13;

                case 0x65:
                    return Key.F14;

                case 0x66:
                    return Key.F15;

                //case 0x70:
                //    return "KANA";

                //case 0x73:
                //    return "ABNT_C1";

                //case 0x79:
                //    return "CONVERT";

                //case 0x7B:
                //    return "NOCONVERT";

                //case 0x7D:
                //    return "YEN";

                //case 0x7E:
                //    return "ABNT_C2";

                //case 0x8D:
                //    return "NUMPADEQUALS";

                //case 0x90:
                //    return "PREVTRACK";

                //case 0x91:
                //    return "AT";

                //case 0x92:
                //    return "COLON";

                //case 0x93:
                //    return "UNDERLINE";

                //case 0x94:
                //    return "KANJI";

                //case 0x95:
                //    return "STOP";

                //case 0x96:
                //    return "AX";

                //case 0x97:
                //    return "UNLABELED";

                //case 0x99:
                //    return Key.MediaNextTrack;

                //case 0x9C:
                //    return "NUMPADENTER";

                case 0x9D:
                    return Key.RightCtrl;

                case 0xA0:
                    return Key.VolumeMute;

                //case 0xA1:
                //    return "CALCULATOR";

                case 0xA2:
                    return Key.MediaPlayPause;

                case 0xA4:
                    return Key.MediaStop;

                case 0xAE:
                    return Key.VolumeDown;

                case 0xB0:
                    return Key.VolumeUp;

                case 0xB2:
                    return Key.BrowserHome;

                //case 0xB3:
                //    return "NUMPADCOMMA";

                //case 0xB5:
                //    return "DIVIDE";

                //case 0xB7:
                //    return "SYSRQ";

                //case 0xB8:
                //    return "ALT_GR";

                case 0xC5:
                    return Key.Pause;

                case 0xC7:
                    return Key.Home;

                case 0xC8:
                    return Key.Up;

                case 0xC9:
                    return Key.PageUp;

                case 0xCB:
                    return Key.Left;

                case 0xCD:
                    return Key.Right;

                case 0xCF:
                    return Key.End;

                case 0xD0:
                    return Key.Down;

                case 0xD1:
                    return Key.PageDown;

                case 0xD2:
                    return Key.Insert;

                case 0xD3:
                    return Key.Delete;

                case 0xDB:
                    return Key.LWin;

                case 0xDC:
                    return Key.RWin;

                case 0xDD:
                    return Key.Apps;

                //case 0xDE:
                //    return "POWER";

                case 0xDF:
                    return Key.Sleep;

                //case 0xE3:
                //    return "WAKE";

                case 0xE5:
                    return Key.BrowserSearch;

                case 0xE6:
                    return Key.BrowserFavorites;

                case 0xE7:
                    return Key.BrowserRefresh;

                case 0xE8:
                    return Key.BrowserStop;

                case 0xE9:
                    return Key.BrowserForward;

                case 0xEA:
                    return Key.BrowserBack;

                //case 0xEB:
                //    return "MYCOMPUTER";

                case 0xEC:
                    return Key.LaunchMail;

                case 0xED:
                    return Key.SelectMedia;

                default:
                    return Key.None;
            }
        }

        /// <summary>
        /// Converts a <see cref="Key"/> to a dinput key code
        /// </summary>
        /// <param name="key">The key to convert</param>
        /// <returns>The dinput key code</returns>
        public static int GetKeyCode(Key key)
        {
            // TODO: Update below conversion with complete list from GetKey method?

            // Handle numbers 1-9
            if ((int)key <= 43 && (int)key >= 35)
                return (int)key - 33;

            // Handle F1-F10
            if ((int)key <= 99 && (int)key >= 90)
                return (int)key - 31;

            switch (key)
            {
                case Key.LeftShift:
                    return 0x2A;

                case Key.RightShift:
                    return 0x36;

                case Key.LeftCtrl:
                    return 0x1D;

                case Key.RightCtrl:
                    return 0x9D;

                case Key.LeftAlt:
                    return 0x38;

                case Key.RightAlt:
                    return 0xB8;

                case Key.Up:
                    return 0xC8;

                case Key.Left:
                    return 0xCB;

                case Key.Right:
                    return 0xCD;

                case Key.Down:
                    return 0xD0;

                case Key.Space:
                    return 0x39;

                case Key.Tab:
                    return 0x0F;

                case Key.D0:
                    return 0x0B;

                case Key.F11:
                    return 0x57;

                case Key.F12:
                    return 0x58;

                case Key.Escape:
                    return 0x01;

                case Key.Back:
                    return 0x0E;

                case Key.Enter:
                    return 0x1C;

                case Key.CapsLock:
                    return 0x3A;

                case Key.Pause:
                    return 0xC5;

                case Key.Home:
                    return 0xC7;

                case Key.End:
                    return 0xCF;

                case Key.PageUp:
                    return 0xC9;

                case Key.PageDown:
                    return 0xD1;

                case Key.Insert:
                    return 0xD2;

                case Key.Delete:
                    return 0xD3;

                case Key.OemTilde:
                    return 0x29;

                case Key.OemComma:
                    return 0x33;

                case Key.OemPeriod:
                    return 0x34;

                case Key.OemBackslash:
                    return 0x2B;

                case Key.A:
                    return 0x1E;

                case Key.B:
                    return 0x30;

                case Key.C:
                    return 0x2E;

                case Key.D:
                    return 0x20;

                case Key.E:
                    return 0x12;

                case Key.F:
                    return 0x21;

                case Key.G:
                    return 0x22;

                case Key.H:
                    return 0x23;

                case Key.I:
                    return 0x17;

                case Key.J:
                    return 0x24;

                case Key.K:
                    return 0x25;

                case Key.L:
                    return 0x26;

                case Key.M:
                    return 0x32;

                case Key.N:
                    return 0x31;

                case Key.O:
                    return 0x18;

                case Key.P:
                    return 0x19;

                case Key.Q:
                    return 0x10;

                case Key.R:
                    return 0x13;

                case Key.S:
                    return 0x1F;

                case Key.T:
                    return 0x14;

                case Key.U:
                    return 0x16;

                case Key.V:
                    return 0x2F;

                case Key.W:
                    return 0x11;

                case Key.X:
                    return 0x2D;

                case Key.Y:
                    return 0x15;

                case Key.Z:
                    return 0x2C;

                case Key.NumPad0:
                    return 0x52;

                case Key.NumPad1:
                    return 0x4F;

                case Key.NumPad2:
                    return 0x50;

                case Key.NumPad3:
                    return 0x51;

                case Key.NumPad4:
                    return 0x4B;

                case Key.NumPad5:
                    return 0x4C;

                case Key.NumPad6:
                    return 0x4D;

                case Key.NumPad7:
                    return 0x47;

                case Key.NumPad8:
                    return 0x48;

                case Key.NumPad9:
                    return 0x49;

                default:
                    return 0;
            }
        }

        #endregion

        #region Private Fields

        private static readonly byte[] _code1 = { 0x8B, 0xC3, 0x83, 0xE8 };
        private static readonly byte[] _code2 = { 0x0F, 0x85, 0x0C, 0x00, 0x00, 0x00, 0xC7, 0x45, 0x10 };
        private static readonly byte[] _code3 = { 0x00, 0x00, 0x00, 0xE9 };
        private static readonly byte[] _codef = { 0xFF, 0x75, 0x10, 0xFF, 0x75, 0x0C, 0xE9 };

        #endregion

        #region Private Constants

        /// <summary>
        /// The offset of the DLL almost to the end of this, from which the code is injected
        /// </summary>
        private const int CodeAdress = 0x213c3;

        #endregion
    }
}