using System.Collections;
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
    [SerializeField]
    private AudioClip hurtSound;
    [SerializeField]
    private AudioSource soundSource;

    public Character character { get; private set; }

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
            character = new PlayableCharacter(this, wand, protagonist, spellStepEmitter, sparksEmitter, cooldownTime, loadingTime,
                                              hurtSound, soundSource);
        } else {
            character = new NonPlayableCharacter(this, wand, protagonist, spellStepEmitter, cooldownTime, tracingTime, loadingTime,
                                                 hurtSound, soundSource);
        }
        GameManagerScript.AppendCharacter(character);
    }
}

#if UNITY_EDITOR
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
                       wand,
                       hurtSound,
                       soundSource;

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
        hurtSound = serializedObject.FindProperty("hurtSound");
        soundSource = serializedObject.FindProperty("soundSource");
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
        hurtSound.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Hurt Sound"), hurtSound.objectReferenceValue,
                                                                     typeof(AudioClip), true);
        soundSource.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sound Source"), soundSource.objectReferenceValue,
                                                                       typeof(AudioSource), true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
