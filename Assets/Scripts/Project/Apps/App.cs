using Core.Chains;
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
                new ActionStep(Game.Initialize)
                );
        }
    }
}