using System.Collections;
using UnityEngine;

/// <summary>
/// Represents the game object that the player can collide with to get a powerup.
/// </summary>
class PowerupBubble : MonoBehaviour
{
    public Powerup powerup;
    float lifetime = 10;

    private Renderer[] _renderers;

    private void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        StartCoroutine(AutoSelfDestroy());
        StartCoroutine(Pulse());
    }

    /// <summary>
    /// Checks for collision with the player so it can apply itself
    /// </summary>
    /// <param name="other">the other collider</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PowerupManager.instance.ApplyPowerup(powerup);
            AudioManager.instance.PlayAudioFX(AudioFX.POWERUP_PICKUP);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        PowerupManager.instance.RemovePowerupBubble(gameObject);
    }

    /// <summary>
    /// Animation Routine that scales the bubble up and down
    /// </summary>
    /// <returns></returns>
    private IEnumerator Pulse()
    {
        float scale = 2;
        float time = 0; 

        while (true)
        {
            time += 10f * Time.deltaTime;
            scale = 2 + Mathf.Sin(time) / 2;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine that destroys the bubble after a set amount of time
    /// </summary>
    /// <returns></returns>
    private IEnumerator AutoSelfDestroy()
    {
        yield return new WaitForSeconds(lifetime / 3);
        var animationTime = lifetime / 3;
        const float flashInterval = 0.25f;

        // Flash the bubble until it's destroyed
        while (animationTime > 0)
        {
            foreach (var renderer in _renderers)
            {
                renderer.enabled = !renderer.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
            animationTime -= flashInterval;
        }

        Destroy(gameObject);
    }
}

