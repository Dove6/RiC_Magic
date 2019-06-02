using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandScript : MonoBehaviour
{
    public enum SoundEffect { tracing, recognized, loading, loaded, shot };
    private SoundEffect? activeSoundEffect;

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
    
    private Transform transform;
    private SpriteRenderer spriteRenderer;
    private int soundTimer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform = GetComponent<Transform>();
        soundTimer = TimerScript.MakeTimer(0f);
        activeSoundEffect = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PlayLoopedSound();
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

    public void ChangeSoundEffect(State state)
    {
        activeSoundEffect = StateToSoundEffect(state);
    }

    public void ChangeSoundEffect(SoundEffect? soundEffect)
    {
        activeSoundEffect = soundEffect;
    }

    public void PlaySingleSound(SoundEffect soundEffect)
    {
        soundSource.PlayOneShot(EnumToAudioClip(soundEffect));
    }
}
