#if QALAB
using MiawWorks.QALab;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrimsonTactics.QA
{
    /// <summary>
    /// Registers the <c>crimson_tactics</c> bot, and puts a <see cref="QAProbe"/> in every loaded scene
    /// that has a player while a QA Lab run is recording. Nothing here runs in a normal game.
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Register()
        {
            BotAdapterRegistry.Register(CrimsonTacticsAdapter.AdapterName, () => new CrimsonTacticsAdapter(FindGame));
            SceneManager.sceneLoaded += (scene, mode) => AddProbe();
        }

        private static void AddProbe()
        {
            if (!QALab.IsRunning || Object.FindFirstObjectByType<PlayerUnit>() == null) return;
            if (Object.FindFirstObjectByType<QAProbe>() != null) return;
            new GameObject("QA Probe").AddComponent<QAProbe>();
        }

        // Unity's "destroyed equals null" only works through a UnityEngine.Object reference.
        private static IGameCommands FindGame()
        {
            var game = GameCommandsLocator.Current;
            return game is Object unityObject && unityObject == null ? null : game;
        }
    }
}
#endif
