using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Marching))]
public class PlanetEditor : Editor
{
    Marching marching;
    Editor shapeEditor;
    Editor colourEditor;

    public override void OnInspectorGUI()
    {
       using (var check = new EditorGUI.ChangeCheckScope())
       {
           base.OnInspectorGUI();
           if (check.changed)
           {
               marching.UpdateMesh();
           }
       }
       
       if (GUILayout.Button("Generate Planet")) {
           marching.UpdateMesh();
       }

        DrawSettingsEditor(marching.shapeSettings, marching.OnShapeSettingsUpdated, ref marching.shapeSettingsFoldout, ref shapeEditor);
        DrawSettingsEditor(marching.colourSettings, marching.onColourSettingsUpdated, ref marching.colourSettingsFoldout, ref colourEditor);
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        // foldout is if you can press the little arrow
        if( settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);

                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        marching = (Marching)target;
    }
}
