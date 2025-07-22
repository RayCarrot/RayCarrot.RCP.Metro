#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR2_MiniGame : BinarySerializable
{
    public RRR2_ScoreEntry[] Scores { get; set; }

    public bool IsTrophy { get; set; }

    public int UserHighScore { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Scores = s.SerializeObjectArray<RRR2_ScoreEntry>(Scores, 3, name: nameof(Scores));

        if (Scores[0].Score != 12000 || Scores[0].Name != "GLOBOX")
        {
            IsTrophy = true;
            UserHighScore = Scores[0].Score;
        }
        else if (Scores[1].Score != 8000 || Scores[1].Name != "BETILLA")
        {
            IsTrophy = true;
            UserHighScore = Scores[1].Score;
        }
        else if (Scores[2].Score != 4000 || Scores[2].Name != "MURFY")
        {
            IsTrophy = true;
            UserHighScore = Scores[2].Score;
        }
        else
        {
            IsTrophy = false;
            UserHighScore = 0;
        }
    }
}