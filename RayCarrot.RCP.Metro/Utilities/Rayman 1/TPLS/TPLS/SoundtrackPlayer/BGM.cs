using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public class BGM : SoundtrackPlayer
    {
        public BGM(TPLSDataViewModel data) : base(data)
        {
        }

        protected override long[][] Soundtrack => SoundtrackList.GetSoundtrack(Data.Level, Data.World);
        
        protected override async Task PlayAsync()
        {
            if (Soundtrack != null)
                await Player.PlayLoopAsync(Data.MusicFile, Soundtrack[0], Soundtrack[1]);
        }
    }
}