using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { idle, tracing, loading, loaded };
public enum Spell { pudding, sleep, flies, frog, whirls, darkness };

public abstract class Character
{
    protected static Dictionary<bool, List<Collider2D>> colliderList = new Dictionary<bool, List<Collider2D>>();
    
    protected bool protagonist;
    protected CharacterScript character;
    protected WandScript wand;
    protected ParticleSystem spellStepEmitter;
    protected float cooldownTime;
    protected int cooldownTimer;
    protected float loadingTime;
    protected int loadingTimer;
    protected Animator animator;

    protected Dictionary<Spell, int> takenSpells;
    protected State state = State.idle;
    private Spell? loadedSpell = null;

    protected abstract bool CheckForTracingStart();
    protected abstract bool CheckForTracingEnd();
    protected abstract bool CheckForShot();
    protected abstract void CalculateShot(Vector2 MousePosition);
    protected abstract bool RecognizeSpell();
    public abstract void Update();
    protected abstract void AlterState(State desiredState);
}

public class NonPlayableCharacter : Character
{
    private float tracingTime;
    private int tracingTimer;

    private Spell? tracedSpell = null;

    protected override bool CheckForTracingStart()
    {
        return TimerScript.HasPassed(cooldownTimer);
    }
    protected override bool CheckForTracingEnd()
    {
        return TimerScript.HasPassed(tracingTimer);
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
        GameManagerScript.print((protagonist ? "protagonist " : "antagonist ") + state.ToString());
        switch (state) {
            case State.idle: {
                if (CheckForTracingStart()) {
                    AlterState(State.tracing);
                }
                break;
            }
            case State.tracing: {
                if (CheckForTracingEnd()) {
                    AlterState(State.loading);
                }
                //take point from spell table
                //emit particle at that point
                //continue until whole spell is traced
                //if (spellStepEmitter != null) {
                //    ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                //    emitParams.position = new Vector3(/*point position*/);
                //    spellStepEmitter.Emit(emitParams, 1);
                //}
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
                    //CalculateShot(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                    //take first enemy from list and harm it
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
            } else if (previousState == State.tracing && state == State.idle) {
                animator.SetFloat("wand_speed", 1f);
            } else if (previousState == State.tracing && state == State.loading) {
                loadingTimer = TimerScript.MakeTimer(loadingTime);
                wand.PlaySingleSound(WandScript.SoundEffect.recognized);
                animator.SetFloat("wand_speed", 1f);
            } else if (previousState == State.loading && state == State.loaded) {
                TimerScript.Remove(loadingTimer);
                TimerScript.Remove(cooldownTimer);
                cooldownTimer = TimerScript.MakeTimer(cooldownTime);
            } else if (previousState == State.loaded && state == State.idle) {
                TimerScript.Remove(cooldownTimer);
                cooldownTimer = TimerScript.MakeTimer(cooldownTime);
                wand.PlaySingleSound(WandScript.SoundEffect.shot);
                //animator.SetBool("wand_shot", true);
                animator.SetTrigger("wand_shot");
            }
            wand.ChangeSpriteAndSoundEffect(state);
        }
    }

    public NonPlayableCharacter(CharacterScript characterScript, WandScript wandScript, bool protagonist, ParticleSystem spellStepEmitter, float cooldownTime, float tracingTime, float loadingTime)
    {
        character = characterScript;
        wand = wandScript;
        this.protagonist = protagonist;
        this.cooldownTime = cooldownTime;
        this.tracingTime = tracingTime;
        this.loadingTime = loadingTime;

        if (!colliderList.ContainsKey(protagonist)) {
            colliderList.Add(protagonist, new List<Collider2D>());
        }
        colliderList[protagonist].Add(character.GetComponent<Collider2D>());
        takenSpells = new Dictionary<Spell, int>();
        this.spellStepEmitter = spellStepEmitter;
        animator = characterScript.GetComponent<Animator>();

        cooldownTimer = TimerScript.MakeTimer(0f);
    }
}

public class PlayableCharacter : Character
{
    protected ParticleSystem sparksEmitter;
    private readonly float spellStepMinDistance = 0.25f;

    private static List<Vector2> pattern;
    private CounterScript patternCounter,
                          particleCounter;

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
        //print(ClickHit.collider);
        if (colliderList[!protagonist].Contains(ClickHit.collider)) {
            //print("Bingo");
            //EnemyScript.GetHurt();
        } else {
            //print("Fail");
        }
    }
    protected override bool RecognizeSpell()
    {
        List<Vector2> processedPattern = new List<Vector2>(pattern);
        //print(processedPattern);
        if (spellStepEmitter != null) {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            spellStepEmitter.Clear();
            processedPattern.ForEach(delegate (Vector2 Position) {
                emitParams.position = new Vector3(Position.x, Position.y, -1f);
                emitParams.startColor = new Color32(0, 255, 0, 255);
                emitParams.startLifetime = 1f;
                spellStepEmitter.Emit(emitParams, 1);
                sparksEmitter.Emit(emitParams, 1);
                //print(EmitParams.position);
            });
        }
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

            } else if (previousState == State.tracing && state == State.idle) {
                ;
            } else if (previousState == State.tracing && state == State.loading) {
                loadingTimer = TimerScript.MakeTimer(loadingTime);
                wand.PlaySingleSound(WandScript.SoundEffect.recognized);
            } else if (previousState == State.loading && state == State.loaded) {
                TimerScript.Remove(loadingTimer);
            } else if (previousState == State.loaded && state == State.idle) {
                TimerScript.Remove(cooldownTimer);
                cooldownTimer = TimerScript.MakeTimer(cooldownTime);
                wand.PlaySingleSound(WandScript.SoundEffect.shot);
            }
            wand.ChangeSpriteAndSoundEffect(state);
        }
    }

    public PlayableCharacter(CharacterScript characterScript, WandScript wandScript, bool protagonist, ParticleSystem spellStepEmitter, ParticleSystem sparksEmitter, float cooldownTime, float loadingTime)
    {
        character = characterScript;
        wand = wandScript;
        this.protagonist = protagonist;
        this.cooldownTime = cooldownTime;
        this.loadingTime = loadingTime;

        if (!colliderList.ContainsKey(protagonist)) {
            colliderList.Add(protagonist, new List<Collider2D>());
        }
        colliderList[protagonist].Add(character.GetComponent<Collider2D>());
        takenSpells = new Dictionary<Spell, int>();
        this.spellStepEmitter = spellStepEmitter;
        animator = characterScript.GetComponent<Animator>();
        this.sparksEmitter = sparksEmitter;
        pattern = new List<Vector2>();

        cooldownTimer = TimerScript.MakeTimer(0f);
        patternCounter = new CounterScript(0, 3, 1);
        particleCounter = new CounterScript(0, 3, 1);
    }
}
