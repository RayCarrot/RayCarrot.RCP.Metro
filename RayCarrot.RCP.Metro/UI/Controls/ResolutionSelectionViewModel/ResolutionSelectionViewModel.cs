using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    public class ResolutionSelectionViewModel : BaseRCPViewModel
    {
        #region Constructor

        public ResolutionSelectionViewModel()
        {
            AvailableResolutions = new ObservableCollection<string>();
        }

        #endregion

        #region Private Fields

        private int _width;
        private int _height;

        #endregion

        #region Public Properties

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                OnPropertyChanged(nameof(SelectedResolution));
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                OnPropertyChanged(nameof(SelectedResolution));
            }
        }

        public ObservableCollection<string> AvailableResolutions { get; }
        public string SelectedResolution
        {
            get => $"{Width} x {Height}";
            set
            {
                if (value == null)
                {
                    ResetResolutionToDefault();
                    return;
                }

                string[] values = value.Split('x').Select(x => x.Trim()).ToArray();

                if (values.Length != 2)
                {
                    ResetResolutionToDefault();
                    return;
                }

                Width = Int32.TryParse(values[0], out int w) ? w : 0;
                Height = Int32.TryParse(values[1], out int h) ? h : 0;

                OnResolutionChanged();
            }
        }

        #endregion

        #region Public Methods

        public void ResetResolutionToDefault()
        {
            Width = (int)SystemParameters.PrimaryScreenWidth;
            Height = (int)SystemParameters.PrimaryScreenHeight;
        }

        public void GetAvailableResolutions(int minWidth = 0, int minHeight = 0)
        {
            lock (AvailableResolutions)
            {
                Metro.App.Current.Dispatcher.Invoke(() =>
                {
                    AvailableResolutions.Clear();

                    DEVMODE vDevMode = new DEVMODE();
                    var res = new HashSet<Vector2>();

                    int i = 0;
                    while (EnumDisplaySettings(null, i, ref vDevMode))
                    {
                        if (vDevMode.dmPelsWidth >= minWidth && vDevMode.dmPelsHeight >= minHeight)
                            res.Add(new Vector2(vDevMode.dmPelsWidth, vDevMode.dmPelsHeight));

                        i++;
                    }

                    AvailableResolutions.AddRange(res.OrderBy(x => x.X).ThenBy(x => x.Y).Reverse().Select(x => $"{x.X} x {x.Y}"));
                });
            }
        }

        #endregion

        #region P/Invoke

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        #endregion

        #region Events

        protected virtual void OnResolutionChanged() => ResolutionChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler ResolutionChanged;

        #endregion
    }
}