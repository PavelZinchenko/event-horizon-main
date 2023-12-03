using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gui.Presenter
{
    public abstract class PresenterBase : MonoBehaviour
    {
#if UNITY_EDITOR
        private const string _generatedFilePath = "Modules/Gui/Scripts/Generated";
        [SerializeField] private string _rootElement;
#endif
        [SerializeField] private UIDocument _uIDocument;

        public virtual VisualElement RootElement => null;

        protected UIDocument UiDocument => _uIDocument;

#if UNITY_EDITOR
        [ContextMenu("Ganerate Code")]
        private void GenerateScript()
        {
            var ns = GetType().Namespace ?? string.Empty;
            var className = GetType().Name;

            var code = new CodeGenerator.DocumentClassBuilder(className)
                .Using("Gui.Presenter")
                .Using("UnityEngine.UIElements")
                .SetNamespace(ns)
                .SetBaseClass(nameof(PresenterBase))
                .SetRootElement(_rootElement)
                .Build(_uIDocument);

            var path = Path.Combine(Application.dataPath, _generatedFilePath, ns.Replace('.', '\\'));
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, className + "Partial.cs"), code);
        }
#endif
    }
}
