using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonController)), CanEditMultipleObjects]
public class ButtonControllerEditor : Editor
{
    public SerializedProperty _buttonColor, _buttonType, _temporaryCooldown, _requiresBoth, _detectionRadius, _activatables, _events, _animator, _lightMeshRenderer, _lightMaterials, _glassMeshRenderers, _glassMaterials;

    private void OnEnable()
    {
        _buttonColor = serializedObject.FindProperty("_buttonColor");
        _buttonType = serializedObject.FindProperty("_buttonType");
        _temporaryCooldown = serializedObject.FindProperty("_temporaryCooldown");
        _requiresBoth = serializedObject.FindProperty("_requiresBoth");
        _detectionRadius = serializedObject.FindProperty("_detectionRadius");
        _activatables = serializedObject.FindProperty("_activatables");
        _events = serializedObject.FindProperty("_events");
        _animator = serializedObject.FindProperty("_animator");
        _lightMeshRenderer = serializedObject.FindProperty("_lightMeshRenderer");
        _lightMaterials = serializedObject.FindProperty("_lightMaterials");
        _glassMeshRenderers = serializedObject.FindProperty("_glassMeshRenderers");
        _glassMaterials = serializedObject.FindProperty("_glassMaterials");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_buttonColor);
        EditorGUILayout.PropertyField(_buttonType);

        if (_buttonType.enumValueIndex == (int)ButtonType.Temporary)
        {
            EditorGUILayout.PropertyField(_temporaryCooldown);
        }

        EditorGUILayout.PropertyField(_requiresBoth);
        if (_requiresBoth.boolValue)
        {
            EditorGUILayout.PropertyField(_detectionRadius);
        }

        EditorGUILayout.PropertyField(_activatables);

        EditorGUILayout.PropertyField(_animator);

        EditorGUILayout.PropertyField(_lightMeshRenderer);
        EditorGUILayout.PropertyField(_lightMaterials);

        EditorGUILayout.PropertyField(_glassMeshRenderers);
        EditorGUILayout.PropertyField(_glassMaterials);

        EditorGUILayout.PropertyField(_events);

        serializedObject.ApplyModifiedProperties();
    }
}
