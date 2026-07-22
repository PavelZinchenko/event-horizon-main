using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReUI
{
    [DefaultExecutionOrder(-9000)]
    public sealed class ReUIBootstrap : MonoBehaviour
    {
        public const string EnabledPreference = "ReUI.Enabled";
        private const string ShipEditorSceneName = "ShipEditorScene";
        private const string CombatSceneName = "CombatScene";
        private const string SettingsSceneName = "SettingsScene";

        private static ReUIBootstrap _instance;

        public static bool IsEnabled => PlayerPrefs.GetInt(EnabledPreference, 1) != 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (!IsEnabled || _instance != null) return;

            GameObject host = new("ReUI Runtime", typeof(ReUIBootstrap))
            {
                hideFlags = HideFlags.DontSave,
            };
            DontDestroyOnLoad(host);
            _instance = host.GetComponent<ReUIBootstrap>();
        }

        public static void SetEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(EnabledPreference, enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (enabled)
            {
                Install();
                _instance?.ApplyNow();
            }
            else if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        /// <summary>
        /// Refreshes the small, explicitly supported ReUI surface set after a
        /// local theme preference changes. It deliberately does not restyle
        /// generic menu canvases.
        /// </summary>
        public static void RefreshTheme()
        {
            if (!IsEnabled) return;
            if (!Application.isPlaying)
            {
                // Editor validation constructs temporary UI controls. Do not
                // create a DontDestroyOnLoad runtime host from that context.
                _instance?.ApplyNow(true);
                return;
            }
            Install();
            _instance?.ApplyNow(true);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Start()
        {
            ApplyNow();
            StartCoroutine(ApplyAfterSceneInitialization(SceneManager.GetActiveScene()));
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsSupportedScene(scene.name))
                return;

            ApplyScene(scene);
            StartCoroutine(ApplyAfterSceneInitialization(scene));
        }

        public void ApplyNow()
        {
            ApplyNow(false);
        }

        private void ApplyNow(bool resetStyleFlags)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                ApplyScene(SceneManager.GetSceneAt(i), resetStyleFlags);
        }

        private static void ApplyScene(Scene scene, bool resetStyleFlags = false)
        {
            if (!scene.IsValid() || !scene.isLoaded || !IsSupportedScene(scene.name))
                return;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Canvas[] canvases = roots[rootIndex].GetComponentsInChildren<Canvas>(true);
                for (int canvasIndex = 0; canvasIndex < canvases.Length; canvasIndex++)
                {
                    Canvas canvas = canvases[canvasIndex];
                    if (canvas == null || !canvas.gameObject.scene.IsValid()) continue;
                    if ((canvas.hideFlags & HideFlags.HideAndDontSave) != 0) continue;

                    if (scene.name == ShipEditorSceneName)
                        ReUIShipEditorStyler.Apply(canvas);
                    else if (scene.name == CombatSceneName)
                        ReUIHudStyler.Apply(canvas);
                    else if (scene.name == SettingsSceneName)
                        ReUIThemePalettePanel.EnsureForSettings(canvas);
                }
            }
        }

        private IEnumerator ApplyAfterSceneInitialization(Scene scene)
        {
            yield return null;
            ApplyScene(scene);
            yield return new WaitForEndOfFrame();
            ApplyScene(scene);
            yield return new WaitForSecondsRealtime(0.20f);
            ApplyScene(scene);
        }

        private static bool IsSupportedScene(string sceneName)
        {
            // Beta5 intentionally limited ReUI to the ship editor's component
            // list and the combat HUD. Settings is included solely to host the
            // local theme palette; it does not restyle authored controls.
            return sceneName == ShipEditorSceneName ||
                   sceneName == CombatSceneName ||
                   sceneName == SettingsSceneName;
        }
    }
}
