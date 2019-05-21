using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandScript : MonoBehaviour
{
    public enum State {empty, tracing, loading, loaded};
    public enum Holder {enemy, player};
    public enum Spell {pudding, sleep, flies, frog, whirls, darkness};

    private Dictionary<Spell, int> TakenSpells;

    [SerializeField]
    private Holder WandHolder = Holder.enemy;

    [SerializeField]
    private Sprite VanillaWand,
                   TracingWand,
                   LoadingWand,
                   LoadedWand;

    [SerializeField]
    private AudioClip TracingSound,
                      RecognitionSound,
                      LoadingSound,
                      LoadedSound,
                      ShootingSound;

    [SerializeField]
    private AudioSource SoundSource;

    [SerializeField]
    private float CooldownTime = 0.5f;
    [SerializeField]
    private float LoadingTime = 3f;

    private readonly float AITracingTime = 4f;
    private readonly float TracingSoundTime = 0.2f;
    private readonly float LoadingSoundTime = 0.8f;
    private readonly float LoadedSoundTime = 0.8f;

    State WandState = State.empty;
    private Spell? LoadedSpell = null;
    private SpriteRenderer RendererReference;
    private Transform TransformReference;
    private int RotationTimer,
                SoundTimer,
                LoadingTimer,
                CooldownTimer,
                AITracingTimer;
    private CounterScript RotationCounter;
    private Vector3 OriginalPosition;
    // Start is called before the first frame update
    void Start()
    {
        RendererReference = GetComponent<SpriteRenderer>();
        TransformReference = GetComponent<Transform>();
        RotationTimer = TimerScript.MakeTimer(20);
        SoundTimer = TimerScript.MakeTimer(0);
        CooldownTimer = TimerScript.MakeTimer(0f);
        RotationCounter = new CounterScript(0, 9, 1, (RendererReference.flipX ? 1 : 6));
        OriginalPosition = TransformReference.position;
        TakenSpells = new Dictionary<Spell, int>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //print(WandHolder + ": " + WandState + ", " + TransformReference.position);
        switch (WandState) {
            case State.empty: {
                if (CheckForTracingStart()) {
                    StateTransition(State.tracing);
                }
                break;
            }
            case State.tracing: {
                if (CheckForTracingEnd()) {
                    if (SpellRecognition()) {
                        StateTransition(State.loading);
                    } else {
                        StateTransition(State.empty);
                    }
                }
                break;
            }
            case State.loading:
            {
                if (TimerScript.HasPassed(LoadingTimer))
                {
                    StateTransition(State.loaded);
                }
                break;
            }
            case State.loaded: {
                if (CheckForShot()) {
                    ShotCalculation();
                    StateTransition(State.empty);
                }
                break;
            }
        }

        SoundsPlayer();
        
        if (TimerScript.HasPassed(RotationTimer)) {
            TimerScript.Remove(RotationTimer);
            RotationTimer = TimerScript.MakeTimer(20);
            TransformReference.RotateAround(TransformReference.position - 
                                            new Vector3(RendererReference.bounds.size.x / 2f, RendererReference.bounds.size.y / 2f, 0),
                                            new Vector3(1, 1, 1), 5 * ((RotationCounter.Get() > 4) ? -1 : 1));
        }

        if (TransformReference.rotation.eulerAngles.z > -5 && TransformReference.rotation.eulerAngles.z < 5) {
            TransformReference.position = OriginalPosition;
        }
    }

    private bool CheckForTracingStart()
    {
        if (TimerScript.HasPassed(CooldownTimer)) {
            switch (WandHolder) {
                case Holder.player: {
                    return Input.GetMouseButton(0);
                }
                case Holder.enemy: {
                    return true;
                }
                default: { //for future use?
                    return false;
                }
            }
        } else {
            return false;
        }
    }

    private bool CheckForTracingEnd()
    {
        switch (WandHolder) {
            case Holder.player: {
                return !Input.GetMouseButton(0);
            }
            case Holder.enemy: {
                if (TimerScript.HasPassed(AITracingTimer)) {
                    TimerScript.Remove(AITracingTimer);
                    return true;
                } else {
                    return false;
                }
            }
            default: { //for future use?
                return false;
            }
        }
    }

    private bool CheckForShot()
    {
        switch (WandHolder) {
            case Holder.player: {
                return Input.GetMouseButton(0);
            }
            case Holder.enemy: {
                return true;
            }
            default: { //for future use?
                return false;
            }
        }
    }

    private void ShotCalculation()
    {
        ;
    }

    private void SoundsPlayer()
    {
        if (TimerScript.HasPassed(SoundTimer)) {
            TimerScript.Remove(SoundTimer);
            switch (WandState) {
                case State.empty: {
                    break;
                }
                case State.tracing: {
                    SoundTimer = TimerScript.MakeTimer(TracingSoundTime);
                    SoundSource.PlayOneShot(TracingSound);
                    break;
                }
                case State.loading: {
                    SoundTimer = TimerScript.MakeTimer(LoadingSoundTime);
                    SoundSource.PlayOneShot(LoadingSound);
                    break;
                }
                case State.loaded: {
                    SoundTimer = TimerScript.MakeTimer(LoadedSoundTime);
                    SoundSource.PlayOneShot(LoadedSound);
                    break;
                }
            }
        }
    }

    private bool SpellRecognition()
    {
        return true;
    }

    private void StateTransition(State CurrentState)
    {
        State PreviousState = WandState;
        if (PreviousState != CurrentState) {
            WandState = CurrentState;
            if (PreviousState == State.empty && CurrentState == State.tracing) {
                if (WandHolder == Holder.enemy) {
                    AITracingTimer = TimerScript.MakeTimer(AITracingTime);
                }
            } else if (PreviousState == State.tracing && CurrentState == State.empty) {
                ;
            } else if (PreviousState == State.tracing && CurrentState == State.loading) {
                LoadingTimer = TimerScript.MakeTimer(LoadingTime);
                SoundSource.PlayOneShot(RecognitionSound);
            } else if (PreviousState == State.loading && CurrentState == State.loaded) {
                TimerScript.Remove(LoadingTimer);
            } else if (PreviousState == State.loaded && CurrentState == State.empty) {
                SoundSource.PlayOneShot(ShootingSound);
                TimerScript.Remove(CooldownTimer);
                CooldownTimer = TimerScript.MakeTimer(CooldownTime);
            }
            switch (CurrentState) {
                case State.empty: {
                    RendererReference.sprite = VanillaWand;
                    break;
                }
                case State.tracing: {
                    RendererReference.sprite = TracingWand;
                    break;
                }
                case State.loading: {
                    RendererReference.sprite = LoadingWand;
                    break;
                }
                case State.loaded: {
                    RendererReference.sprite = LoadedWand;
                    break;
                }
            }
        }
    }
}
