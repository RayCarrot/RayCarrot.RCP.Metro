using NAudio.Wave;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public class PositionedBGM : SoundtrackPlayer
    {
        public PositionedBGM(TPLSDataModel data) : base(data)
        {
        }

        protected override async Task PlayAsync()
        {
            if (Soundtrack != null)
            {
                await Player.PlayLoopAsync(Data.MusicFile, Soundtrack[0], Soundtrack[1]);
                CurSoundtrack = Soundtrack[0][0];
            } 
        }

        public async Task AxisChangeAsync()
        {
            if (Soundtrack == null)
                return;

            if (Player.PlaybackState == PlaybackState.Playing && CurSoundtrack != Soundtrack[0][0])
            {
                Player.Stop();
                await PlayAsync();
            }
        }

        public long CurSoundtrack { get; set; }

        protected override long[][] Soundtrack => SoundtrackList.GetPosBGM(Data.Level, Data.World, Data.XAxis, Data.YAxis);

    }
}
