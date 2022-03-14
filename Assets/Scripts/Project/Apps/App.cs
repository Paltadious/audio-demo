using Core.Audios;
using Core.Chains;
using Project.Audios;
using Project.Games;

namespace Project.Apps
{
    /// <summary>
    /// Class for initialization and access to core, app-level systems.
    /// </summary>
    public class App
    {
        public static bool isInited;
        
        public static void Initialize()
        {
            if (isInited)
                return;

            Chain.CreateAndRun(
                new ActionStep(Audio.Instance.Initialize),
                new AudioStep(SoundType.MainMusicTheme),
                new ActionStep(Game.Initialize)
                );
        }
    }
}