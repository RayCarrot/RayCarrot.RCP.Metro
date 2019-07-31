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
            switch (value)
            {
                case TPLSRaymanVersion.Auto:
                    return 0;

                case TPLSRaymanVersion.Ray_1_00:
                    return 1;

                case TPLSRaymanVersion.Ray_1_10:
                    return 2;

                case TPLSRaymanVersion.Ray_1_12_0:
                    return 3;

                case TPLSRaymanVersion.Ray_1_12_1:
                    return 4;

                case TPLSRaymanVersion.Ray_1_12_2:
                    return 5;

                case TPLSRaymanVersion.Ray_1_20:
                    return 6;

                case TPLSRaymanVersion.Ray_1_21:
                    return 7;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public override TPLSRaymanVersion ConvertValueBack(int value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case 0:
                    return TPLSRaymanVersion.Auto;

                case 1:
                    return TPLSRaymanVersion.Ray_1_00;

                case 2:
                    return TPLSRaymanVersion.Ray_1_10;

                case 3:
                    return TPLSRaymanVersion.Ray_1_12_0;

                case 4:
                    return TPLSRaymanVersion.Ray_1_12_1;

                case 5:
                    return TPLSRaymanVersion.Ray_1_12_2;

                case 6:
                    return TPLSRaymanVersion.Ray_1_20;

                case 7:
                    return TPLSRaymanVersion.Ray_1_21;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}