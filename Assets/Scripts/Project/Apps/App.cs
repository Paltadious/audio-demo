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

            // Here all app systems are being initialized - UI, audio, input, push notifications, ads, IAPs, socials,
            // cloud saves, different plugins, etc. After that start menu ("new game", "load game", etc.) is being shown
            // or the game is launched directly if needed.  
            Chain.CreateAndRun(
                new ActionStep(Audio.Instance.Initialize),
                new AudioStep(SoundType.MainMusicTheme),
                new ActionStep(Game.Initialize)
            );
        }
    }
}