#nullable disable
using RayCarrot.Rayman;
using System.Collections.Generic;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public static class DirectXKeyHelpers
{
    static DirectXKeyHelpers()
    {
        DirectXWPFKeyTable = new Dictionary<DirectXKey, Key>()
        { 
            [DirectXKey.Escape] = Key.Escape,
            [DirectXKey.D1] = Key.D1,
            [DirectXKey.D2] = Key.D2,
            [DirectXKey.D3] = Key.D3,
            [DirectXKey.D4] = Key.D4,
            [DirectXKey.D5] = Key.D5,
            [DirectXKey.D6] = Key.D6,
            [DirectXKey.D7] = Key.D7,
            [DirectXKey.D8] = Key.D8,
            [DirectXKey.D9] = Key.D9,
            [DirectXKey.D0] = Key.D0,
            [DirectXKey.Minus] = Key.OemMinus,
            //[DirectXKey.Equals] = ,
            [DirectXKey.BackSpace] = Key.Back,
            [DirectXKey.Tab] = Key.Tab,
            [DirectXKey.Q] = Key.Q,
            [DirectXKey.W] = Key.W,
            [DirectXKey.E] = Key.E,
            [DirectXKey.R] = Key.R,
            [DirectXKey.T] = Key.T,
            [DirectXKey.Y] = Key.Y,
            [DirectXKey.U] = Key.U,
            [DirectXKey.I] = Key.I,
            [DirectXKey.O] = Key.O,
            [DirectXKey.P] = Key.P,
            [DirectXKey.LeftBracket] = Key.OemOpenBrackets,
            [DirectXKey.RightBracket] = Key.OemCloseBrackets,
            [DirectXKey.Return] = Key.Return,
            [DirectXKey.LeftControl] = Key.LeftCtrl,
            [DirectXKey.A] = Key.A,
            [DirectXKey.S] = Key.S,
            [DirectXKey.D] = Key.D,
            [DirectXKey.F] = Key.F,
            [DirectXKey.G] = Key.G,
            [DirectXKey.H] = Key.H,
            [DirectXKey.J] = Key.J,
            [DirectXKey.K] = Key.K,
            [DirectXKey.L] = Key.L,
            [DirectXKey.SemiColon] = Key.OemSemicolon,
            //[DirectXKey.Apostrophe] = ,
            //[DirectXKey.Grave] = ,
            [DirectXKey.LeftShift] = Key.LeftShift,
            //[DirectXKey.BackSlash] = ,
            [DirectXKey.Z] = Key.Z,
            [DirectXKey.X] = Key.X,
            [DirectXKey.C] = Key.C,
            [DirectXKey.V] = Key.V,
            [DirectXKey.B] = Key.B,
            [DirectXKey.N] = Key.N,
            [DirectXKey.M] = Key.M,
            [DirectXKey.Comma] = Key.OemComma,
            [DirectXKey.Period] = Key.OemPeriod,
            //[DirectXKey.Slash] = ,
            [DirectXKey.RightShift] = Key.RightShift,
            [DirectXKey.NumPadStar] = Key.Multiply,
            [DirectXKey.Multiply] = Key.Multiply,
            [DirectXKey.LeftAlt] = Key.LeftAlt,
            [DirectXKey.LeftMenu] = Key.LeftAlt,
            [DirectXKey.Space] = Key.Space,
            [DirectXKey.CapsLock] = Key.Capital,
            [DirectXKey.Capital] = Key.Capital,
            [DirectXKey.F1] = Key.F1,
            [DirectXKey.F2] = Key.F2,
            [DirectXKey.F3] = Key.F3,
            [DirectXKey.F4] = Key.F4,
            [DirectXKey.F5] = Key.F5,
            [DirectXKey.F6] = Key.F6,
            [DirectXKey.F7] = Key.F7,
            [DirectXKey.F8] = Key.F8,
            [DirectXKey.F9] = Key.F9,
            [DirectXKey.F10] = Key.F10,
            [DirectXKey.Numlock] = Key.NumLock,
            [DirectXKey.Scroll] = Key.Scroll,
            [DirectXKey.NumPad7] = Key.NumPad7,
            [DirectXKey.NumPad8] = Key.NumPad8,
            [DirectXKey.NumPad9] = Key.NumPad9,
            [DirectXKey.NumPadMinus] = Key.Subtract,
            [DirectXKey.NumPad4] = Key.NumPad4,
            [DirectXKey.NumPad5] = Key.NumPad5,
            [DirectXKey.NumPad6] = Key.NumPad6,
            [DirectXKey.NumPadPlus] = Key.Add,
            [DirectXKey.NumPad1] = Key.NumPad1,
            [DirectXKey.NumPad2] = Key.NumPad2,
            [DirectXKey.NumPad3] = Key.NumPad3,
            [DirectXKey.NumPad0] = Key.NumPad0,
            [DirectXKey.Decimal] = Key.Decimal,
            [DirectXKey.NumPadPeriod] = Key.Decimal,
            [DirectXKey.OEM102] = Key.Oem102,
            [DirectXKey.F11] = Key.F11,
            [DirectXKey.F12] = Key.F12,
            [DirectXKey.F13] = Key.F13,
            [DirectXKey.F14] = Key.F14,
            [DirectXKey.F15] = Key.F15,
            [DirectXKey.Kana] = Key.KanaMode,
            [DirectXKey.AbntC1] = Key.AbntC1,
            [DirectXKey.Convert] = Key.ImeConvert,
            [DirectXKey.NoConvert] = Key.ImeNonConvert,
            //[DirectXKey.Yen] = ,
            [DirectXKey.AbntC2] = Key.AbntC2,
            //[DirectXKey.NumPadEquals] = ,
            [DirectXKey.Circumflex] = Key.MediaPreviousTrack,
            [DirectXKey.PrevTrack] = Key.MediaPreviousTrack,
            //[DirectXKey.At] = ,
            //[DirectXKey.Colon] = ,
            //[DirectXKey.Underline] = ,
            [DirectXKey.Kanji] = Key.KanjiMode,
            [DirectXKey.Stop] = Key.MediaStop,
            //[DirectXKey.AX] = ,
            //[DirectXKey.Unlabeled] = ,
            [DirectXKey.NextTrack] = Key.MediaNextTrack,
            //[DirectXKey.NumPadEnter] = ,
            [DirectXKey.RightControl] = Key.RightCtrl,
            [DirectXKey.Mute] = Key.VolumeMute,
            //[DirectXKey.Calculator] = ,
            [DirectXKey.PlayPause] = Key.MediaPlayPause,
            [DirectXKey.MediaStop] = Key.MediaStop,
            [DirectXKey.VolumeDown] = Key.VolumeDown,
            [DirectXKey.VolumeUp] = Key.VolumeUp,
            [DirectXKey.WebHome] = Key.BrowserHome,
            //[DirectXKey.NumPadComma] = ,
            [DirectXKey.Divide] = Key.Divide,
            //[DirectXKey.SysRq] = ,
            [DirectXKey.RightAlt] = Key.RightAlt,
            [DirectXKey.RightMenu] = Key.RightAlt,
            [DirectXKey.Pause] = Key.Pause,
            [DirectXKey.Home] = Key.Home,
            [DirectXKey.UpArrow] = Key.Up,
            [DirectXKey.Up] = Key.Up,
            [DirectXKey.PageUp] = Key.PageUp,
            [DirectXKey.Prior] = Key.PageUp,
            [DirectXKey.LeftArrow] = Key.Left,
            [DirectXKey.Left] = Key.Left,
            [DirectXKey.RightArrow] = Key.Right,
            [DirectXKey.Right] = Key.Right,
            [DirectXKey.End] = Key.End,
            [DirectXKey.DownArrow] = Key.Down,
            [DirectXKey.Down] = Key.Down,
            [DirectXKey.Next] = Key.PageDown,
            [DirectXKey.PageDown] = Key.PageDown,
            [DirectXKey.Insert] = Key.Insert,
            [DirectXKey.Delete] = Key.Delete,
            [DirectXKey.LeftWindows] = Key.LWin,
            [DirectXKey.RightWindows] = Key.RWin,
            [DirectXKey.Apps] = Key.Apps,
            //[DirectXKey.Power] = ,
            [DirectXKey.Sleep] = Key.Sleep,
            //[DirectXKey.Wake] = ,
            [DirectXKey.WebSearch] = Key.BrowserSearch,
            [DirectXKey.WebFavorites] = Key.BrowserFavorites,
            [DirectXKey.WebRefresh] = Key.BrowserRefresh,
            [DirectXKey.WebStop] = Key.BrowserStop,
            [DirectXKey.WebForward] = Key.BrowserForward,
            [DirectXKey.WebBack] = Key.BrowserBack,
            //[DirectXKey.MyComputer] = ,
            [DirectXKey.Mail] = Key.LaunchMail,
            [DirectXKey.MediaSelect] = Key.SelectMedia,
        };

        WPFDirectXKeyTable = new Dictionary<Key, DirectXKey>();

        foreach (var entry in DirectXWPFKeyTable)
        {
            if (!WPFDirectXKeyTable.ContainsKey(entry.Value))
                WPFDirectXKeyTable.Add(entry.Value, entry.Key);
        }
    }

    private static Dictionary<DirectXKey, Key> DirectXWPFKeyTable { get; }
    private static Dictionary<Key, DirectXKey> WPFDirectXKeyTable { get; }

    /// <summary>
    /// Converts a dinput key code to a <see cref="Key"/>
    /// </summary>
    /// <param name="key">The key code to convert</param>
    /// <returns>The <see cref="Key"/></returns>
    public static Key GetKey(int key) => DirectXWPFKeyTable.TryGetValue((DirectXKey)key);

    /// <summary>
    /// Converts a <see cref="Key"/> to a dinput key code
    /// </summary>
    /// <param name="key">The key to convert</param>
    /// <returns>The dinput key code</returns>
    public static int GetKeyCode(Key key) => (int)WPFDirectXKeyTable.TryGetValue(key);
}