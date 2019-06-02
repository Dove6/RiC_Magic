using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandScript : MonoBehaviour
{
    public enum SoundEffect { tracing, recognized, loading, loaded, shot };
    private SoundEffect? activeSoundEffect;

    [SerializeField]
    private Sprite vanillaWand,
                   tracingWand,
                   loadingWand,
                   loadedWand;

    [SerializeField]
    private AudioClip tracingSound,
                      recognizedSound,
                      loadingSound,
                      loadedSound,
                      shotSound;

    [SerializeField]
    private float tracingSoundLength = 0.2f,
                  loadingSoundLength = 0.8f,
                  loadedSoundLength = 0.8f;

    [SerializeField]
    private AudioSource soundSource;
    
    private Transform wandHandle,
                      transform;
    private SpriteRenderer spriteRenderer;
    private int rotationTimer,
                soundTimer;
    private CounterScript rotationCounter;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform = GetComponent<Transform>();
        rotationTimer = TimerScript.MakeTimer(20);
        soundTimer = TimerScript.MakeTimer(0f);
        rotationCounter = new CounterScript(0, 9, 1, (spriteRenderer.flipX ? 1 : 6));
        wandHandle = transform.GetChild(0);
        activeSoundEffect = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PlayLoopedSound();
        
        if (TimerScript.HasPassed(rotationTimer)) {
            TimerScript.Remove(rotationTimer);
            rotationTimer = TimerScript.MakeTimer(20);
            transform.RotateAround(wandHandle.position, new Vector3(0, 0, 1), 5 * ((rotationCounter.Get() > 4) ? -1 : 1));
        }
    }

    private Sprite EnumToSprite(State state)
    {
        switch (state) {
            case State.idle: {
                return vanillaWand;
            }
            case State.tracing: {
                return tracingWand;
            }
            case State.loading: {
                return loadingWand;
            }
            case State.loaded: {
                return loadedWand;
            }
            default: {
                return null;
            }
        }
    }

    private AudioClip EnumToAudioClip(SoundEffect soundEffect)
    {
        switch (soundEffect) {
            case SoundEffect.tracing: {
                return tracingSound;
            }
            case SoundEffect.recognized: {
                return recognizedSound;
            }
            case SoundEffect.loading: {
                return loadingSound;
            }
            case SoundEffect.loaded: {
                return loadedSound;
            }
            case SoundEffect.shot: {
                return shotSound;
            }
            default: {
                return null;
            }
        }
    }

    private float EnumToSoundLength(SoundEffect soundEffect)
    {
        switch (soundEffect) {
            case SoundEffect.tracing: {
                return tracingSoundLength;
            }
            case SoundEffect.loading: {
                return loadingSoundLength;
            }
            case SoundEffect.loaded: {
                return loadedSoundLength;
            }
            default: {
                return 0f;
            }
        }
    }

    private SoundEffect? StateToSoundEffect(State state)
    {
        switch (state) {
            case State.tracing: {
                return SoundEffect.tracing;
            }
            case State.loading: {
                return SoundEffect.loading;
            }
            case State.loaded: {
                return SoundEffect.loaded;
            }
            default: {
                return null;
            }
        }
    }

    private void PlayLoopedSound()
    {
        if (TimerScript.HasPassed(soundTimer)) {
            TimerScript.Remove(soundTimer);

            if (activeSoundEffect != null) {
                AudioClip loopedClip = EnumToAudioClip(activeSoundEffect.Value);
                soundTimer = TimerScript.MakeTimer(EnumToSoundLength(activeSoundEffect.Value));
                soundSource.PlayOneShot(loopedClip);
            }
        }
    }

    public void ChangeSprite(State state)
    {
        spriteRenderer.sprite = EnumToSprite(state);
    }

    public void ChangeSoundEffect(State state)
    {
        activeSoundEffect = StateToSoundEffect(state);
    }

    public void ChangeSoundEffect(SoundEffect? soundEffect)
    {
        activeSoundEffect = soundEffect;
    }

    public void ChangeSpriteAndSoundEffect(State state)
    {
        spriteRenderer.sprite = EnumToSprite(state);
        activeSoundEffect = StateToSoundEffect(state);
    }

    public void PlaySingleSound(SoundEffect soundEffect)
    {
        soundSource.PlayOneShot(EnumToAudioClip(soundEffect));
    }
}
