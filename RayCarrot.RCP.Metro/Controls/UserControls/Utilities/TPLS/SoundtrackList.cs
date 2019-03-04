namespace RayCarrot.RCP.Metro
{
    public static class SoundtrackList
    {
        private static readonly long[][] ReturnValue = new long[2][];

        public static long[][] GetSoundtrack(string Level, string World)
        {
            if (World == "RAY1")
            {
                switch (Level)
                {
                    case "RAY1":
                        ReturnValue[0] = new long[] { 31720237, 29542498 };
                        ReturnValue[1] = new long[] { 1222633, 2177739 };
                        break;

                    case "RAY2":
                        ReturnValue[0] = new long[] { 22786573, 20390262 };
                        ReturnValue[1] = new long[] { 656206, 2396311 };
                        break;

                    case "RAY3":
                        return null;

                    case "RAY4":
                        ReturnValue[0] = new long[] { 28460009, 26510592 };
                        ReturnValue[1] = new long[] { 1082489, 1949417 };
                        break;

                    case "RAY5":
                        ReturnValue[0] = new long[] { 31720237, 29542498 };
                        ReturnValue[1] = new long[] { 1222633, 2177739 };
                        break;

                    case "RAY6":
                    case "RAY7":
                    case "RAY8":
                    case "RAY9":
                        return null;
                    case "RAY10":
                        ReturnValue[0] = new long[] { 28460009, 26510592 };
                        ReturnValue[1] = new long[] { 1082489, 1949417 };
                        break;
                    case "RAY11":
                        ReturnValue[0] = new long[] { 28460009, 26510592 };
                        ReturnValue[1] = new long[] { 1082489, 1949417 };
                        break;
                    case "RAY12":
                        ReturnValue[0] = new long[] { 31720237, 29542498 };
                        ReturnValue[1] = new long[] { 1222633, 2177739 };
                        break;
                    case "RAY13":
                        ReturnValue[0] = new long[] { 22786573, 20390262 };
                        ReturnValue[1] = new long[] { 656206, 2396311 };
                        break;
                    case "RAY14":
                        ReturnValue[0] = new long[] { 87789323, 85872167 };
                        ReturnValue[1] = new long[] { 560786, 1917156 };
                        break;
                    case "RAY15":
                        ReturnValue[0] = new long[] { 35162640, 32942870 };
                        ReturnValue[1] = new long[] { 469006, 2219770 };
                        break;
                    case "RAY16":
                        ReturnValue[0] = new long[] { 37576545, 36573839 };
                        ReturnValue[1] = new long[] { 1210122, 1002706 };
                        break;
                    case "RAY17":
                    case "RAY18":
                    case "RAY19":
                    case "RAY20":
                    case "RAY21":
                    case "RAY22":
                    default:
                        return null;
                }
            }
            else if (World == "RAY2")
            {
                switch (Level)
                {
                    case "RAY1":
                        ReturnValue[0] = new long[] { 63268113, 60977646 };
                        ReturnValue[1] = new long[] { 495197, 2290467 };
                        break;
                    case "RAY2":
                        ReturnValue[0] = new long[] { 54715946, 52302652 };
                        ReturnValue[1] = new long[] { 972212, 2413294 };
                        break;
                    case "RAY3":
                        ReturnValue[0] = new long[] { 48840974, 46327922 };
                        ReturnValue[1] = new long[] { 931093, 2513052 };
                        break;
                    case "RAY4":
                        ReturnValue[0] = new long[] { 72360982, 71234026 };
                        ReturnValue[1] = new long[] { 368425, 1126956 };
                        break;
                    case "RAY5":
                        ReturnValue[0] = new long[] { 54715946, 52302652 };
                        ReturnValue[1] = new long[] { 972212, 2413294 };
                        break;
                    case "RAY6":
                        ReturnValue[0] = new long[] { 68284466, 66895587 };
                        ReturnValue[1] = new long[] { 999993, 1388879 };
                        break;
                    case "RAY7":
                        ReturnValue[0] = new long[] { 70729506, 69284459 };
                        ReturnValue[1] = new long[] { 504520, 1445047 };
                        break;
                    case "RAY8":
                        ReturnValue[0] = new long[] { 63268113, 60977646 };
                        ReturnValue[1] = new long[] { 495197, 2290467 };
                        break;
                    case "RAY9":
                        ReturnValue[0] = new long[] { 70729506, 69284459 };
                        ReturnValue[1] = new long[] { 504520, 1445047 };
                        break;
                    case "RAY10":
                        ReturnValue[0] = new long[] { 51971460, 49772067 };
                        ReturnValue[1] = new long[] { 331192, 2199393 };
                        break;
                    case "RAY11":
                        return null;
                    case "RAY12":
                        ReturnValue[0] = new long[] { 57329229, 55688158 };
                        ReturnValue[1] = new long[] { 878792, 1641071 };
                        break;
                    case "RAY13":
                        ReturnValue[0] = new long[] { 65467614, 63763310 };
                        ReturnValue[1] = new long[] { 1427973, 1704304 };
                        break;
                    case "RAY14":
                        ReturnValue[0] = new long[] { 51971460, 49772067 };
                        ReturnValue[1] = new long[] { 331192, 2199393 };
                        break;
                    case "RAY15":
                        ReturnValue[0] = new long[] { 60084178, 58208021 };
                        ReturnValue[1] = new long[] { 893468, 1876157 };
                        break;
                    case "RAY16":
                        ReturnValue[0] = new long[] { 60084178, 58208021 };
                        ReturnValue[1] = new long[] { 893468, 1876157 };
                        break;
                    case "RAY17":
                        return null;
                    case "RAY18":
                        return null;
                }
            }
            else if (World == "RAY3")
            {
                switch (Level)
                {
                    case "RAY1":
                        ReturnValue[0] = new long[] { 82013734, 80248120 };
                        ReturnValue[1] = new long[] { 319362, 1765614 };
                        break;
                    case "RAY2":
                        ReturnValue[0] = new long[] { 82013734, 80248120 };
                        ReturnValue[1] = new long[] { 319362, 1765614 };
                        break;
                    case "RAY3":
                        ReturnValue[0] = new long[] { 75280276, 72729407 };
                        ReturnValue[1] = new long[] { 681754, 2550869 };
                        break;
                    case "RAY4":
                        ReturnValue[0] = new long[] { 84951434, 82333096 };
                        ReturnValue[1] = new long[] { 920733, 2618338 };
                        break;
                    case "RAY5":
                        ReturnValue[0] = new long[] { 82013734, 80248120 };
                        ReturnValue[1] = new long[] { 319362, 1765614 };
                        break;
                    case "RAY6":
                        ReturnValue[0] = new long[] { 82013734, 80248120 };
                        ReturnValue[1] = new long[] { 319362, 1765614 };
                        break;
                    case "RAY7":
                        ReturnValue[0] = new long[] { 87789323, 85872167 };
                        ReturnValue[1] = new long[] { 560786, 1917156 };
                        break;
                    case "RAY8":
                        ReturnValue[0] = new long[] { 41987417, 38786667 };
                        ReturnValue[1] = new long[] { 1198254, 3200750 };
                        break;
                    case "RAY9":
                        ReturnValue[0] = new long[] { 89538037, 88350109 };
                        ReturnValue[1] = new long[] { 283812, 1187928 };
                        break;
                    case "RAY10":
                        ReturnValue[0] = new long[] { 78535486, 77089354 };
                        ReturnValue[1] = new long[] { 1712634, 1446132 };
                        break;
                    case "RAY11":
                        return null;
                    case "RAY12":
                        return null;
                    case "RAY13":
                        return null;
                }
            }
            else if (World == "RAY4")
            {
                switch (Level)
                {
                    case "RAY1":
                        ReturnValue[0] = new long[] { 94340731, 92766271 };
                        ReturnValue[1] = new long[] { 275891, 1574460 };
                        break;
                    case "RAY2":
                        ReturnValue[0] = new long[] { 99668617, 97779195 };
                        ReturnValue[1] = new long[] { 768697, 1889422 };
                        break;
                    case "RAY3":
                        ReturnValue[0] = new long[] { 91952950, 89821849 };
                        ReturnValue[1] = new long[] { 813321, 2131101 };
                        break;
                    case "RAY4":
                        ReturnValue[0] = new long[] { 102810820, 100437314 };
                        ReturnValue[1] = new long[] { 311316, 2373506 };
                        break;
                    case "RAY5":
                        ReturnValue[0] = new long[] { 99668617, 97779195 };
                        ReturnValue[1] = new long[] { 768697, 1889422 };
                        break;
                    case "RAY6":
                        ReturnValue[0] = new long[] { 97313075, 94616622 };
                        ReturnValue[1] = new long[] { 466120, 2696453 };
                        break;
                    case "RAY7":
                        ReturnValue[0] = new long[] { 91952950, 89821849 };
                        ReturnValue[1] = new long[] { 813321, 2131101 };
                        break;
                    case "RAY8":
                        ReturnValue[0] = new long[] { 97313075, 94616622 };
                        ReturnValue[1] = new long[] { 466120, 2696453 };
                        break;
                    case "RAY9":
                        ReturnValue[0] = new long[] { 72360982, 71234026 };
                        ReturnValue[1] = new long[] { 368425, 1126956 };
                        break;
                    case "RAY10":
                        ReturnValue[0] = new long[] { 87789323, 85872167 };
                        ReturnValue[1] = new long[] { 560786, 1917156 };
                        break;
                    case "RAY11":
                        ReturnValue[0] = new long[] { 105417855, 103122136 };
                        ReturnValue[1] = new long[] { 1364821, 2295719 };
                        break;
                    case "RAY12":
                        return null;
                    case "RAY13":
                        return null;
                }
            }
            else if (World == "RAY5")
            {
                switch (Level)
                {
                    case "RAY1":
                        ReturnValue[0] = new long[] { 112108237, 109625356 };
                        ReturnValue[1] = new long[] { 315572, 2482881 };
                        break;
                    case "RAY2":
                        ReturnValue[0] = new long[] { 2072694, 224859 };
                        ReturnValue[1] = new long[] { 533234, 1847835 };
                        break;
                    case "RAY3":
                        ReturnValue[0] = new long[] { 117543817, 115202576 };
                        ReturnValue[1] = new long[] { 522661, 2341241 };
                        break;
                    case "RAY4":
                        ReturnValue[0] = new long[] { 0, 118066478 };
                        ReturnValue[1] = new long[] { 224859, 2257449 };
                        break;
                    case "RAY5":
                        ReturnValue[0] = new long[] { 2072694, 224859 };
                        ReturnValue[1] = new long[] { 533234, 1847835 };
                        break;
                    case "RAY6":
                        ReturnValue[0] = new long[] { 0, 118066478 };
                        ReturnValue[1] = new long[] { 224859, 2257449 };
                        break;
                    case "RAY7":
                        ReturnValue[0] = new long[] { 2072694, 224859 };
                        ReturnValue[1] = new long[] { 533234, 1847835 };
                        break;
                    case "RAY8":
                        ReturnValue[0] = new long[] { 117543817, 115202576 };
                        ReturnValue[1] = new long[] { 522661, 2341241 };
                        break;
                    case "RAY9":
                        ReturnValue[0] = new long[] { 108802337, 106782676 };
                        ReturnValue[1] = new long[] { 823019, 2019661 };
                        break;
                    case "RAY10":
                        ReturnValue[0] = new long[] { 114567259, 112423809 };
                        ReturnValue[1] = new long[] { 635317, 2143450 };
                        break;
                    case "RAY11":
                        ReturnValue[0] = new long[] { 114567259, 112423809 };
                        ReturnValue[1] = new long[] { 635317, 2143450 };
                        break;
                    case "RAY12":
                        return null;
                    case "RAY13":
                        return null;

                }
            }
            else if (World == "RAY6")
            {
                switch (Level)
                {
                    case "RAY1":
                        ReturnValue[0] = new long[] { 8434183, 6531785 };
                        ReturnValue[1] = new long[] { 840804, 1902398 };
                        break;
                    case "RAY2":
                        ReturnValue[0] = new long[] { 6207028, 4923023 };
                        ReturnValue[1] = new long[] { 324757, 1284005 };
                        break;
                    case "RAY3":
                        ReturnValue[0] = new long[] { 4804876, 2605928 };
                        ReturnValue[1] = new long[] { 118147, 2198948 };
                        break;
                    case "RAY4":
                        ReturnValue[0] = new long[] { 11669736, 9274987 };
                        ReturnValue[1] = new long[] { 454217, 2394749 };
                        break;
                }
            }

            return ReturnValue;
        }

        public static long[][] GetMidi(string Level, string World)
        {
            if (World == "RAY1" && Level == "RAY2")
            {
                ReturnValue[0] = new long[] { 16214875 };
                ReturnValue[1] = new long[] { 1661628 };
                return ReturnValue;
            }

            else if (World == "RAY2" && Level == "RAY4")
            {
                ReturnValue[0] = new long[] { 13763444 };
                ReturnValue[1] = new long[] { 634589 };
                return ReturnValue;
            }

            else if (World == "RAY3" && Level == "RAY1")
            {
                ReturnValue[0] = new long[] { 17876503 };
                ReturnValue[1] = new long[] { 1569546 };
                return ReturnValue;
            }

            else if (World == "RAY3" && Level == "RAY5")
            {
                ReturnValue[0] = new long[] { 17876503 };
                ReturnValue[1] = new long[] { 1569546 };
                return ReturnValue;
            }

            else if (World == "RAY3" && Level == "RAY9")
            {
                ReturnValue[0] = new long[] { 17876503 };
                ReturnValue[1] = new long[] { 1569546 };
                return ReturnValue;
            }

            else if (World == "RAY4" && Level == "RAY1")
            {
                ReturnValue[0] = new long[] { 19630414 };
                ReturnValue[1] = new long[] { 759848 };
                return ReturnValue;
            }

            else if (World == "RAY4" && Level == "RAY9")
            {
                ReturnValue[0] = new long[] { 19630414 };
                ReturnValue[1] = new long[] { 759848 };
                return ReturnValue;
            }

            else if (World == "RAY5" && Level == "RAY6")
            {
                ReturnValue[0] = new long[] { 14398033 };
                ReturnValue[1] = new long[] { 1816842 };
                return ReturnValue;
            }

            else if (World == "RAY6" && Level == "RAY2")
            {
                ReturnValue[0] = new long[] { 12123953 };
                ReturnValue[1] = new long[] { 1639491 };
                return ReturnValue;
            }
            else
                return null;
        }

        public static long[][] GetPosBGM(string Level, string World, short XAxis, short YAxis)
        {
            if (World == "RAY1" && Level == "RAY6" && YAxis < 830)
            {
                ReturnValue[0] = new long[] { 22786573, 20390262 };
                ReturnValue[1] = new long[] { 656206, 2396311 };
                return ReturnValue;
            }
            else if (World == "RAY1" && Level == "RAY6" && YAxis > 830)
            {
                ReturnValue[0] = new long[] { 25130666, 23442779 };
                ReturnValue[1] = new long[] { 1379926, 1687887 };
                return ReturnValue;
            }

            else if (World == "RAY1" && Level == "RAY7" && (XAxis< 4850 || XAxis > 9250))
            {
                ReturnValue[0] = new long[] { 35162640, 32942870 };
                ReturnValue[1] = new long[] { 469006, 2219770 };
                return ReturnValue;
            }
            else if (World == "RAY1" && Level == "RAY7" && XAxis > 4850)
            {
                ReturnValue[0] = new long[] { 35631646 };
                ReturnValue[1] = new long[] { 942193 };
                return ReturnValue;
            }

            else if (World == "RAY1" && Level == "RAY9" && YAxis< 2650)
            {
                ReturnValue[0] = new long[] { 45334864, 43185671 };
                ReturnValue[1] = new long[] { 993058, 2149193 };
                return ReturnValue;
            }
            else if (World == "RAY1" && Level == "RAY9" && YAxis > 2650)
            {
                ReturnValue[0] = new long[] { 22786573, 20390262 };
                ReturnValue[1] = new long[] { 656206, 2396311 };
                return ReturnValue;
            }

            else if (World == "RAY3" && Level == "RAY2" && ((1930 < XAxis && XAxis< 4525) || (5670 < XAxis && XAxis< 6670)))
            {
                ReturnValue[0] = new long[] { 19446049 };
                ReturnValue[1] = new long[] { 184365 };
                return ReturnValue;
            }
            else
                return null;
        }
    }
}