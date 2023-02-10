using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRavingRabbids : GameProgressionManager
{
    public GameProgressionManager_RaymanRavingRabbids(GameInstallation gameInstallation, string backupId) 
        : base(gameInstallation, backupId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static float[] GetMinigameMaxScores
    {
        get
        {
            float[] MG_score_max = new float[150];

            MG_score_max[0] = 28000;
            MG_score_max[1] = 36000;
            MG_score_max[2] = 34000;
            MG_score_max[3] = 30000;
            MG_score_max[4] = 28000;
            MG_score_max[5] = 28000;
            MG_score_max[6] = 27000;
            MG_score_max[7] = 308; // Note: This is actually the max score for MG 17. The actual max score for MG 7 is supposed to be 28000.0
            MG_score_max[8] = 33000;
            MG_score_max[9] = 38000;
            MG_score_max[10] = 210;
            MG_score_max[11] = 395;
            MG_score_max[12] = 255;
            MG_score_max[13] = 150;
            MG_score_max[14] = 235;
            MG_score_max[15] = 155;
            MG_score_max[16] = 285;
            MG_score_max[17] = 312;  // - (equal to record 0 for MG 17, so: MG_score_max[17] = 312.0)
            MG_score_max[18] = 295;
            MG_score_max[19] = 340;
            MG_score_max[20] = 20000;
            MG_score_max[21] = 22;
            MG_score_max[22] = 145;
            MG_score_max[23] = 80;
            MG_score_max[24] = 7;
            MG_score_max[25] = 130;
            MG_score_max[26] = 0.1f;
            MG_score_max[27] = 10;
            MG_score_max[28] = 55;
            MG_score_max[29] = 85;
            MG_score_max[30] = 170;
            MG_score_max[31] = 32;
            MG_score_max[32] = 10200;
            MG_score_max[33] = 470;
            MG_score_max[34] = 3000;
            MG_score_max[35] = 3100;
            MG_score_max[36] = 20.5f;
            MG_score_max[37] = 3178;
            MG_score_max[38] = 800;
            MG_score_max[39] = 4088;
            MG_score_max[40] = 88;
            MG_score_max[41] = 90;
            MG_score_max[42] = 55;
            MG_score_max[43] = 30;
            MG_score_max[44] = 1868;
            MG_score_max[45] = 40;
            MG_score_max[46] = 400;
            MG_score_max[47] = 1000;
            MG_score_max[48] = 4000;
            MG_score_max[49] = 60;
            MG_score_max[50] = 10;
            MG_score_max[51] = 43;
            MG_score_max[52] = 11;
            MG_score_max[53] = 3000;
            MG_score_max[54] = 30;
            MG_score_max[55] = 85;
            MG_score_max[56] = 275;
            MG_score_max[57] = 3000;
            MG_score_max[58] = 1300;
            MG_score_max[59] = 21;
            MG_score_max[60] = 23;
            MG_score_max[61] = 600;
            MG_score_max[62] = 400;
            MG_score_max[63] = 1000;
            MG_score_max[64] = 75000;
            MG_score_max[65] = 25000;
            MG_score_max[66] = 20;
            MG_score_max[67] = 14;
            MG_score_max[68] = 3000;
            MG_score_max[69] = 21;
            MG_score_max[70] = 5600;
            MG_score_max[71] = 3000;
            MG_score_max[72] = 40;
            MG_score_max[73] = 170;
            MG_score_max[74] = 800;
            MG_score_max[75] = 0.15000001f;
            MG_score_max[76] = 7600;
            MG_score_max[77] = 30;
            MG_score_max[78] = 2088;
            MG_score_max[79] = 2668;
            MG_score_max[80] = 3198;
            MG_score_max[81] = 2358;
            MG_score_max[82] = 2478;
            MG_score_max[83] = 2658;
            MG_score_max[84] = 2528;
            MG_score_max[85] = 2908;
            MG_score_max[86] = 2288;
            MG_score_max[87] = 3538;
            MG_score_max[88] = 2568;
            MG_score_max[89] = 2928;
            MG_score_max[90] = 155;
            MG_score_max[91] = 395;
            MG_score_max[92] = 257;
            MG_score_max[93] = 155;
            MG_score_max[94] = 207;
            MG_score_max[95] = 157;
            MG_score_max[96] = 287;
            MG_score_max[97] = 313;
            MG_score_max[98] = 297;
            MG_score_max[99] = 313;
            MG_score_max[100] = 28000;
            MG_score_max[101] = 36000;
            MG_score_max[102] = 34000;
            MG_score_max[103] = 30000;
            MG_score_max[104] = 28000;
            MG_score_max[105] = 28000;
            MG_score_max[106] = 27000;
            MG_score_max[107] = 28000;
            MG_score_max[108] = 33000;
            MG_score_max[109] = 38000;
            MG_score_max[110] = 22000;
            MG_score_max[111] = 30000;
            MG_score_max[112] = 28000;
            MG_score_max[113] = 24000;
            MG_score_max[114] = 22000;
            MG_score_max[115] = 22000;
            MG_score_max[116] = 12000;
            MG_score_max[117] = 22000;
            MG_score_max[118] = 24000;
            MG_score_max[119] = 32000;
            MG_score_max[120] = 3000;
            MG_score_max[121] = 5000;
            MG_score_max[122] = 5000;
            MG_score_max[123] = 5000;
            MG_score_max[124] = 5000;
            MG_score_max[125] = 10000;
            MG_score_max[126] = 10000;
            MG_score_max[127] = 10000;

            MG_score_max[140] = 103;

            // If PC
            MG_score_max[21] = 24;
            MG_score_max[23] = 80;
            MG_score_max[24] = 10;
            MG_score_max[29] = 85;
            MG_score_max[30] = 170;
            MG_score_max[47] = 1000;
            MG_score_max[49] = 60;
            MG_score_max[56] = 275;
            MG_score_max[63] = 1000;
            MG_score_max[72] = 39.5f;
            MG_score_max[73] = 170;
            MG_score_max[76] = 7600;

            return MG_score_max;
        }
    }

    private static int[] GetMinigameTypes
    {
        get
        {
            int[] MG_objectif_type = new int[150];

            MG_objectif_type[0] = 32786;
            MG_objectif_type[1] = 32786;
            MG_objectif_type[2] = 32786;
            MG_objectif_type[3] = 32786;
            MG_objectif_type[4] = 32786;
            MG_objectif_type[5] = 32786;
            MG_objectif_type[6] = 32786;
            MG_objectif_type[7] = 32786;
            MG_objectif_type[8] = 32786;
            MG_objectif_type[9] = 32786;
            MG_objectif_type[10] = 17;
            MG_objectif_type[11] = 17;
            MG_objectif_type[12] = 17;
            MG_objectif_type[13] = 17;
            MG_objectif_type[14] = 17;
            MG_objectif_type[15] = 17;
            MG_objectif_type[16] = 17;
            MG_objectif_type[17] = 17;
            MG_objectif_type[18] = 17;
            MG_objectif_type[19] = 17;
            MG_objectif_type[20] = 32770;
            MG_objectif_type[21] = 32769;
            MG_objectif_type[22] = 32772;
            MG_objectif_type[23] = 32769;
            MG_objectif_type[24] = 9;
            MG_objectif_type[25] = 32769;
            MG_objectif_type[26] = 12;
            MG_objectif_type[27] = 9;
            MG_objectif_type[28] = 32769;
            MG_objectif_type[29] = 32769;
            MG_objectif_type[30] = 32772;
            MG_objectif_type[31] = 1;
            MG_objectif_type[32] = 32770;
            MG_objectif_type[33] = 32770;
            MG_objectif_type[34] = 33026;
            MG_objectif_type[35] = 32770;
            MG_objectif_type[36] = 1;
            MG_objectif_type[37] = 32906;
            MG_objectif_type[38] = 32770;
            MG_objectif_type[39] = 32906;
            MG_objectif_type[40] = 65;
            MG_objectif_type[41] = 65;
            MG_objectif_type[42] = 65;
            MG_objectif_type[43] = 65;
            MG_objectif_type[44] = 32906;
            MG_objectif_type[45] = 32777;
            MG_objectif_type[46] = 32778;
            MG_objectif_type[47] = 32770;
            MG_objectif_type[48] = 32770;
            MG_objectif_type[49] = 32769;
            MG_objectif_type[50] = 9;
            MG_objectif_type[51] = 65;
            MG_objectif_type[52] = 9;
            MG_objectif_type[53] = 33026;
            MG_objectif_type[54] = 32769;
            MG_objectif_type[55] = 32769;
            MG_objectif_type[56] = 32772;
            MG_objectif_type[57] = 33026;
            MG_objectif_type[58] = 32778;
            MG_objectif_type[59] = 1;
            MG_objectif_type[60] = 1;
            MG_objectif_type[61] = 32770;
            MG_objectif_type[62] = 32778;
            MG_objectif_type[63] = 32770;
            MG_objectif_type[64] = 32770;
            MG_objectif_type[65] = 32778;
            MG_objectif_type[66] = 1;
            MG_objectif_type[67] = 9;
            MG_objectif_type[68] = 33026;
            MG_objectif_type[69] = 1;
            MG_objectif_type[70] = 32770;
            MG_objectif_type[71] = 33026;
            MG_objectif_type[72] = 9;
            MG_objectif_type[73] = 32780;
            MG_objectif_type[74] = 32778;
            MG_objectif_type[75] = 12;
            MG_objectif_type[76] = 32778;
            MG_objectif_type[77] = 1;
            MG_objectif_type[78] = 32906;
            MG_objectif_type[79] = 32906;
            MG_objectif_type[80] = 32906;
            MG_objectif_type[81] = 32906;
            MG_objectif_type[82] = 32906;
            MG_objectif_type[83] = 32906;
            MG_objectif_type[84] = 32906;
            MG_objectif_type[85] = 32906;
            MG_objectif_type[86] = 32906;
            MG_objectif_type[87] = 32906;
            MG_objectif_type[88] = 32906;
            MG_objectif_type[89] = 32906;
            MG_objectif_type[90] = 25;
            MG_objectif_type[91] = 25;
            MG_objectif_type[92] = 25;
            MG_objectif_type[93] = 25;
            MG_objectif_type[94] = 25;
            MG_objectif_type[95] = 25;
            MG_objectif_type[96] = 25;
            MG_objectif_type[97] = 25;
            MG_objectif_type[98] = 25;
            MG_objectif_type[99] = 25;
            MG_objectif_type[100] = 32794;
            MG_objectif_type[101] = 32794;
            MG_objectif_type[102] = 32794;
            MG_objectif_type[103] = 32794;
            MG_objectif_type[104] = 32794;
            MG_objectif_type[105] = 32794;
            MG_objectif_type[106] = 32794;
            MG_objectif_type[107] = 32794;
            MG_objectif_type[108] = 32794;
            MG_objectif_type[109] = 32794;
            MG_objectif_type[110] = 32786;
            MG_objectif_type[111] = 32786;
            MG_objectif_type[112] = 32786;
            MG_objectif_type[113] = 32786;
            MG_objectif_type[114] = 32786;
            MG_objectif_type[115] = 32786;
            MG_objectif_type[116] = 32786;
            MG_objectif_type[117] = 32786;
            MG_objectif_type[118] = 32786;
            MG_objectif_type[119] = 32786;
            MG_objectif_type[120] = 33026;
            MG_objectif_type[121] = 33026;
            MG_objectif_type[122] = 33026;
            MG_objectif_type[123] = 33026;
            MG_objectif_type[124] = 33026;
            MG_objectif_type[125] = 33026;
            MG_objectif_type[126] = 33026;
            MG_objectif_type[127] = 33026;

            MG_objectif_type[130] = 32794;
            MG_objectif_type[131] = 32794;
            MG_objectif_type[132] = 32794;
            MG_objectif_type[133] = 32794;
            MG_objectif_type[134] = 32794;
            MG_objectif_type[135] = 32794;
            MG_objectif_type[136] = 32794;
            MG_objectif_type[137] = 32794;
            MG_objectif_type[138] = 32794;
            MG_objectif_type[139] = 32794;
            MG_objectif_type[140] = 32778;

            return MG_objectif_type;
        }
    }

    // Indexing here is different than other arrays
    private static World[] GetWorlds => new World[]
    {
        // 0
        new World(1, 0x3D00C452, 0, "_main_fix", 0, -1, -1, -1),
        // 1
        new World(2, 0x3D00C456, 0, "_main_logo", 0, -1, -1, -1),
        // 2
        new World(3, 0x47000B84, 0, "_main_menu", 0, -1, -1, -1),
        // 3
        new World(4, 0x4902D989, 0, "_main_E3endscreen", 0, -1, -1, -1),
        // 4
        new World(5, 0x4902DE8F, 0, "_main_pad", 0, -1, -1, -1),
        // 5
        new World(6, 0x4902EA59, 0, "_main_bootup", 0, -1, -1, -1),
        // 6
        new World(7, 0x3D0129BE, 0, "_main_credits", 0, -1, -1, -1),
        // 7
        new World(601, 0x1B0053C5, 0, "Pulp_Fiction_Easy", 1, 44, 0, 4),
        // 8
        new World(602, 0x1B005AFD, 0, "Chic_Good_Times_Easy", 1, 79, 1, 4),
        // 9
        new World(603, 0x1B005B14, 0, "Cindy_Lauper_Easy", 1, 78, 2, 4),
        // 10
        new World(604, 0x1B005B16, 0, "Hip_Hip_Hooray_Easy", 1, 80, 3, 4),
        // 11
        new World(605, 0x1B005B18, 0, "La_Bamba_Easy", 1, 81, 4, 4),
        // 12
        new World(606, 0x1B005B1A, 0, "Rock_n_Roll_Easy", 1, 82, 5, 4),
        // 13
        new World(607, 0x1B005B1C, 0, "Dance_07_Easy", 1, 83, 6, 4),
        // 14
        new World(608, 0x1B005A85, 0, "Pulp_Fiction_Hard", 1, 84, 7, 4),
        // 15
        new World(609, 0x1B005B13, 0, "Chic_Good_Times_Hard", 1, 85, 8, 4),
        // 16
        new World(610, 0x1B005B15, 0, "Cindy_Lauper_Hard", 1, 86, 9, 4),
        // 17
        new World(611, 0x1B005B17, 0, "Hip_Hop_Hooray_Hard", 1, 87, 10, 4),
        // 18
        new World(612, 0x1B005B19, 0, "La_Bamba_Hard", 1, 88, 11, 4),
        // 19
        new World(613, 0x1B005B1B, 0, "Rock_n_roll_Hard", 1, 89, 12, 4),
        // 20
        new World(614, 0x1B005B1D, 0, "Dance_14_Hard", 1, 39, 13, 4),
        // 21
        new World(615, 0x1B005B1E, 0, "Pulp_Fiction_Hardcore", 1, 37, 14, 4),
        // 22
        new World(701, 0x1B005F6A, 0, "PulpFiction_Easy_MULTI", 8, 44, -1, -1),
        // 23
        new World(702, 0x1B005F7F, 0, "ChicGoodTimes_Easy_MULTI", 8, 79, -1, -1),
        // 24
        new World(703, 0x1B005F87, 0, "CindyLauper_Easy_MULTI", 8, 78, -1, -1),
        // 25
        new World(704, 0x1B005F8F, 0, "HipHopHooray_Easy_MULTI", 8, 80, -1, -1),
        // 26
        new World(705, 0x1B005F97, 0, "LaBamba_Easy_MULTI", 8, 81, -1, -1),
        // 27
        new World(706, 0x1B005F9F, 0, "Rock_n_roll_Easy_MULTI", 8, 82, -1, -1),
        // 28
        new World(707, 0x1B005FA7, 0, "DANSE07_Easy_MULTI", 8, 83, -1, -1),
        // 29
        new World(708, 0x1B005F7B, 0, "PulpFiction_Hard_MULTI", 8, 84, -1, -1),
        // 30
        new World(709, 0x1B005F83, 0, "ChicGoodTimes_Hard _MULTI", 8, 85, -1, -1),
        // 31
        new World(710, 0x1B005F8B, 0, "CindyLauper_Hard _MULTI", 8, 86, -1, -1),
        // 32
        new World(711, 0x1B005F93, 0, "HipHopHooray_Hard _MULTI", 8, 87, -1, -1),
        // 33
        new World(712, 0x1B005F9B, 0, "LaBamba_Hard _MULTI", 8, 88, -1, -1),
        // 34
        new World(713, 0x1B005FA3, 0, "Rock_n_Roll_Hard _MULTI_", 8, 89, -1, -1),
        // 35
        new World(714, 0x1B005FAB, 0, "Danse_14_Hard_MULTI", 8, 39, -1, -1),
        // 36
        new World(715, 0x1B005FAF, 0, "Pulp_Fiction_Hardcore_MULTI", 8, 37, -1, -1),
        // 37
        new World(400, 0xC2003349, 0, "FPS00_TUTORIAL", 2, 3, 0, 5),
        // 38
        new World(401, 0xD007D0B, 0, "FPS01_BEACH01", 2, 0, 1, 5),
        // 39
        new World(402, 0xC2002C1D, 0, "FPS02_TRAIN_WESTERN", 2, 5, 3, 5),
        // 40
        new World(403, 0x3A00352B, 0, "FPS03_CIMETIERE_MORT", 2, 8, 4, 5),
        // 41
        new World(404, 0x17002D66, 1, "FPS04_CHARIOT_MINE", 2, 6, 6, 5),
        // 42
        new World(405, 0x2900556D, 0, "FPS05_CANYON_DESERT", 2, 4, 7, 5),
        // 43
        new World(406, 0xB013816, 0, "FPS06_BEACH_02", 2, 1, 9, 5),
        // 44
        new World(407, 0xB01261C, 0, "FPS07_VILLE_WESTERN", 2, 2, 10, 5),
        // 45
        new World(408, 0x29006B44, 0, "FPS08_COURSE_MORT", 2, 9, 12, 5),
        // 46
        new World(409, 0xD007E79, 0, "FPS09_DOOM_BASE", 2, 7, 14, 5),
        // 47
        new World(202, 0xC10272B9, 0, "Course Atolls", 3, 21, 0, 1),
        // 48
        new World(203, 0xC10272B5, 0, "Lancer de Vache", 3, 22, 0, 2),
        // 49
        new World(207, 0x1B00566A, 0, "Restau Lapin", 3, 38, 0, 3),
        // 50
        new World(209, 0x2010E83, 0, "Toilettes Zone", 3, 23, 1, 1),
        // 51
        new World(201, 0xC10272B4, 0, "Corde a Sauter", 3, 20, 1, 2),
        // 52
        new World(208, 0x1B0054E7, 0, "Lancer Biere", 3, 25, 1, 3),
        // 53
        new World(210, 0x1B0054EF, 0, "Chariot Shadok", 3, 30, 2, 1),
        // 54
        new World(211, 0xC10274D3, 0, "Tirage de vers", 3, 29, 2, 2),
        // 55
        new World(216, 0xDF003C8D, 0, "Equilibre", 3, 36, 2, 3),
        // 56
        new World(204, 0x2010F0B, 0, "Traite des Vaches", 3, 24, 3, 1),
        // 57
        new World(218, 0x3A0031E4, 0, "Bat dans Atolls", 3, 28, 3, 2),
        // 58
        new World(223, 0x9E00E9A7, 0, "Cerveaux Lapin", 3, 67, 3, 3),
        // 59
        new World(205, 0x1B0056F8, 0, "Simon", 3, 46, 4, 1),
        // 60
        new World(206, 0x1B005701, 0, "Tape Taupe", 3, 47, 4, 2),
        // 61
        new World(226, 0x1B0058B2, 0, "123_Soleil", 3, 72, 4, 3),
        // 62
        new World(212, 0x52006BB4, 0, "Medailles-Demineur", 3, 27, 5, 1),
        // 63
        new World(236, 0xDD001721, 0, "Lapin Marteau", 3, 73, 5, 2),
        // 64
        new World(227, 0xDD001633, 0, "LapinPunaise", 3, 74, 5, 3),
        // 65
        new World(220, 0x1B0058BB, 0, "Lapin foot", 3, 32, 6, 1),
        // 66
        new World(224, 0x1B00570A, 0, "Lapin Maillard", 3, 64, 6, 2),
        // 67
        new World(809, 0x52007498, 0, "Equilibre Variante 1", 4, 59, 6, 3),
        // 68
        new World(802, 0xDE00140C, 0, "Toilette Zone avec Piétons", 4, 49, 7, 1),
        // 69
        new World(225, 0x7200B413, 0, "Curling", 3, 75, 7, 2),
        // 70
        new World(222, 0x1B00571C, 0, "Fight Arene", 3, 66, 7, 3),
        // 71
        new World(215, 0xDE001469, 0, "Aspire a Laine", 3, 58, 8, 1),
        // 72
        new World(214, 0x1B005713, 0, "Lancer couteaux", 3, 65, 8, 2),
        // 73
        new World(221, 0x2600D797, 0, "Paparazzi", 3, 35, 8, 3),
        // 74
        new World(228, 0x1B00584D, 0, "Encerclement", 3, 76, 9, 1),
        // 75
        new World(808, 0x1B005C12, 0, "Traite des vaches Variante", 4, 50, 9, 2),
        // 76
        new World(811, 0xDD00179B, 0, "Demineur Variante", 4, 52, 9, 3),
        // 77
        new World(217, 0x9E00E8F6, 0, "Reve Lapin (Cubes)", 3, 45, 10, 1),
        // 78
        new World(807, 0xDE0014C7, 0, "Tape_taupes_Variante", 4, 63, 10, 2),
        // 79
        new World(803, 0x9E00E970, 0, "Ride Mini Bat - Variante1", 4, 54, 10, 3),
        // 80
        new World(219, 0x1B0054FB, 0, "Bowling", 3, 33, 11, 1),
        // 81
        new World(813, 0x1B005EEB, 0, "Curling Variante 2", 4, 26, 11, 2),
        // 82
        new World(239, 0xDE001529, 0, "Grappin Soucoupe", 3, 77, 11, 3),
        // 83
        new World(810, 0x520074AD, 0, "Equilibre Variante 2", 4, 60, 12, 1),
        // 84
        new World(805, 0x9E00E964, 0, "Corde a Sauter - Variante1", 4, 48, 12, 2),
        // 85
        new World(812, 0xDD0017B4, 0, "Simon Variante 2", 4, 62, 12, 3),
        // 86
        new World(238, 0xD9019B8D, 0, "Mastermind", 3, 70, 13, 1),
        // 87
        new World(801, 0x2600D849, 0, "Tir les Vers - Variante1", 4, 55, 13, 2),
        // 88
        new World(804, 0x9E00E93F, 0, "Chariot Shadok - Variante1", 4, 56, 13, 3),
        // 89
        new World(505, 0x1B0054F3, 0, "Chute Libre", 5, 31, 14, 1),
        // 90
        new World(814, 0x1B005EF8, 0, "Fight Arene Variante", 4, 69, 14, 2),
        // 91
        new World(815, 0x1B005A31, 0, "Resto Lapin Variante", 4, 61, 14, 3),
        // 92
        new World(237, 0xDD0017F9, 0, "Duel", 3, -1, -1, -1),
        // 93
        new World(501, 0xDB000ABB, 0, "La Course des Morts", 5, 40, 5, 5),
        // 94
        new World(502, 0xC1027747, 0, "La Course des Morts Reverse", 5, 41, 11, 5),
        // 95
        new World(503, 0xC1027523, 0, "Phaco Atoll", 5, 42, 2, 5),
        // 96
        new World(504, 0xC10277B1, 0, "Phaco Western", 5, 51, 13, 5),
        // 97
        new World(213, 0x5200779C, 0, "Chute Libre variante", 3, 43, 8, 5),
        // 98
        new World(100, 0xDB001435, 0, "Cachot", 6, -1, -1, -1),
        // 99
        new World(101, 0x2011497, 0, "Arene", 6, -1, -1, -1),
        // 100
        new World(303, 0x3501767B, 0, "Tests cine", 7, -1, -1, -1),
        // 101
        new World(305, 0x8F014095, 0, "Test Mapping", 7, -1, -1, -1),
        // 102
        new World(306, 738202144, 0, "Test Synchro Son", 7, -1, -1, -1),
        // 103
        new World(307, 0x1B005673, 0, "Test Tri Selectif", 7, -1, -1, -1),
        // 104
        new World(308, 0x1B005682, 0, "Test Resto", 7, -1, -1, -1),
        // 105
        new World(309, 0x1B005690, 0, "Test Gladiator", 7, -1, -1, -1),
        // 106
        new World(310, 0x4800E48F, 0, "Test Mire", 7, -1, -1, -1),
        // 107
        new World(311, 0x1B00584D, 0, "Test Encerclement", 7, -1, -1, -1),
        // 108
        new World(312, 0x1B0058BB, 0, "Test Foot", 7, -1, -1, -1),
        // 109
        new World(313, 0x1B0058B2, 0, "Test 1/2/3 soleil", 7, -1, -1, -1),
        // 110
        new World(301, 0x201149B, 1, "Gladiator Lapins Taupes", 7, -1, -1, -1),
        // 111
        new World(301, 0x201149B, 1, "Gladiator Lapins Taupes", 7, -1, -1, -1),
        // 112
        new World(315, 0x87002A3B, 0, "Prefabs Sound", 7, -1, -1, -1),
    };

    private static int ConvertScore(float max, int type, int mgID, float score)
    {
        // Check if the score should be used as is
        if ((type & 0x100) != 0)
            return (int)score;

        float scaledScore;

        // Bunnies like surprises
        if (mgID == 64)
        {
            float v3 = score - 50000;

            if (v3 <= 0)
                v3 = 0;

            scaledScore = v3 / (max - 50000);

            if (scaledScore > 1)
                return 1 * 1000;

            if (scaledScore < 0)
                return 0 * 1000;
        }
        else
        {
            if (score < 0.001)
                return 0 * 1000;

            float v5;

            if ((BitHelpers.ExtractBits(type, 8, 8) & 0x80u) == 0)
                v5 = max / score;
            else
                v5 = score / max;

            scaledScore = v5 switch
            {
                > 1 => 1,
                < 0 => 0,
                _ => v5
            };
        }
        return (int)(scaledScore * 1000);
    }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveFile = fileSystem.GetFile(InstallDir + "Rayman4.sav");

        Logger.Info("{0} save is being loaded...", GameInstallation.FullId);

        using RCPContext context = new(saveFile.Parent);

        RRR_SaveFile? saveData = await context.ReadFileDataAsync<RRR_SaveFile>(saveFile.Name, new RRR_SaveEncoder(), removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("Save has been deserialized");

        World[] worlds = GetWorlds;

        // Add save slots
        for (int saveIndex = 0; saveIndex < saveData.StorySlots.Length; saveIndex++)
        {
            RRR_SaveSlot slot = saveData.StorySlots[saveIndex];

            // Make sure the slot isn't empty
            if (slot.SlotDesc.Time == 0)
                continue;

            int completedMinigames = 0;

            // Enumerate every world used in story mode
            foreach (World w in worlds.Where(x => x.MG_Journey != -1))
            {
                // Get the bit array index
                int index = 2 * w.MG_ID + 5001;

                // Check if it has been completed
                if ((slot.Univers.Flag_speciaux_sauve[index >> 5] & (1 << (index & 0x1F))) != 0)
                    completedMinigames++;
            }

            GameProgressionDataItem[] dataItems =
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.RRR_Plunger, 
                    header: new ResourceLocString(nameof(Resources.Progression_RRRDays)), 
                    value: slot.SlotDesc.Progress_Days, 
                    max: 15),
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.RRR_Trophy, 
                    header: new ResourceLocString(nameof(Resources.Progression_RRRMinigamesCompleted)), 
                    value: completedMinigames, 
                    max: 5 * 15),
            };

            int storySlotIndex = saveIndex;

            yield return new SerializableGameProgressionSlot<RRR_SaveFile>(slot.SlotDesc.Name, saveIndex, slot.SlotDesc.Progress_Percentage, dataItems, context, saveData, saveFile.Name)
            {
                GetExportObject = x => x.StorySlots[storySlotIndex],
                SetImportObject = (x, o) => x.StorySlots[storySlotIndex] = (RRR_SaveSlot)o,
                ExportedType = typeof(RRR_SaveSlot),
            };
        }

        float[] maxScores = GetMinigameMaxScores;
        int[] types = GetMinigameTypes;

        List<GameProgressionDataItem> scoreDataItems = new();

        int totalScore = 0;
        const int maxScore = 183000;

        // Enumerate every minigame
        for (int i = 0; i < 128; i++)
        {
            int mgID = i;

            // Find player top score
            int[] playerScores = Enumerable.Range(0, 3).Where(i => saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + i * 3] > 0).ToArray();

            bool isAscending = (types[mgID] & 0x8000) != 0;
            float defaultScore = isAscending ? 0 : 100000000000000;

            // Skip if there are no high scores in this minigame
            if (playerScores.Length == 0)
            {
                totalScore += ConvertScore(maxScores[mgID], types[mgID], mgID, defaultScore);
                continue;
            }

            int highScoreIndex = isAscending
                ? playerScores.OrderBy(i => saveData.ScoreSlot.Univers.MG_record[mgID * 3 + i]).Last()
                : playerScores.OrderByDescending(i => saveData.ScoreSlot.Univers.MG_record[mgID * 3 + i]).Last();

            float playerHighScore = saveData.ScoreSlot.Univers.MG_record[mgID * 3 + highScoreIndex];

            int score = ConvertScore(maxScores[mgID], types[mgID], mgID, isAscending
                ? Math.Max(defaultScore, playerHighScore)
                : Math.Min(defaultScore, playerHighScore));

            totalScore += score;

            // Maybe correct this?
            static string getChar(int value)
            {
                value = Math.Abs(value);

                value = BitHelpers.ExtractBits(value, 12, 0);

                return ((char)value).ToString();
            }

            string char0 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + highScoreIndex * 3 + 0]);
            string char1 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + highScoreIndex * 3 + 1]);
            string char2 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + highScoreIndex * 3 + 2]);

            string name = char0 + char1 + char2;

            if (score != 0 && (types[mgID] & 0x100) == 0 && mgID != 64)
                scoreDataItems.Add(new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.RRR_Star, 
                    header: new GeneratedLocString(() => String.Format(Resources.Progression_RRRPoints, Resources.ResourceManager.GetString($"RRR_LevelName_{mgID}"))), 
                    value: score, 
                    max: 1000));

            scoreDataItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false, 
                icon: ProgressionIconAsset.RRR_Trophy, 
                header: new GeneratedLocString(() => String.Format(Resources.Progression_RRRScore, Resources.ResourceManager.GetString($"RRR_LevelName_{mgID}"))), 
                text: $"{name}: {playerHighScore}"));
        }

        // Add total score
        scoreDataItems.Insert(0, new GameProgressionDataItem(
            isPrimaryItem: true, 
            icon: ProgressionIconAsset.RRR_Star, 
            header: new ResourceLocString(nameof(Resources.Progression_RRRTotalPoints)), 
            value: totalScore, 
            max: maxScore));

        // Add score slot
        yield return new SerializableGameProgressionSlot<RRR_SaveFile>(new ResourceLocString(nameof(Resources.Progression_RRRScoreSlot)), 3, totalScore, maxScore, scoreDataItems, context, saveData, saveFile.Name)
        {
            GetExportObject = x => x.ScoreSlot,
            SetImportObject = (x, o) => x.ScoreSlot = (RRR_SaveSlot)o,
            ExportedType = typeof(RRR_SaveSlot),
            SlotGroup = 1,
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }

    private class World
    {
        public World(int id, uint key, int entry, string name, int menu, int mgId, int mgJourney, int mgDoor)
        {
            ID = id;
            Key = key;
            Entry = entry;
            Name = name;
            Menu = menu;
            MG_ID = mgId;
            MG_Journey = mgJourney;
            MG_Door = mgDoor;
        }

        public int ID { get; }
        public uint Key { get; }
        public int Entry { get; }
        public string Name { get; }
        public int Menu { get; }
        public int MG_ID { get; }
        public int MG_Journey { get; }
        public int MG_Door { get; }
    }
}