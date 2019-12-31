using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public class Midi : SoundtrackPlayer
    {
        public Midi(TPLSDataViewModel data) : base(data)
        {
        }

        protected override long[][] Soundtrack => SoundtrackList.GetMidi(Data.Level, Data.World);
        
        protected override async Task PlayAsync()
        {
            if (Soundtrack != null)
                await Player.PlayLoopAsync(Data.MusicFile, Soundtrack[0], Soundtrack[1]);
        }
    }
}