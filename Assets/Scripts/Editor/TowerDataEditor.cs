using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Custom inspector for TowerData with rotation sprite helpers
    /// </summary>
    [CustomEditor(typeof(TowerData))]
    public class TowerDataEditor : UnityEditor.Editor
    {
        private SerializedProperty useRotationSprites;
        private SerializedProperty rotationSprites;
        private SerializedProperty spriteAngles;

        private void OnEnable()
        {
            useRotationSprites = serializedObject.FindProperty("useRotationSprites");
            rotationSprites = serializedObject.FindProperty("rotationSprites");
            spriteAngles = serializedObject.FindProperty("spriteAngles");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            serializedObject.Update();
            
            // Rotation sprite helpers
            if (useRotationSprites.boolValue)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Rotation Setup", EditorStyles.boldLabel);
                
                // Quick setup buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Setup 8-Direction Sprites"))
                {
                    SetupEightDirectionSprites();
                }
                if (GUILayout.Button("Setup 4-Direction Sprites"))
                {
                    SetupFourDirectionSprites();
                }
                EditorGUILayout.EndHorizontal();
                
                // Validation
                if (rotationSprites.arraySize > 0)
                {
                    int emptySprites = 0;
                    for (int i = 0; i < rotationSprites.arraySize; i++)
                    {
                        if (rotationSprites.GetArrayElementAtIndex(i).objectReferenceValue == null)
                            emptySprites++;
                    }
                    
                    if (emptySprites > 0)
                    {
                        EditorGUILayout.HelpBox($"{emptySprites} rotation sprites are not assigned.", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("All rotation sprites assigned!", MessageType.Info);
                    }
                }
                
                // Angle visualization
                if (rotationSprites.arraySize > 0 && spriteAngles.arraySize > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Sprite Angles:", EditorStyles.miniBoldLabel);
                    for (int i = 0; i < Mathf.Min(rotationSprites.arraySize, spriteAngles.arraySize); i++)
                    {
                        var sprite = rotationSprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                        var angle = spriteAngles.GetArrayElementAtIndex(i).floatValue;
                        string spriteName = sprite ? sprite.name : "None";
                        EditorGUILayout.LabelField($"  [{i}] {angle}° - {spriteName}");
                    }
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void SetupEightDirectionSprites()
        {
            rotationSprites.arraySize = 8;
            spriteAngles.arraySize = 8;
            
            float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
            for (int i = 0; i < 8; i++)
            {
                spriteAngles.GetArrayElementAtIndex(i).floatValue = angles[i];
            }
            
            Debug.Log("TowerData: Set up for 8-direction rotation sprites. Assign sprites in order: 0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°");
        }

        private void SetupFourDirectionSprites()
        {
            rotationSprites.arraySize = 4;
            spriteAngles.arraySize = 4;
            
            float[] angles = { 0f, 90f, 180f, 270f };
            for (int i = 0; i < 4; i++)
            {
                spriteAngles.GetArrayElementAtIndex(i).floatValue = angles[i];
            }
            
            Debug.Log("TowerData: Set up for 4-direction rotation sprites. Assign sprites in order: 0°, 90°, 180°, 270°");
        }
    }
}