#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR_UniversSave : BinarySerializable
{
    public int CAM_SensHoriz { get; set; }
    public int MENU_i_ShowInterface { get; set; }
    public int MG_story_jour { get; set; }
    public int world_ID { get; set; }
    public int CAM_Rotation { get; set; }
    public int[] PROG_ai_XtraUnlocked__dimensions { get; set; }
    public int[] PROG_ai_XtraUnlocked { get; set; }
    public int SND_SKMM_mi_CurrentLine { get; set; }
    public int CAM_SensVert { get; set; }
    public int MENU_i_ShowInventory { get; set; }
    public int[] PROG_Score__dimensions { get; set; }
    public float[] PROG_Score { get; set; }
    public float PROG_GameCumul_JackKll { get; set; }
    public int[] MG_record_pn__dimensions { get; set; }
    public int[] MG_record_pn { get; set; }
    public int[] MG_player_shapes__dimensions { get; set; }
    public int[] MG_player_shapes { get; set; }
    public int MG_Reward_Costume { get; set; }
    public int PROG_i_Map { get; set; }
    public int world_entrance_ID { get; set; }
    public int SND_SKMM_mi_CurrentActiveTrig { get; set; }
    public int MENU_i_ProgressiveMode { get; set; }
    public int SND_gi_ShowSubtitle { get; set; }
    public int MG_Reward_Score { get; set; }
    public int MG_Reward_Zic { get; set; }
    public int PROG_i_MapCur { get; set; }
    public float PROG_GameCumul_KongKill { get; set; }
    public float PROG_GameCumul_KongTime { get; set; }
    public int VID_gi_InvertHoriz { get; set; }
    public int VID_gi_ModeOldMovie { get; set; }
    public float PROG_GameCumul_JackBullet { get; set; }
    public int[] MG_record__dimensions { get; set; }
    public float[] MG_record { get; set; }
    public int[] Vector_speciaux_sauve__dimensions { get; set; }
    public Jade_Vector[] Vector_speciaux_sauve { get; set; }
    public int MG_IDsimultane { get; set; }
    public int[] Flag_speciaux_sauve__dimensions { get; set; }
    public int[] Flag_speciaux_sauve { get; set; }
    public float PROG_GameCumul_JackMort { get; set; }
    public int[] MG_WardRobeShape__dimensions { get; set; }
    public int[] MG_WardRobeShape { get; set; }
    public int[] Float_speciaux_sauve__dimensions { get; set; }
    public float[] Float_speciaux_sauve { get; set; }
    public int CAM_Viseur { get; set; }
    public int MENU_i_ShowLifeBar { get; set; }
    public int MENU_i_ShowAmmoLeft { get; set; }
    public int PROG_i_ES { get; set; }
    public float PROG_GameCumul_KongMort { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        CAM_SensHoriz = s.Serialize<int>(CAM_SensHoriz, name: nameof(CAM_SensHoriz));
        MENU_i_ShowInterface = s.Serialize<int>(MENU_i_ShowInterface, name: nameof(MENU_i_ShowInterface));
        MG_story_jour = s.Serialize<int>(MG_story_jour, name: nameof(MG_story_jour));
        world_ID = s.Serialize<int>(world_ID, name: nameof(world_ID));
        CAM_Rotation = s.Serialize<int>(CAM_Rotation, name: nameof(CAM_Rotation));
        PROG_ai_XtraUnlocked__dimensions = s.SerializeArray<int>(PROG_ai_XtraUnlocked__dimensions, 1, name: nameof(PROG_ai_XtraUnlocked__dimensions));
        PROG_ai_XtraUnlocked = s.SerializeArray<int>(PROG_ai_XtraUnlocked, 32, name: nameof(PROG_ai_XtraUnlocked));
        SND_SKMM_mi_CurrentLine = s.Serialize<int>(SND_SKMM_mi_CurrentLine, name: nameof(SND_SKMM_mi_CurrentLine));
        CAM_SensVert = s.Serialize<int>(CAM_SensVert, name: nameof(CAM_SensVert));
        MENU_i_ShowInventory = s.Serialize<int>(MENU_i_ShowInventory, name: nameof(MENU_i_ShowInventory));
        PROG_Score__dimensions = s.SerializeArray<int>(PROG_Score__dimensions, 2, name: nameof(PROG_Score__dimensions));
        PROG_Score = s.SerializeArray<float>(PROG_Score, 500, name: nameof(PROG_Score));
        PROG_GameCumul_JackKll = s.Serialize<float>(PROG_GameCumul_JackKll, name: nameof(PROG_GameCumul_JackKll));
        MG_record_pn__dimensions = s.SerializeArray<int>(MG_record_pn__dimensions, 3, name: nameof(MG_record_pn__dimensions));
        MG_record_pn = s.SerializeArray<int>(MG_record_pn, 1350, name: nameof(MG_record_pn));
        MG_player_shapes__dimensions = s.SerializeArray<int>(MG_player_shapes__dimensions, 2, name: nameof(MG_player_shapes__dimensions));
        MG_player_shapes = s.SerializeArray<int>(MG_player_shapes, 16, name: nameof(MG_player_shapes));
        MG_Reward_Costume = s.Serialize<int>(MG_Reward_Costume, name: nameof(MG_Reward_Costume));
        PROG_i_Map = s.Serialize<int>(PROG_i_Map, name: nameof(PROG_i_Map));
        world_entrance_ID = s.Serialize<int>(world_entrance_ID, name: nameof(world_entrance_ID));
        SND_SKMM_mi_CurrentActiveTrig = s.Serialize<int>(SND_SKMM_mi_CurrentActiveTrig, name: nameof(SND_SKMM_mi_CurrentActiveTrig));
        MENU_i_ProgressiveMode = s.Serialize<int>(MENU_i_ProgressiveMode, name: nameof(MENU_i_ProgressiveMode));
        SND_gi_ShowSubtitle = s.Serialize<int>(SND_gi_ShowSubtitle, name: nameof(SND_gi_ShowSubtitle));
        MG_Reward_Score = s.Serialize<int>(MG_Reward_Score, name: nameof(MG_Reward_Score));
        MG_Reward_Zic = s.Serialize<int>(MG_Reward_Zic, name: nameof(MG_Reward_Zic));
        PROG_i_MapCur = s.Serialize<int>(PROG_i_MapCur, name: nameof(PROG_i_MapCur));
        PROG_GameCumul_KongKill = s.Serialize<float>(PROG_GameCumul_KongKill, name: nameof(PROG_GameCumul_KongKill));
        PROG_GameCumul_KongTime = s.Serialize<float>(PROG_GameCumul_KongTime, name: nameof(PROG_GameCumul_KongTime));
        VID_gi_InvertHoriz = s.Serialize<int>(VID_gi_InvertHoriz, name: nameof(VID_gi_InvertHoriz));
        VID_gi_ModeOldMovie = s.Serialize<int>(VID_gi_ModeOldMovie, name: nameof(VID_gi_ModeOldMovie));
        PROG_GameCumul_JackBullet = s.Serialize<float>(PROG_GameCumul_JackBullet, name: nameof(PROG_GameCumul_JackBullet));
        MG_record__dimensions = s.SerializeArray<int>(MG_record__dimensions, 2, name: nameof(MG_record__dimensions));
        MG_record = s.SerializeArray<float>(MG_record, 450, name: nameof(MG_record));
        Vector_speciaux_sauve__dimensions = s.SerializeArray<int>(Vector_speciaux_sauve__dimensions, 1, name: nameof(Vector_speciaux_sauve__dimensions));
        Vector_speciaux_sauve = s.SerializeObjectArray<Jade_Vector>(Vector_speciaux_sauve, 200, name: nameof(Vector_speciaux_sauve));
        MG_IDsimultane = s.Serialize<int>(MG_IDsimultane, name: nameof(MG_IDsimultane));
        Flag_speciaux_sauve__dimensions = s.SerializeArray<int>(Flag_speciaux_sauve__dimensions, 1, name: nameof(Flag_speciaux_sauve__dimensions));
        Flag_speciaux_sauve = s.SerializeArray<int>(Flag_speciaux_sauve, 200, name: nameof(Flag_speciaux_sauve));
        PROG_GameCumul_JackMort = s.Serialize<float>(PROG_GameCumul_JackMort, name: nameof(PROG_GameCumul_JackMort));
        MG_WardRobeShape__dimensions = s.SerializeArray<int>(MG_WardRobeShape__dimensions, 1, name: nameof(MG_WardRobeShape__dimensions));
        MG_WardRobeShape = s.SerializeArray<int>(MG_WardRobeShape, 4, name: nameof(MG_WardRobeShape));
        Float_speciaux_sauve__dimensions = s.SerializeArray<int>(Float_speciaux_sauve__dimensions, 1, name: nameof(Float_speciaux_sauve__dimensions));
        Float_speciaux_sauve = s.SerializeArray<float>(Float_speciaux_sauve, 200, name: nameof(Float_speciaux_sauve));
        CAM_Viseur = s.Serialize<int>(CAM_Viseur, name: nameof(CAM_Viseur));
        MENU_i_ShowLifeBar = s.Serialize<int>(MENU_i_ShowLifeBar, name: nameof(MENU_i_ShowLifeBar));
        MENU_i_ShowAmmoLeft = s.Serialize<int>(MENU_i_ShowAmmoLeft, name: nameof(MENU_i_ShowAmmoLeft));
        PROG_i_ES = s.Serialize<int>(PROG_i_ES, name: nameof(PROG_i_ES));
        PROG_GameCumul_KongMort = s.Serialize<float>(PROG_GameCumul_KongMort, name: nameof(PROG_GameCumul_KongMort));
    }

    public class Jade_Vector : BinarySerializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            X = s.Serialize<float>(X, name: nameof(X));
            Y = s.Serialize<float>(Y, name: nameof(Y));
            Z = s.Serialize<float>(Z, name: nameof(Z));
        }
    }
}