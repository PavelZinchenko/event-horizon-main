using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Callbacks;

namespace EditorWindows
{
    public class ApplicationManager : EditorWindow
    {
        [MenuItem("Window/Application Manager")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ApplicationManager));
        }

		public enum IapType
		{
			Disabled,
			Unity,
			Yandex
		}

		public enum AdsType
        {
            Disabled,
            Unity,
            Google,
            Yandex,
            Appodeal
        }

        public enum LicenseType
        {
            Undefined,
            OpenSource,
            Commercial
        }

        public enum StoreType
        {
            Undefined,
            Yandex,
            Google,
            Apple,
            Web,
            Steam,
        }

        public static string StoreTypeToName(StoreType storeType)
        {
            switch (storeType)
            {
                case StoreType.Yandex:
                    return "STORE_YANDEX";
                case StoreType.Google:
                    return "STORE_GOOGLE";
                case StoreType.Apple:
                    return "STORE_APPLE";
                case StoreType.Web:
                    return "STORE_WEB";
                case StoreType.Steam:
                    return "STORE_STEAM";
                default:
                    return string.Empty;
            }
        }

        public static string IapTypeToName(IapType iapType)
        {
            switch (iapType)
            {
                case IapType.Unity:
                    return "IAP_UNITY";
                case IapType.Yandex:
                    return "IAP_YANDEX";
				case IapType.Disabled:
                default:
                    return "IAP_DISABLED";
            }
        }

        public static string AdsTypeToName(AdsType adsType)
        {
            switch (adsType)
            {
                case AdsType.Unity:
                    return "ADS_UNITY";
                case AdsType.Yandex:
                    return "ADS_YANDEX";
                case AdsType.Google:
                    return "ADS_GOOGLE";
                case AdsType.Appodeal:
                    return "ADS_APPODEAL";
                case AdsType.Disabled:
                default:
                    return "ADS_DISABLED";
            }
        }

        public static string LicenseTypeToName(LicenseType licenseType)
        {
            switch (licenseType)
            {
                case LicenseType.Commercial:
                    return "LICENSE_COMMERCIAL";
                case LicenseType.OpenSource:
                    return "LICENSE_OPENSOURCE";
                case LicenseType.Undefined:
                default:
                    return string.Empty;
            }
        }

        private void SelectStoreType()
        {
            var storeType = (StoreType)EditorGUILayout.EnumPopup("Store", _storeType);
            if (storeType == _storeType) return;

            RemoveDefineSymbol(StoreTypeToName(_storeType));
            AddDefineSymbol(StoreTypeToName(storeType));

            _storeType = storeType;
        }

        private void SelectIapType()
        {
            var iapType = (IapType)EditorGUILayout.EnumPopup("InAppPurchases", _iapType);
            if (iapType == _iapType) return;

            RemoveDefineSymbol(IapTypeToName(_iapType));
            AddDefineSymbol(IapTypeToName(iapType));

            _iapType = iapType;
        }

        private void SelectAdsType()
        {
            var adsType = (AdsType)EditorGUILayout.EnumPopup("Advertisements", _adsType);
            if (adsType == _adsType) return;

            RemoveDefineSymbol(AdsTypeToName(_adsType));
            AddDefineSymbol(AdsTypeToName(adsType));

            _adsType = adsType;
        }

        private void SelectLicenseType()
        {
            var license = (LicenseType)EditorGUILayout.EnumPopup("License", _license);
            if (license == _license) return;

            RemoveDefineSymbol(LicenseTypeToName(_license));
            AddDefineSymbol(LicenseTypeToName(license));

            _license = license;
        }

        private void OnGUI()
        {
            GUILayout.Label("Actions", EditorStyles.boldLabel);
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Cannot modify config in play mode.");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Update config file"))
                    GenerateAppConfigFile(_localizationFile, false, _fakeIap, _enableCheats, _alternativeTitle);

                if (GUILayout.Button("Generate ship prefabs"))
                    GenerateShipPrefabs();

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            UpdateId(EditorGUILayout.TextField("Bundle id", PlayerSettings.applicationIdentifier));
            UpdateVersion(EditorGUILayout.TextField("Bundle version", PlayerSettings.bundleVersion));
            UpdateAndroidVersionCode(EditorGUILayout.TextField("Android version code",
                PlayerSettings.Android.bundleVersionCode.ToString()));
            _localizationFile = EditorGUILayout.TextField("Localizations", _localizationFile);
            _fakeIap = EditorGUILayout.Toggle("Fake in app purchases ", _fakeIap);
			_enableCheats = EditorGUILayout.Toggle("Enable cheats", _enableCheats);

			var alternativeTitle = EditorGUILayout.Toggle("Use alternative title", _alternativeTitle);
			if (alternativeTitle != _alternativeTitle)
			{
				_alternativeTitle = alternativeTitle;
				PlayerSettings.productName = alternativeTitle ? ProductNameAlternative : ProductNameDefault;
			}

