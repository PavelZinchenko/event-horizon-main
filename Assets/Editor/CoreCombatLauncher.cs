using GameStateMachine;
using GameStateMachine.States;
using UnityEditor;
using UnityEngine;
using Zenject;

// Starts quick combat on the combat core from the main menu, so every play-mode check enters the same way
// (migration phase 3c). Both fleets lead with build 34 (f0s1); the rest are random.
public static class CoreCombatLauncher
{
    public const string TestShipId = "@34";

    [MenuItem("Tools/Combat/Start Core Quick Combat")]
    public static void Start()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("CoreCombatLauncher: enter Play mode first.");
            return;
        }

        foreach (var context in Object.FindObjectsByType<SceneContext>(FindObjectsSortMode.None))
        {
            if (context.gameObject.scene.name != "LoaderScene") continue;

            var container = context.Container;
            var stateMachine = container.Resolve<IStateMachine>();
            if (stateMachine.ActiveState != StateType.MainMenu)
            {
                Debug.LogError($"CoreCombatLauncher: the active state is {stateMachine.ActiveState}, not the main menu.");
                return;
            }

            container.Resolve<StartQuickBattleSignal.Trigger>().Fire(false, TestShipId);
            return;
        }

        Debug.LogError("CoreCombatLauncher: LoaderScene is not loaded.");
    }

    [MenuItem("Tools/Combat/Start Core Quick Combat", true)]
    public static bool CanStart() => EditorApplication.isPlaying;
}
