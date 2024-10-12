using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(MechBehavior))]
public class MechAudioManager : MonoBehaviour
{
    [Tooltip("The audio effects \"collection\" to be played when a Mech starts walking and at footfall intervals")]
    [SerializeField] private SFXCollection mechstepsSFX;

    [Tooltip("The audio effects \"collection\" to be played when a Mech fires a weapon")]
    [SerializeField] private SFXCollection weaponFireSFX;

    [Tooltip("The audio effects \"collection\" to be played when a Mech finishes walking (not being used rn, didn't sound right)")]
    [SerializeField] private SFXCollection endMechstepsSFX;

    [Tooltip("The Mixer Group as defined in MainMixer that mech SFX effects should be played through")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    // internal component references
    private AudioSource _footstepSource;
    private AudioSource _gunfireSource;
    private MechBehavior _mechBehaviour;

    /// <summary>
    /// The time since the mechs last footfall while moving
    /// </summary>
    private float _secondsMoving;

    [Tooltip("The interval while a mech is moving in which a footfall SFX is triggered")]
    [SerializeField] private float _secondsBetweenFootfalls;

    // region sound boxes
    // rect-collider region -> region-triggered sound
    // [SerializeField] private Sound
    private void Awake()
    {
        _footstepSource = gameObject.AddComponent<AudioSource>();
        _footstepSource.spatialBlend = 1f;
        _footstepSource.outputAudioMixerGroup = sfxMixerGroup;
        _gunfireSource = gameObject.AddComponent<AudioSource>();
        _gunfireSource.spatialBlend = 1f;
        _gunfireSource.outputAudioMixerGroup = sfxMixerGroup;
        _mechBehaviour = gameObject.GetComponent<MechBehavior>();
        _mechBehaviour.stoppedMoving += OnStoppedMoving;
        _mechBehaviour.startedMoving += OnStartedMoving;
        _mechBehaviour.weaponFired += OnGunfire;
    }

    private void Update()
    {
        if (_mechBehaviour.IsMoving) _secondsMoving += Time.deltaTime;
        if (_secondsMoving >= _secondsBetweenFootfalls)
        {
            _secondsMoving = 0f;
            PlayFromSFXCollection(_footstepSource, mechstepsSFX, false);
        }
    }

    private void OnStoppedMoving()
    {
        // PlayFromSFXCollection(_footstepSource, endMechstepsSFX, true);
        _secondsMoving = 0f;
    }

    private void OnStartedMoving()
    {
       PlayFromSFXCollection(_footstepSource, mechstepsSFX, true);
    }

    private void OnGunfire()
    {
       PlayFromSFXCollection(_gunfireSource, weaponFireSFX, false);
    }

    private void PlayFromSFXCollection(AudioSource source, SFXCollection sfx, bool stopSourceFirst)
    {
        if (stopSourceFirst) source.Stop();
        if (sfx.audioClips.Length == 0) return;
        var audio_clip = sfx.audioClips[Random.Range(0, sfx.audioClips.Length-1)];
        float audio_volume = Random.Range(1f - sfx.deviationVolume, 1f);
        source.pitch = Random.Range(1f - sfx.deviationPitch, 1f + sfx.deviationPitch);
        source.PlayOneShot(audio_clip, audio_volume);
    }
    // a looooot of event functions
}

/// <summary>
/// A struct for localizing audio-clips together into an SFX "package" that can be treated as one sound effect
/// </summary>
[Tooltip("A struct for localizing audio-clips together into an SFX \"package\" that can be treated as one sound effect")]
[System.Serializable] 
public struct SFXCollection
{
    [Tooltip("All the sound files that will be used for this SFX")]
    public AudioClip[] audioClips;

    [Tooltip("The deviation from max volume this clip can have when played (1-volumeDeviation -> 1)")]
    [Range(0f, .25f)] public float deviationVolume;

    [Tooltip("The deviation from normal pitch this clip can have when played (1-pitchDeviation -> 1+pitchDeviation)")]
    [Range(0f, .125f)] public float deviationPitch;
}
