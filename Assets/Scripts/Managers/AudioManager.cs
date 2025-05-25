using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The different available audio clips to play (must be added in the inspector)
/// </summary>
public enum AudioFX
{
    ASTEROID_BULLET,
    ASTEROID_SPACESHIP,
    SPACESHIP_SHOOT,
    POWERUP_PICKUP,
}

/// <summary>
/// Manager responsible for playing audio.
/// </summary>
public class AudioManager : BaseManager
{
    public static AudioManager instance;

    // Audio clips
    public AudioClip asteroidBulletClip;
    public AudioClip asteroidSpaceshipClip;
    public AudioClip spaceshipBulletClip;
    public AudioClip powerupPickupClip;

    // volume slider in options panel
    public Slider volumeSlider;

    AudioSource source;
    float volume = 0.5f;

    void Start()
    {
        SetUp();
    }

    public override void SetUp()
    {
        instance = this;
        source = gameObject.AddComponent<AudioSource>();
        source.rolloffMode = AudioRolloffMode.Custom;
        source.volume = volume;

        // Listen for volume slider changing in the options panel.
        if (volumeSlider)
        {
            volumeSlider.value = volume;
            volumeSlider.onValueChanged.AddListener(delegate
            {
                volume = volumeSlider.value;
            });
        }
        base.SetUp();
    }

    /// <summary>
    /// Plays a once off audio clip at the camera
    /// </summary>
    /// <param name="clip">The clip to play</param>
    public void PlayAudioFX(AudioFX clip)
    {
        var clipToPlay = clip switch
        {
            AudioFX.ASTEROID_BULLET => asteroidBulletClip,
            AudioFX.ASTEROID_SPACESHIP => asteroidSpaceshipClip,
            AudioFX.SPACESHIP_SHOOT => spaceshipBulletClip,
            AudioFX.POWERUP_PICKUP => powerupPickupClip,
            _ => null
        };

        if (clipToPlay && source)
            source.PlayOneShot(clipToPlay);
    }

    /// <summary>
    /// Updates the volume of the audio source.
    /// This is called when the mouse cursor is released on the volume slider in the options panel.
    /// This is done instead of updating the source volume with onValueChange to avoid overplaying the test sound
    /// </summary>
    public void UpdateVolume()
    {
        source.volume = volume;
        PlayAudioFX(AudioFX.POWERUP_PICKUP);
    }
}