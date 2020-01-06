using RayCarrot.WPF;
using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="TPLSRaymanVersion"/> to an <see cref="Int32"/> index
    /// </summary>
    public class TPLSRaymanVersionEnumToComboBoxIndexConverter : BaseValueConverter<TPLSRaymanVersionEnumToComboBoxIndexConverter, TPLSRaymanVersion, int>
    {
        public override int ConvertValue(TPLSRaymanVersion value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                TPLSRaymanVersion.Auto => 0,
                TPLSRaymanVersion.Ray_1_00 => 1,
                TPLSRaymanVersion.Ray_1_10 => 2,
                TPLSRaymanVersion.Ray_1_12_0 => 3,
                TPLSRaymanVersion.Ray_1_12_1 => 4,
                TPLSRaymanVersion.Ray_1_12_2 => 5,
                TPLSRaymanVersion.Ray_1_20 => 6,
                TPLSRaymanVersion.Ray_1_21 => 7,
                TPLSRaymanVersion.Ray_1_21_Chinese => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public override TPLSRaymanVersion ConvertValueBack(int value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                0 => TPLSRaymanVersion.Auto,
                1 => TPLSRaymanVersion.Ray_1_00,
                2 => TPLSRaymanVersion.Ray_1_10,
                3 => TPLSRaymanVersion.Ray_1_12_0,
                4 => TPLSRaymanVersion.Ray_1_12_1,
                5 => TPLSRaymanVersion.Ray_1_12_2,
                6 => TPLSRaymanVersion.Ray_1_20,
                7 => TPLSRaymanVersion.Ray_1_21,
                8 => TPLSRaymanVersion.Ray_1_21_Chinese,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}