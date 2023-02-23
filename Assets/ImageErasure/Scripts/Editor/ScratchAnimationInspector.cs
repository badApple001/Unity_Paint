using ScratchCardAsset.Animation;
using UnityEditor;
using UnityEngine;

namespace ScratchCardAsset.Editor
{
    [CustomEditor(typeof(ScratchAnimation))]
    public class ScratchAnimationInspector : UnityEditor.Editor
    {
        private SerializedProperty scratchSpace;
        private SerializedProperty scratches;
        private ScratchAnimation scratchAnimation;
        private string jsonText;
        private Vector2 scroll;

        void OnEnable()
        {
            scratchSpace = serializedObject.FindProperty("ScratchSpace");
            scratches = serializedObject.FindProperty("Scratches");
            scratchAnimation = target as ScratchAnimation;
            if (scratchAnimation != null)
            {
                jsonText = scratchAnimation.ToJson();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(scratchSpace, new GUIContent("Scratch Space"));

            EditorGUILayout.PropertyField(scratches, new GUIContent("Scratches"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Hole Scratch"))
            {
                var scratch = new BaseScratch();
                scratchAnimation.Scratches.Add(scratch);
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Add Line Scratch"))
            {
                var scratch = new LineScratch();
                (target as ScratchAnimation)?.Scratches.Add(scratch);
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Json:");
            scroll = EditorGUILayout.BeginScrollView(scroll);
            jsonText = EditorGUILayout.TextArea(jsonText);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Get Json from Scratches"))
            {
                jsonText = scratchAnimation.ToJson();
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Set Scratches from Json"))
            {
                scratchAnimation.FromJson(jsonText);
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}