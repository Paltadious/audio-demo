using UnityEngine;

namespace Project.Apps
{
    /// <summary>
    /// Starting point of the app.
    /// </summary>
    /// <remarks>Script has to be called from the starter scene and has to have ScriptExecutionOrder earlier than
    /// anything else.</remarks>
    public class AppInitializer : MonoBehaviour
    {
        void Start()
        {
            App.Initialize();
        }
    }
}