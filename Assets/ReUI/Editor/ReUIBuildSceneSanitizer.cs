using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReUI.Editor
{
    /// <summary>
    /// Removes missing MonoBehaviour components only from Unity's temporary build
    /// scene instances. Source .unity files are never saved or modified.
    /// Unity 6000.0.75f1 can crash in WriteBuildSceneFile when a scene containing
    /// a missing script is rebuilt after the player serialization cache is cleared.
    /// </summary>
    internal sealed class ReUIBuildSceneSanitizer : IProcessSceneWithReport
    {
        public int callbackOrder => -10000;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            int removed = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    GameObject gameObject = transforms[j].gameObject;
                    int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                    if (missing <= 0) continue;

                    removed += missing;
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                }
            }

            if (removed > 0)
                Debug.Log($"[ReUI Build] Removed {removed} missing script component(s) from temporary scene copy: {scene.path}");
        }
    }
}
