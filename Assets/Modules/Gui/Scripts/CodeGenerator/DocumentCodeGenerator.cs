using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gui.CodeGenerator
{
    [RequireComponent(typeof(UIDocument))]
    public class DocumentCodeGenerator : MonoBehaviour
    {
        [SerializeField] private string _generatedFilePath = "Modules/Gui/Scripts/Generated";
        [SerializeField] private string _namespace = "Gui.Generated";

        private const string _nameSuffix = "DocumentModel";

#if UNITY_EDITOR
        [ContextMenu("Ganerate Code")]
        private void GenerateScript()
        {
            var document = GetComponent<UIDocument>();
            var className = gameObject.name + _nameSuffix;

            var code = new DocumentClassBuilder(className)
                .SetNamespace(_namespace)
                .Build(document);

            File.WriteAllText(Path.Combine(Application.dataPath, _generatedFilePath, className + ".cs"), code);
        }
#endif
    }
}