#if NO_GPGS
			if (!EditorGUILayout.Toggle("Disable GooglePlay", true)) RemoveDefineSymbol("NO_GPGS");
#else
            if (EditorGUILayout.Toggle("Disable GooglePlay", false)) AddDefineSymbol("NO_GPGS");
#endif

            SelectStoreType();
            SelectIapType();
            SelectAdsType();
            SelectLicenseType();

#if NO_INTERNET
            if (!EditorGUILayout.Toggle("Disable Internet", true)) RemoveDefineSymbol("NO_INTERNET");
#else
            if (EditorGUILayout.Toggle("Disable Internet", false)) AddDefineSymbol("NO_INTERNET");
#endif

            EditorGUILayout.LabelField("Target dir", AssetsDir + ConfigDir);
            EditorGUILayout.LabelField("Config class name", ClassName);
        }

        private void RemoveDefineSymbol(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defines = definesString.Split(';').ToList();

            if (!defines.Remove(value))
            {
                Debug.LogError("Symbol not found - " + value);
                Debug.Break();
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defines.ToArray()));
        }

        private void AddDefineSymbol(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defines = definesString.Split(';').ToList();

            if (defines.Contains(value))
            {
                Debug.LogError("Symbol already defined - " + value);
                Debug.Break();
                return;
            }

            defines.Add(value);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defines.ToArray()));
        }

        private void GenerateShipPrefabs()
        {
            var shipSprites = LoadAllAssets<Sprite>("/Sprites/Ships").ToArray();
            var starbaseSprites = LoadAllAssets<Sprite>("/Sprites/Starbases").ToArray();
            var defaultPrefab = Resources.Load<GameObject>("Combat/Ships/Default");

            foreach (var sprite in shipSprites.Concat(starbaseSprites))
            {
                var prefabPath = "Combat/Ships/" + sprite.name;
                var prefab = Resources.Load<GameObject>(prefabPath);

                UnityEngine.Debug.Log(sprite.name + (prefab ? " - ok" : " - not found"));

                if (prefab)
                    continue;

                var ship = Instantiate(defaultPrefab);
                ship.name = sprite.name;
                ship.GetComponent<SpriteRenderer>().sprite = sprite;
                ship.AddComponent<PolygonCollider2D>();

                PrefabUtility.SaveAsPrefabAsset(ship, "Assets/Resources/" + prefabPath + ".prefab");
                DestroyImmediate(ship);
            }
        }

        private IEnumerable<T> LoadAllAssets<T>(string path) where T : UnityEngine.Object
        {
            var files =
                Directory.GetFiles(Application.dataPath + path, "*", SearchOption.AllDirectories)
                    .Where(file => !file.EndsWith(".meta"));
            foreach (var file in files)
            {
                var assetPath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    yield return asset;
            }
        }

        private void UpdateId(string id)
        {
            if (id == PlayerSettings.applicationIdentifier) return;

            PlayerSettings.applicationIdentifier = id;

            Debug.Log("bundle id updated");
        }

        private void UpdateVersion(string version)
        {
            if (version == PlayerSettings.bundleVersion) return;

            PlayerSettings.bundleVersion = version;

            Debug.Log("bundle version updated");
        }

        private void UpdateAndroidVersionCode(string version)
        {
            try
            {
                var code = Convert.ToInt32(version);
                if (code != PlayerSettings.Android.bundleVersionCode)
                {
                    PlayerSettings.Android.bundleVersionCode = code;
                    Debug.Log("android bundle version code updated");
                }
            }
            catch (Exception)
            {
            }
        }

        [PostProcessBuild]
        private static void OnPostBuild(BuildTarget buildTarget, string pathToBuildProject)
        {
            GenerateAppConfigFile(AppConfig.localizationFile, true, AppConfig.testMode, AppConfig.enableCheats, AppConfig.alternativeTitle);
        }

        private static void GenerateAppConfigFile(
            string localizationFile,
            bool increaseBuildNumber, 
            bool fakePurchases, 
            bool enableCheats,
			bool alternativeTitle)
        {
            var code = string.Empty;
            code += "public static class " + ClassName + Environment.NewLine;
            code += "{" + Environment.NewLine;
            code += "    public const string bundleIdentifier = \"" + PlayerSettings.applicationIdentifier + "\";" +
                    Environment.NewLine;
            code += "    public const string version = \"" + PlayerSettings.bundleVersion + "\";" + Environment.NewLine;
            code += "    public const int versionCode = " + PlayerSettings.Android.bundleVersionCode + ";" +
                    Environment.NewLine;
            code += "    public const int buildNumber = " +
                    (increaseBuildNumber ? AppConfig.buildNumber + 1 : AppConfig.buildNumber) + ";" +
                    Environment.NewLine;
            code += "    public const string localizationFile = \"" + localizationFile + "\";" + Environment.NewLine;
            code += "    public const bool testMode = " + (fakePurchases ? "true" : "false") + ";" +
                    Environment.NewLine;
			code += "    public const bool enableCheats = " + (enableCheats ? "true" : "false") + ";" + Environment.NewLine;
			code += "    public const bool alternativeTitle = " + (alternativeTitle ? "true" : "false") + ";" + Environment.NewLine;
			code += "}" + Environment.NewLine;

            CreateNewBuildVersionClassFile(code);
        }

        private static void CreateNewBuildVersionClassFile(string code)
        {
            if (String.IsNullOrEmpty(code))
            {
                Debug.Log("Code generation stopped, no code to write.");
            }

            CheckOrCreateDirectory(AssetsDir + ConfigDir);

            var fileName = AssetsDir + ConfigDir + "/" + ClassName + ".cs";
            bool success = false;
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                try
                {
                    writer.WriteLine("{0}", code);
                    success = true;
                }
                catch (Exception ex)
                {
                    string msg = " \n" + ex;
                    Debug.LogError(msg);
                    EditorUtility.DisplayDialog("Error when trying to generate file " + fileName, msg, "OK");
                }
            }
            if (success)
            {
                AssetDatabase.Refresh(ImportAssetOptions.Default);
            }
        }

        private static void CheckOrCreateDirectory(string dir)
        {
            if (File.Exists(dir))
            {
                Debug.LogWarning(dir + " is a file instead of a directory !");
                return;
            }
            else if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex.Message);
                    throw ex;
                }
            }
        }

        private string _localizationFile = AppConfig.localizationFile;
        private bool _fakeIap = AppConfig.testMode;
        private bool _enableCheats = AppConfig.enableCheats;
		private bool _alternativeTitle = AppConfig.alternativeTitle;

