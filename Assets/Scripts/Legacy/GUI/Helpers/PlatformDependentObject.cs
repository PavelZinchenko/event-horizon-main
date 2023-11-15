using Services.Gui;
using UnityEngine;
using Zenject;

namespace Gui
{
    public class PlatformDependentObject : MonoBehaviour
    {
        [Inject] private readonly IGuiManager _guiManager;

        public enum Condition
        {
            Ignore = 0,
            Must = 1,
            MustNot = 2,
            AnyOf = 3,
        }

        [SerializeField] Condition Editor = Condition.Ignore;
        [SerializeField] Condition Android = Condition.Ignore;
        [SerializeField] Condition Ios = Condition.Ignore;
        [SerializeField] Condition Standalone = Condition.Ignore;
        [SerializeField] Condition WebGL = Condition.Ignore;
        [SerializeField] Condition Premium = Condition.Ignore;
        [SerializeField] Condition Online = Condition.Ignore;

        [SerializeField] private bool ForceAwake = false;

        [Inject]
        private void Initialize()
        {
            if (!IsObjectAllowed())
            {
                Destroy(gameObject);
                return;
            }

            if (ForceAwake) gameObject.SetActive(true);
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        private bool IsObjectAllowed()
        {
#if UNITY_EDITOR
            if (Editor == Condition.Ignore && Android == Condition.Ignore && Ios == Condition.Ignore && WebGL == Condition.Ignore &&
                Standalone == Condition.Ignore && Premium == Condition.Ignore && Online == Condition.Ignore)
            {
                Debug.LogError(GetGameObjectPath(gameObject) + " - No platforms defined", gameObject);
                Debug.Break();
            }
#endif

#if IAP_DISABLED
            if (!TrySurvive(Premium, false)) return false;
#else
            if (!TrySurvive(Premium, true)) return false;
#endif

#if NO_INTERNET
            if (!TrySurvive(Online, false)) return false;
#else
            if (!TrySurvive(Online, true)) return false;
#endif

#if UNITY_EDITOR
            if (!TrySurvive(Editor, true)) return false;
#else
            if (!TrySurvive(Editor, false)) return false;
#endif

#if UNITY_ANDROID
            if (!TrySurvive(Android, true)) return false;
#else
            if (!TrySurvive(Android, false)) return false;
#endif

#if UNITY_STANDALONE
            if (!TrySurvive(Standalone, true)) return false;
#else
            if (!TrySurvive(Standalone, false)) return false;
#endif

#if UNITY_IOS
            if (!TrySurvive(Ios, true)) return false;
#else
            if (!TrySurvive(Ios, false)) return false;
#endif

#if UNITY_WEBGL
            if (!TrySurvive(WebGL, true)) return false;
#else
            if (!TrySurvive(WebGL, false)) return false;
#endif

            if (_anyOfCondition && !_anyRequirementMet) return false;

            return true;
        }

        private bool TrySurvive(Condition condition, bool state)
        {
            if (condition == Condition.Ignore) return true;
            if (condition == Condition.Must && state) return true;
            if (condition == Condition.MustNot && !state) return true;
            if (condition == Condition.AnyOf)
            {
                _anyOfCondition = true;
                if (state) _anyRequirementMet = true;
                return true;
            }

            return false;
        }

        private bool _anyOfCondition;
        private bool _anyRequirementMet;
    }
}
