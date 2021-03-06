﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum State { idle, tracing, loading, loaded };
public enum Spell { pudding, sleep, flies, frog, whirls, darkness };
public struct SpellHeader
{
    public int  identifier;
    public byte width,
                height,
                dataType,
                dataSize;
    public bool CheckIdentifier() => identifier == 5395009; //"ARR\0"
};

public abstract class Character
{
    protected static Dictionary<bool, List<Rigidbody2D>> rigidbodyList = new Dictionary<bool, List<Rigidbody2D>>();
    protected static Dictionary<Spell, List<Vector2>> patternList = new Dictionary<Spell, List<Vector2>>();
    protected static Dictionary<Spell, string> spellFiles = new Dictionary<Spell, string>() {
        [Spell.pudding] = "pudding",
        [Spell.sleep] = "sleep",
        [Spell.flies] = "flies",
        [Spell.frog] = "frog",
        [Spell.whirls] = "whirls",
        [Spell.darkness] = "darkness"
    };
    
    public bool protagonist { get; protected set; }
    protected CharacterScript character;
    protected WandScript wand;
    protected ParticleSystem spellStepEmitter;
    protected float cooldownTime;
    protected int cooldownTimer;
    protected float loadingTime;
    protected int loadingTimer;
    protected Animator animator;
    protected AudioClip hurtSound;
    protected AudioSource soundSource;
    protected Dictionary<Spell, int> takenSpells;
    protected State state = State.idle;
    protected Spell? loadedSpell = null;
    protected System.Random random = new System.Random();

    protected abstract bool CheckForTracingStart();
    protected abstract bool CheckForTracingEnd();
    protected abstract bool CheckForShot();
    protected abstract void CalculateShot(Vector2 MousePosition);
    protected abstract bool RecognizeSpell();
    public abstract void Update();
    protected abstract void AlterState(State desiredState);

    public void GetHurt()
    {
        animator.SetTrigger("hurt");
        soundSource.PlayOneShot(hurtSound);
    }
    protected void LoadPatternDictionary()
    {
        SpellHeader spellHeader;
        int headerSize = Marshal.SizeOf<SpellHeader>();

        foreach (KeyValuePair<Spell, string> spellFile in spellFiles) {
            TextAsset spellData = Resources.Load<TextAsset>(spellFile.Value);
            if (spellData.bytes.Length >= 8) {
                spellHeader = new SpellHeader {
                    identifier = BitConverter.ToInt32(spellData.bytes, 0),
                    width = spellData.bytes[4],
                    height = spellData.bytes[5],
                    dataType = spellData.bytes[6],
                    dataSize = spellData.bytes[7]
                };
                if (spellHeader.CheckIdentifier()) {
                    if (spellData.bytes.Length == headerSize + spellHeader.width * spellHeader.height * spellHeader.dataSize) {
                        MonoBehaviour.print("OK: " + spellFile.Key);
                        if (spellHeader.height == 2) {
                            patternList.Add(spellFile.Key, new List<Vector2>(spellHeader.width));
                            for (int i = 0; i < spellHeader.width; i++) {
                                patternList[spellFile.Key].Add(new Vector2(BitConverter.ToSingle(spellData.bytes, headerSize + i * 8),
                                                                         BitConverter.ToSingle(spellData.bytes, headerSize + i * 8 + 4)));
                                MonoBehaviour.print(patternList[spellFile.Key][i]);
                            }
                            MonoBehaviour.print("Spell " + spellFile.Key + " loaded");
                        } else {
                            MonoBehaviour.print("Incorrect spell point format: " + spellFile.Key);
                        }
                    } else {
                        MonoBehaviour.print("Incorrect total size: " + spellFile.Key);
                    }
                } else {
                    MonoBehaviour.print("Incorrect identifier: " + spellFile.Key);
                }
            } else {
                MonoBehaviour.print("Incorrect size: " + spellFile.Key);
            }
        }
    }
}

public class NonPlayableCharacter : Character
{

    private float tracingTime;
    private int tracingTimer;

    private readonly float spellStepDistance = 0.4f;
    private readonly float patternSizeMultiplier = 2f;
    private Vector2 spellPosition = new Vector2(0f, 0.5f);
    private CounterScript spellStepCounter = new CounterScript(0, 1);
    private static List<Vector2> pattern = new List<Vector2>();

    private Spell? tracedSpell = null;

