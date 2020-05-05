using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    // TODO: Use DirectX key enum and create dictionary where we match them with WPF keys

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
            using var stream = new FileStream(dllFile, FileMode.Open);

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
            using FileStream stream = new FileStream(dllFile, FileMode.Open, FileAccess.ReadWrite);

            int i;

            stream.Seek(CodeAdress, SeekOrigin.Begin);

            // Write each key item
            for (i = 0; i < items.Count; i++)
            {
                await stream.WriteAsync(Code1, 0, 4);

                stream.WriteByte((byte)items[i].NewKey);

                await stream.WriteAsync(Code2, 0, 9);

                stream.WriteByte((byte)items[i].OriginalKey);

                await stream.WriteAsync(Code3, 0, 4);

                // Custom "jmp" instruction, you should always append to the final codef code
                await stream.WriteAsync(BitConverter.GetBytes((items.Count - i - 1) * 23), 0, 4);
            }

            // End code
            await stream.WriteAsync(Codef, 0, 7);

            uint n = 0xFFFE4082;

            n -= (uint)(23 * i);

            // Custom "jmp" instruction, go back to where we were at the beginning
            await stream.WriteAsync(BitConverter.GetBytes(n), 0, 4);

            // Set the remaining bytes to 0 until the end of the file
            while (stream.Position < stream.Length)
                stream.WriteByte(0);
        }

        /// <summary>
        /// Converts a dinput key code to a <see cref="Key"/>
        /// </summary>
        /// <param name="key">The key code to convert</param>
        /// <returns>The <see cref="Key"/></returns>
        public static Key GetKey(int key)
        {
            return key switch
            {
                0x01 => Key.Escape,
                0x02 => Key.D1,
                0x03 => Key.D2,
                0x04 => Key.D3,
                0x05 => Key.D4,
                0x06 => Key.D5,
                0x07 => Key.D6,
                0x08 => Key.D7,
                0x09 => Key.D8,
                0x0A => Key.D9,
                0x0B => Key.D0,
                0x0C => Key.OemMinus,
                //case 0x0D:
                //    return "EQUALS";
                //case 0x0E:
                //    return "BACK";
                0x0F => Key.Tab,
                0x10 => Key.Q,
                0x11 => Key.W,
                0x12 => Key.E,
                0x13 => Key.R,
                0x14 => Key.T,
                0x15 => Key.Y,
                0x16 => Key.U,
                0x17 => Key.I,
                0x18 => Key.O,
                0x19 => Key.P,
                //case 0x1A:
                //    return "LBRACKET";
                //case 0x1B:
                //    return "RBRACKET";
                0x1C => Key.Return,
                0x1D => Key.LeftCtrl,
                0x1E => Key.A,
                0x1F => Key.S,
                0x20 => Key.D,
                0x21 => Key.F,
                0x22 => Key.G,
                0x23 => Key.H,
                0x24 => Key.J,
                0x25 => Key.K,
                0x26 => Key.L,
                0x27 => Key.OemSemicolon,
                //case 0x28:
                //    return "APOSTROPHE";
                //case 0x29:
                //    return "GRAVE";
                0x2A => Key.LeftShift,
                //case 0x2B:
                //    return "BACKSLASH";
                0x2C => Key.Z,
                0x2D => Key.X,
                0x2E => Key.C,
                0x2F => Key.V,
                0x30 => Key.B,
                0x31 => Key.N,
                0x32 => Key.M,
                0x33 => Key.OemComma,
                0x34 => Key.OemPeriod,
                //case 0x35:
                //    return "SLASH";
                0x36 => Key.RightShift,
                0x37 => Key.Multiply,
                0x38 => Key.LeftAlt,
                0x39 => Key.Space,
                0x3A => Key.Capital,
                0x3B => Key.F1,
                0x3C => Key.F2,
                0x3D => Key.F3,
                0x3E => Key.F4,
                0x3F => Key.F5,
                0x40 => Key.F6,
                0x41 => Key.F7,
                0x42 => Key.F8,
                0x43 => Key.F9,
                0x44 => Key.F10,
                0x45 => Key.NumLock,
                0x46 => Key.Scroll,
                0x47 => Key.NumPad7,
                0x48 => Key.NumPad8,
                0x49 => Key.NumPad9,
                0x4A => Key.Subtract,
                0x4B => Key.NumPad4,
                0x4C => Key.NumPad5,
                0x4D => Key.NumPad6,
                0x4E => Key.Add,
                0x4F => Key.NumPad1,
                0x50 => Key.NumPad2,
                0x51 => Key.NumPad3,
                0x52 => Key.NumPad0,
                0x53 => Key.Decimal,
                0x57 => Key.F11,
                0x58 => Key.F12,
                0x64 => Key.F13,
                0x65 => Key.F14,
                0x66 => Key.F15,
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
                0x9D => Key.RightCtrl,
                0xA0 => Key.VolumeMute,
                //case 0xA1:
                //    return "CALCULATOR";
                0xA2 => Key.MediaPlayPause,
                0xA4 => Key.MediaStop,
                0xAE => Key.VolumeDown,
                0xB0 => Key.VolumeUp,
                0xB2 => Key.BrowserHome,
                //case 0xB3:
                //    return "NUMPADCOMMA";
                //case 0xB5:
                //    return "DIVIDE";
                //case 0xB7:
                //    return "SYSRQ";
                //case 0xB8:
                //    return "ALT_GR";
                0xC5 => Key.Pause,
                0xC7 => Key.Home,
                0xC8 => Key.Up,
                0xC9 => Key.PageUp,
                0xCB => Key.Left,
                0xCD => Key.Right,
                0xCF => Key.End,
                0xD0 => Key.Down,
                0xD1 => Key.PageDown,
                0xD2 => Key.Insert,
                0xD3 => Key.Delete,
                0xDB => Key.LWin,
                0xDC => Key.RWin,
                0xDD => Key.Apps,
                //case 0xDE:
                //    return "POWER";
                0xDF => Key.Sleep,
                //case 0xE3:
                //    return "WAKE";
                0xE5 => Key.BrowserSearch,
                0xE6 => Key.BrowserFavorites,
                0xE7 => Key.BrowserRefresh,
                0xE8 => Key.BrowserStop,
                0xE9 => Key.BrowserForward,
                0xEA => Key.BrowserBack,
                //case 0xEB:
                //    return "MYCOMPUTER";
                0xEC => Key.LaunchMail,
                0xED => Key.SelectMedia,
                _ => Key.None
            };
        }

        /// <summary>
        /// Converts a <see cref="Key"/> to a dinput key code
        /// </summary>
        /// <param name="key">The key to convert</param>
        /// <returns>The dinput key code</returns>
        public static int GetKeyCode(Key key)
        {
            // IDEA: Update below conversion with complete list from GetKey method?

            // Handle numbers 1-9
            if ((int)key <= 43 && (int)key >= 35)
                return (int)key - 33;

            // Handle F1-F10
            if ((int)key <= 99 && (int)key >= 90)
                return (int)key - 31;

            return key switch
            {
                Key.LeftShift => 0x2A,
                Key.RightShift => 0x36,
                Key.LeftCtrl => 0x1D,
                Key.RightCtrl => 0x9D,
                Key.LeftAlt => 0x38,
                Key.RightAlt => 0xB8,
                Key.Up => 0xC8,
                Key.Left => 0xCB,
                Key.Right => 0xCD,
                Key.Down => 0xD0,
                Key.Space => 0x39,
                Key.Tab => 0x0F,
                Key.D0 => 0x0B,
                Key.F11 => 0x57,
                Key.F12 => 0x58,
                Key.Escape => 0x01,
                Key.Back => 0x0E,
                Key.Enter => 0x1C,
                Key.CapsLock => 0x3A,
                Key.Pause => 0xC5,
                Key.Home => 0xC7,
                Key.End => 0xCF,
                Key.PageUp => 0xC9,
                Key.PageDown => 0xD1,
                Key.Insert => 0xD2,
                Key.Delete => 0xD3,
                Key.OemTilde => 0x29,
                Key.OemComma => 0x33,
                Key.OemPeriod => 0x34,
                Key.OemBackslash => 0x2B,
                Key.A => 0x1E,
                Key.B => 0x30,
                Key.C => 0x2E,
                Key.D => 0x20,
                Key.E => 0x12,
                Key.F => 0x21,
                Key.G => 0x22,
                Key.H => 0x23,
                Key.I => 0x17,
                Key.J => 0x24,
                Key.K => 0x25,
                Key.L => 0x26,
                Key.M => 0x32,
                Key.N => 0x31,
                Key.O => 0x18,
                Key.P => 0x19,
                Key.Q => 0x10,
                Key.R => 0x13,
                Key.S => 0x1F,
                Key.T => 0x14,
                Key.U => 0x16,
                Key.V => 0x2F,
                Key.W => 0x11,
                Key.X => 0x2D,
                Key.Y => 0x15,
                Key.Z => 0x2C,
                Key.NumPad0 => 0x52,
                Key.NumPad1 => 0x4F,
                Key.NumPad2 => 0x50,
                Key.NumPad3 => 0x51,
                Key.NumPad4 => 0x4B,
                Key.NumPad5 => 0x4C,
                Key.NumPad6 => 0x4D,
                Key.NumPad7 => 0x47,
                Key.NumPad8 => 0x48,
                Key.NumPad9 => 0x49,
                _ => 0
            };
        }

        #endregion

        #region Private Fields

        private static readonly byte[] Code1 = { 0x8B, 0xC3, 0x83, 0xE8 };
        private static readonly byte[] Code2 = { 0x0F, 0x85, 0x0C, 0x00, 0x00, 0x00, 0xC7, 0x45, 0x10 };
        private static readonly byte[] Code3 = { 0x00, 0x00, 0x00, 0xE9 };
        private static readonly byte[] Codef = { 0xFF, 0x75, 0x10, 0xFF, 0x75, 0x0C, 0xE9 };

        #endregion

        #region Private Constants

        /// <summary>
        /// The offset of the DLL almost to the end of this, from which the code is injected
        /// </summary>
        private const int CodeAdress = 0x213c3;

        #endregion
    }
}