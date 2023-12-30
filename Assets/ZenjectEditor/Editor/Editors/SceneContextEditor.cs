using UnityEditor;

namespace Zenject
{
    [CustomEditor(typeof(SceneContext))]
    public class SceneContextEditor : ContextEditor
    {
        SerializedProperty _contractNameProperty;
        SerializedProperty _parentContractNameProperty;
        SerializedProperty _parentNewObjectsUnderRootProperty;

        public override void OnEnable()
        {
            base.OnEnable();

            _contractNameProperty = serializedObject.FindProperty("_contractNames");
            _parentContractNameProperty = serializedObject.FindProperty("_parentContractName");
            _parentNewObjectsUnderRootProperty = serializedObject.FindProperty("_parentNewObjectsUnderRoot");
        }

        protected override void OnGui()
        {
            base.OnGui();

            EditorGUILayout.PropertyField(_contractNameProperty, true);
            EditorGUILayout.PropertyField(_parentContractNameProperty);
            EditorGUILayout.PropertyField(_parentNewObjectsUnderRootProperty);
        }
    }
}