    protected override bool CheckForTracingStart()
    {
        return TimerScript.HasPassed(cooldownTimer);
    }
    protected override bool CheckForTracingEnd()
    {
        if (TimerScript.HasPassed(tracingTimer)) {
            int index = spellStepCounter.Get();
            if (index < pattern.Count) {
                TimerScript.Remove(tracingTimer);
                tracingTimer = TimerScript.MakeTimer(tracingTime);
                if (spellStepEmitter != null) {
                    ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams() {
                        position = new Vector3(pattern[index].x, pattern[index].y, -1f)
                    };
                    spellStepEmitter.Emit(emitParams, 1);
                }
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
    }
    protected override bool CheckForShot()
    {
        return TimerScript.HasPassed(cooldownTimer);
    }
    protected override void CalculateShot(Vector2 MousePosition)
    {
        //to-do: harm player
    }
    protected override bool RecognizeSpell()
    {
        return true;
    }
    public override void Update()
    {
        //GameManagerScript.print((protagonist ? "protagonist " : "antagonist ") + state.ToString());
        switch (state) {
            case State.idle: {
                if (CheckForTracingStart()) {
                    AlterState(State.tracing);
                }
                break;
            }
            case State.tracing: {
                if (CheckForTracingEnd()) {
                    if (spellStepEmitter != null) {
                        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                        spellStepEmitter.Clear();
                        pattern.ForEach(delegate (Vector2 position) {
                            emitParams.position = new Vector3(position.x, position.y, -1f);
                            emitParams.startColor = new Color32(0, 255, 0, 255);
                            emitParams.startLifetime = 1f;
                            spellStepEmitter.Emit(emitParams, 1);
                        });
                    }
                    AlterState(State.loading);
                }
                break;
            }
            case State.loading: {
                if (TimerScript.HasPassed(loadingTimer)) {
                    AlterState(State.loaded);
                }
                break;
            }
            case State.loaded: {
                if (CheckForShot()) {
                    //take first enemy from list and harm it
                    GameManagerScript.FindFirstCharacter(!protagonist).GetHurt();
                    AlterState(State.idle);
                }
                break;
            }
        }
    }
    protected override void AlterState(State desiredState)
    {
        if (state != desiredState) {
            State previousState = state;
            state = desiredState;
            if (previousState == State.idle && state == State.tracing) {
                tracingTimer = TimerScript.MakeTimer(tracingTime);
                animator.SetFloat("wand_speed", 2f);
                animator.SetBool("wand_tracing", true);

                spellStepCounter.Reset();
                pattern.Clear();
                Array spellsForRandomization = Enum.GetValues(typeof(Spell));
                tracedSpell = (Spell)spellsForRandomization.GetValue(random.Next(spellsForRandomization.Length));
                MonoBehaviour.print("Now tracing: " + tracedSpell);
                for (int i = 0; i < patternList[tracedSpell.Value].Count - 1; i++) {
                    Vector2 startPoint = new Vector2(patternList[tracedSpell.Value][i].x,
                                                     patternList[tracedSpell.Value][i].y) * patternSizeMultiplier,
                            endPoint = new Vector2(patternList[tracedSpell.Value][i + 1].x,
                                                   patternList[tracedSpell.Value][i + 1].y) * patternSizeMultiplier,
                            lineVector = new Vector2(endPoint.x - startPoint.x, endPoint.y - startPoint.y),
                            point;
                    float lineLength = Vector2.Distance(startPoint, endPoint),
                          vectorMultiplier = spellStepDistance / lineLength;
                    for (int j = 0; ; j++) {
                        point = new Vector2(startPoint.x + vectorMultiplier * lineVector.x * j,
                                            startPoint.y + vectorMultiplier * lineVector.y * j);
                        if (Vector2.Distance(startPoint, point) < lineLength) {
                            pattern.Add(point);
                        } else {
                            break;
                        }
                    }
                }
                pattern.Add(patternList[tracedSpell.Value][patternList[tracedSpell.Value].Count - 1] * patternSizeMultiplier);
            } else if (previousState == State.tracing && state == State.idle) {
                animator.SetFloat("wand_speed", 1f);
                animator.SetBool("wand_tracing", false);
            } else if (previousState == State.tracing && state == State.loading) {
                loadingTimer = TimerScript.MakeTimer(loadingTime);
                wand.PlaySingleSound(WandScript.SoundEffect.recognized);
                animator.SetFloat("wand_speed", 1f);
                animator.SetBool("wand_tracing", false);
                animator.SetBool("wand_loading", true);
            } else if (previousState == State.loading && state == State.loaded) {
                TimerScript.Remove(loadingTimer);
                TimerScript.Remove(cooldownTimer);
                cooldownTimer = TimerScript.MakeTimer(cooldownTime);
                animator.SetBool("wand_loading", false);
            } else if (previousState == State.loaded && state == State.idle) {
                TimerScript.Remove(cooldownTimer);
                cooldownTimer = TimerScript.MakeTimer(cooldownTime);
                wand.PlaySingleSound(WandScript.SoundEffect.shot);
                animator.SetTrigger("wand_shot");
            }
            wand.ChangeSoundEffect(state);
        }
    }

    public NonPlayableCharacter(CharacterScript characterScript, WandScript wandScript, bool protagonist, ParticleSystem spellStepEmitter,
                                float cooldownTime, float tracingTime, float loadingTime, AudioClip hurtSound, AudioSource soundSource)
    {
        character = characterScript;
        wand = wandScript;
        this.protagonist = protagonist;
        this.cooldownTime = cooldownTime;
        //this.tracingTime = tracingTime;
        this.tracingTime = 0.25f;
        this.loadingTime = loadingTime;

        if (!rigidbodyList.ContainsKey(protagonist)) {
            rigidbodyList.Add(protagonist, new List<Rigidbody2D>());
        }
        rigidbodyList[protagonist].Add(character.GetComponent<Rigidbody2D>());
        takenSpells = new Dictionary<Spell, int>();
        this.spellStepEmitter = spellStepEmitter;
        animator = characterScript.GetComponent<Animator>();
        this.hurtSound = hurtSound;
        this.soundSource = soundSource;

        cooldownTimer = TimerScript.MakeTimer(0f);

        if (patternList.Count == 0) {
            LoadPatternDictionary();
        }
    }
}

public class PlayableCharacter : Character
{
    protected ParticleSystem sparksEmitter;
    private readonly float spellStepMinDistance = 0.2f;
    protected float checkedPatternAccuracy = 0.1f;

    private List<Vector2> pattern;
    private static Dictionary<Spell, List<Vector2>> checkedPatternList = new Dictionary<Spell, List<Vector2>>();
    private CounterScript patternCounter,
                          particleCounter;

    private void LoadCheckedPatternDictionary()
    {
        foreach (KeyValuePair<Spell, List<Vector2>> pattern in patternList) {
            checkedPatternList.Add(pattern.Key, new List<Vector2>());
            for (int i = 0; i < pattern.Value.Count - 1; i++) {
                Vector2 startPoint = new Vector2(pattern.Value[i].x,
                                                    pattern.Value[i].y),
                        endPoint = new Vector2(pattern.Value[i + 1].x,
                                                pattern.Value[i + 1].y),
                        lineVector = new Vector2(endPoint.x - startPoint.x, endPoint.y - startPoint.y),
                        point;
                float lineLength = Vector2.Distance(startPoint, endPoint),
                      vectorMultiplier = checkedPatternAccuracy / lineLength;
                for (int j = 0; ; j++) {
                    point = new Vector2(startPoint.x + vectorMultiplier * lineVector.x * j,
                                        startPoint.y + vectorMultiplier * lineVector.y * j);
                    if (Vector2.Distance(startPoint, point) < lineLength) {
                        checkedPatternList[pattern.Key].Add(point);
                        //MonoBehaviour.print(point);
                    } else {
                        break;
                    }
                }
            }
            checkedPatternList[pattern.Key].Add(pattern.Value[pattern.Value.Count - 1]);
        }
    }
    protected override bool CheckForTracingStart()
    {
        if (TimerScript.HasPassed(cooldownTimer)) {
            return Input.GetMouseButton(0);
        } else {
            return false;
        }
    }
    protected override bool CheckForTracingEnd()
    {
        return !Input.GetMouseButton(0);
    }
    protected override bool CheckForShot()
    {
        return Input.GetMouseButton(0);
    }
    protected override void CalculateShot(Vector2 MousePosition)
    {
        Ray ClickRay = Camera.main.ScreenPointToRay(MousePosition);
        RaycastHit2D ClickHit = Physics2D.Raycast(ClickRay.origin, ClickRay.direction);
        //print(ClickHit.rigidbody);
        if (rigidbodyList[!protagonist].Contains(ClickHit.rigidbody)) {
            MonoBehaviour.print("Bingo");
            ClickHit.rigidbody.GetComponent<CharacterScript>().character.GetHurt();
        } else {
            MonoBehaviour.print("Fail");
        }
    }
    protected override bool RecognizeSpell()
    {
        List<Vector2> processedPattern = new List<Vector2>(pattern),
                      originalPattern = new List<Vector2>(pattern);
        //print(processedPattern);
        Vector2 bottomLeft = new Vector2(float.MaxValue, float.MaxValue),
                topRight = new Vector2(float.MinValue, float.MinValue);
        foreach (Vector2 spellStep in processedPattern) {
            if (spellStep.x < bottomLeft.x) {
                bottomLeft.x = spellStep.x;
            } else if (spellStep.x > topRight.x) {
                topRight.x = spellStep.x;
            }
            if (spellStep.y < bottomLeft.y) {
                bottomLeft.y = spellStep.y;
            } else if (spellStep.y > topRight.y) {
                topRight.y = spellStep.y;
            }
        }
        Vector2 spellSize = new Vector2(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        if (spellSize.x != 0 && spellSize.y != 0) {
            Vector2 multiplier = new Vector2(1 / spellSize.x, 1 / spellSize.y);
            for (int i = 0; i < processedPattern.Count; i++) {
                processedPattern[i] -= bottomLeft;
                processedPattern[i] = Vector2.Scale(processedPattern[i], multiplier);
                //MonoBehaviour.print(processedPattern[i]);
            }
            foreach (KeyValuePair<Spell, List<Vector2>> checkedPattern in checkedPatternList) {
                bool spellCompliance = true;
                for (int i = 0, j = 0; j < checkedPattern.Value.Count && spellCompliance; j++) {
                    float pointsDistance = Vector2.Distance(processedPattern[i], checkedPattern.Value[j]);
                    if (pointsDistance >= 3 * checkedPatternAccuracy) {
                        //MonoBehaviour.print("Too big distance: " + pointsDistance);
                        float lastPointsDistance = pointsDistance + 0.01f;
                        do {
                            if (i + 1 >= processedPattern.Count) {
                                //MonoBehaviour.print("End of i: " + i);
                                spellCompliance = false;
                                break;
                            }
                            i++;
                            lastPointsDistance = pointsDistance;
                            pointsDistance = Vector2.Distance(processedPattern[i], checkedPattern.Value[j]);
                        } while (pointsDistance >= 3 * checkedPatternAccuracy);
                        if (spellCompliance) {
                            //MonoBehaviour.print("Points match: " + processedPattern[i] + " " + checkedPattern[j]);
                        }
                    } else {
                        //MonoBehaviour.print("Points match: " + processedPattern[i] + " " + checkedPattern[j]);
                    }
                    float processedProgress = i / (float)processedPattern.Count,
                          checkedProgress = j / (float)checkedPattern.Value.Count;
                    if (Math.Abs(processedProgress - checkedProgress) > 0.2f) {
                        spellCompliance = false;
                    }
                }
                if (spellCompliance) {
                    loadedSpell = checkedPattern.Key;
                    MonoBehaviour.print("Spell recognized: " + loadedSpell);
                }
            }
            //if (Vector2.Distance(processedPattern[0], checkedPattern[0]) > 2 * spellStepDistance ||
            //    Vector2.Distance(processedPattern[processedPattern.Count - 1],
            //                     checkedPattern[checkedPattern.Count - 1]) > 2 * spellStepDistance) {
            //    spellCompliance = false;
            //}
            if (loadedSpell != null) {
                //MonoBehaviour.print("Spell recognized!");
                if (spellStepEmitter != null) {
                    ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                    spellStepEmitter.Clear();
                    originalPattern.ForEach(delegate (Vector2 Position) {
                        emitParams.position = new Vector3(Position.x, Position.y, -1f);
                        emitParams.startColor = new Color32(0, 255, 0, 255);
                        emitParams.startLifetime = 1f;
                        spellStepEmitter.Emit(emitParams, 1);
                        sparksEmitter.Emit(emitParams, 1);
                        //print(EmitParams.position);
                    });
                }
                return true;
            } else {
                //MonoBehaviour.print("Spell not recognized!");
                return false;
            }
        } else {
            return false;
        }
    }
    public override void Update()
    {
        //GameManagerScript.print((protagonist ? "protagonist " : "antagonist ") + state.ToString());
        switch (state) {
            case State.idle: {
                if (CheckForTracingStart()) {
                    AlterState(State.tracing);
                }
                break;
            }
            case State.tracing: {
                if (CheckForTracingEnd()) {
                    Vector2 WorldPosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                    pattern.Add(WorldPosition);
                    if (RecognizeSpell()) {
                        AlterState(State.loading);
                    } else {
                        AlterState(State.idle);
                    }
                } else {
                    if (patternCounter.Get() == 0) {
                        if (pattern.Count == 0) {
                            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                            pattern.Add(worldPosition);
                            if (spellStepEmitter != null) {
                                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams() {
                                    position = new Vector3(pattern[pattern.Count - 1].x, pattern[pattern.Count - 1].y, -1)
                                };
                                spellStepEmitter.Emit(emitParams, 1);
                            }
                        } else {
                            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                            float distance = Vector2.Distance(pattern[pattern.Count - 1], worldPosition);
                            if (distance >= spellStepMinDistance) {
                                pattern.Add(worldPosition);
                                if (spellStepEmitter != null) {
                                    ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams() {
                                        position = new Vector3(pattern[pattern.Count - 1].x, pattern[pattern.Count - 1].y, -1)
                                    };
                                    spellStepEmitter.Emit(emitParams, 1);
                                }
                            }
                        }
                    }
                    if (particleCounter.Get() == 0) {
                        if (sparksEmitter != null) {
                            sparksEmitter.Emit(1);
                        }
                    }
                }
                break;
            }
            case State.loading: {
                if (TimerScript.HasPassed(loadingTimer)) {
                    AlterState(State.loaded);
                }
                break;
            }
            case State.loaded: {
                if (CheckForShot()) {
                    CalculateShot(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                    AlterState(State.idle);
                }
                break;
            }
        }
    }
    protected override void AlterState(State desiredState)
    {
        if (state != desiredState) {
            State previousState = state;
            state = desiredState;
            if (previousState == State.idle && state == State.tracing) {
                patternCounter.Reset();
                particleCounter.Reset();
                pattern.Clear();
                animator.SetFloat("wand_speed", 2f);
                animator.SetBool("wand_tracing", true);
            } else if (previousState == State.tracing && state == State.idle) {
                animator.SetFloat("wand_speed", 1f);
                animator.SetBool("wand_tracing", false);
            } else if (previousState == State.tracing && state == State.loading) {
                loadingTimer = TimerScript.MakeTimer(loadingTime);
                wand.PlaySingleSound(WandScript.SoundEffect.recognized);
                animator.SetFloat("wand_speed", 1f);
                animator.SetBool("wand_loading", true);
                animator.SetBool("wand_tracing", false);
            } else if (previousState == State.loading && state == State.loaded) {
                TimerScript.Remove(loadingTimer);
                animator.SetBool("wand_loading", false);
            } else if (previousState == State.loaded && state == State.idle) {
                TimerScript.Remove(cooldownTimer);
                cooldownTimer = TimerScript.MakeTimer(cooldownTime);
                wand.PlaySingleSound(WandScript.SoundEffect.shot);
                animator.SetTrigger("wand_shot");
                loadedSpell = null;
            }
            wand.ChangeSoundEffect(state);
        }
    }

    public PlayableCharacter(CharacterScript characterScript, WandScript wandScript, bool protagonist, ParticleSystem spellStepEmitter,
                             ParticleSystem sparksEmitter, float cooldownTime, float loadingTime, AudioClip hurtSound,
                             AudioSource soundSource)
    {
        character = characterScript;
        wand = wandScript;
        this.protagonist = protagonist;
        this.cooldownTime = cooldownTime;
        this.loadingTime = loadingTime;

        if (!rigidbodyList.ContainsKey(protagonist)) {
            rigidbodyList.Add(protagonist, new List<Rigidbody2D>());
        }
        rigidbodyList[protagonist].Add(character.GetComponent<Rigidbody2D>());
        takenSpells = new Dictionary<Spell, int>();
        this.spellStepEmitter = spellStepEmitter;
        animator = characterScript.GetComponent<Animator>();
        this.sparksEmitter = sparksEmitter;
        pattern = new List<Vector2>();
        this.hurtSound = hurtSound;
        this.soundSource = soundSource;

        cooldownTimer = TimerScript.MakeTimer(0f);
        patternCounter = new CounterScript(0, 3, 1);
        particleCounter = new CounterScript(0, 3, 1);

        if (patternList.Count == 0) {
            LoadPatternDictionary();
        }
        if (checkedPatternList.Count == 0) {
            LoadCheckedPatternDictionary();
        }
    }
}