#if STORE_YANDEX
        private StoreType _storeType = StoreType.Yandex;
#elif STORE_GOOGLE
		private StoreType _storeType = StoreType.Google;
#elif STORE_APPLE
        private StoreType _storeType = StoreType.Apple;
#elif STORE_WEB
        private StoreType _storeType = StoreType.Web;
#elif STORE_STEAM
        private StoreType _storeType = StoreType.Steam;
#else
        private StoreType _storeType = StoreType.Undefined;
#endif

#if IAP_DISABLED
        private IapType _iapType = IapType.Disabled;
#elif IAP_UNITY
		private IapType _iapType = IapType.Unity;
#elif IAP_YANDEX
        private IapType _iapType = IapType.Yandex;
#else
        private IapType _iapType = IapType.Disabled;
#endif

#if ADS_DISABLED
        private AdsType _adsType = AdsType.Disabled;
#elif ADS_UNITY
        private AdsType _adsType = AdsType.Unity;
#elif ADS_YANDEX
        private AdsType _adsType = AdsType.Yandex;
#elif ADS_APPODEAL
        private AdsType _adsType = AdsType.Appodeal;
#elif ADS_GOOGLE
        private AdsType _adsType = AdsType.Google;
#else
        private AdsType _adsType = AdsType.Disabled;
#endif

#if LICENSE_COMMERCIAL
        private LicenseType _license = LicenseType.Commercial;
#elif LICENSE_OPENSOURCE
        private LicenseType _license = LicenseType.OpenSource;
#else
        private LicenseType _license = LicenseType.Undefined;
#endif

        private const string AssetsDir = "Assets/";
        private const string ConfigDir = "Modules/AppConfiguration/Scripts/Generated";
        private const string ClassName = "AppConfig";

		private const string ProductNameDefault = "Event Horizon";
		private const string ProductNameAlternative = "Cosmic Horizons";
	}
}
