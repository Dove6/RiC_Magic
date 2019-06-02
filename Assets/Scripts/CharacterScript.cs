﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterScript : MonoBehaviour
{
    [SerializeField]
    bool playable = false;
    [SerializeField]
    bool protagonist = false;
    [SerializeField]
    float cooldownTime = 0.5f;
    [SerializeField]
    float tracingTime = 4f;
    [SerializeField]
    float loadingTime = 3f;
    [SerializeField]
    ParticleSystem spellStepEmitter;
    [SerializeField]
    ParticleSystem sparksEmitter;
    [SerializeField]
    WandScript wand;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    public void GetHurt()
    {
    }

    private void Awake()
    {
        if (playable) {
            GameManagerScript.AppendCharacter(new PlayableCharacter(this, wand, protagonist, spellStepEmitter, sparksEmitter, cooldownTime,
                                                                    loadingTime));
        } else {
            GameManagerScript.AppendCharacter(new NonPlayableCharacter(this, wand, protagonist, spellStepEmitter, cooldownTime, tracingTime,
                                                                       loadingTime));
        }
    }
}

[CustomEditor (typeof(CharacterScript))]
[CanEditMultipleObjects]
public class CharacterEditor : Editor
{
    SerializedProperty playable,
                       protagonist,
                       cooldownTime,
                       tracingTime,
                       loadingTime,
                       spellStepEmitter,
                       sparksEmitter,
                       wand;

    private void OnEnable()
    {
        playable = serializedObject.FindProperty("playable");
        protagonist = serializedObject.FindProperty("protagonist");
        cooldownTime = serializedObject.FindProperty("cooldownTime");
        tracingTime = serializedObject.FindProperty("tracingTime");
        loadingTime = serializedObject.FindProperty("loadingTime");
        spellStepEmitter = serializedObject.FindProperty("spellStepEmitter");
        sparksEmitter = serializedObject.FindProperty("sparksEmitter");
        wand = serializedObject.FindProperty("wand");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        playable.boolValue = EditorGUILayout.Toggle(new GUIContent("Playable"), playable.boolValue);
        protagonist.boolValue = EditorGUILayout.Toggle(new GUIContent("Protagonist"), protagonist.boolValue);
        wand.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Wand"), wand.objectReferenceValue,
                                                                typeof(WandScript), true);
        cooldownTime.floatValue = EditorGUILayout.FloatField(new GUIContent("Cooldown Time"), cooldownTime.floatValue);
        if (!playable.boolValue) {
            tracingTime.floatValue = EditorGUILayout.FloatField(new GUIContent("Tracing Time"), tracingTime.floatValue);
        }
        loadingTime.floatValue = EditorGUILayout.FloatField(new GUIContent("Loading Time"), loadingTime.floatValue);
        spellStepEmitter.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Spell Step Emitter"),
                                                                            spellStepEmitter.objectReferenceValue,
                                                                            typeof(ParticleSystem), true);
        if (playable.boolValue) {
            sparksEmitter.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sparks Emitter"),
                                                                             sparksEmitter.objectReferenceValue,
                                                                             typeof(ParticleSystem), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
