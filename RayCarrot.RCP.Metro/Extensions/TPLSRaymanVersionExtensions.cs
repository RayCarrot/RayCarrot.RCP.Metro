using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="TPLSRaymanVersion"/>
    /// </summary>
    public static class TPLSRaymanVersionExtensions
    {
        public static int GetWorldBase(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x16D310;

                case TPLSRaymanVersion.Ray_1_12_0:
                    return 0x16D804;

                case TPLSRaymanVersion.Ray_1_12_1:
                    return 0x16D814;

                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x16D5B4;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x16E868;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x16E7D8;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetBufferSize(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x174FE;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x174F0;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x17526;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x17526;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetLevel(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x00020;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x0001C;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x00034;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x00034;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetInLevel(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x02228;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x02278;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x022C0;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x022C0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetMusicOnOff(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x02234;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x02232;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x02278;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x02278;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetOptionsOn(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x174FB;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x174E7;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x17523;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x17523;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetOptionsOff(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x174FD;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x174E9;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x17525;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x17525;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetBossEvent(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x02257;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x02256;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x022A0;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x022A0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetXAxis(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x00E5C;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x00E54;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x00EA0;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x00EA0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }

        public static int GetYAxis(this TPLSRaymanVersion raymanVersion)
        {
            switch (raymanVersion)
            {
                case TPLSRaymanVersion.Auto:
                    throw new ArgumentException("Auto is not a supported Rayman version");

                case TPLSRaymanVersion.Ray_1_00:
                    return 0x00E60;

                case TPLSRaymanVersion.Ray_1_12_0:
                case TPLSRaymanVersion.Ray_1_12_1:
                case TPLSRaymanVersion.Ray_1_12_2:
                    return 0x00E58;

                case TPLSRaymanVersion.Ray_1_20:
                    return 0x00EA4;

                case TPLSRaymanVersion.Ray_1_21:
                    return 0x00EA4;

                default:
                    throw new ArgumentOutOfRangeException(nameof(raymanVersion));
            }
        }
    }
}