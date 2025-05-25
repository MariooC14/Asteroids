using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Script that controls the spaceship, almost like a manager
/// </summary>
public class SpaceshipController : MonoBehaviour
{
    // Inspector stuff
    public GameObject bulletPrefab;
    public GameObject invulnerabilityShield;
    public ParticleSystem leftFlameParticles;
    public ParticleSystem rightFlameParticles;

    public float movementSpeed = 20;
    public float maxSpeed = 15;
    public float torque;
    private Rigidbody rb;

    // Player is invulnerable after spawning for 2s
    public float gracePeriod = 2f;

    [Description("Fire rate per second")] 
    public float defaultFireRate = 4;
    [Description("The spread of bullets for triple bullet powerup")]
    public float bulletSpread = 20;
    //

    // Used for handling fire rate
    private bool allowFire = true;
    private bool isInvulnerable;
    bool[] activePowerups;

    private Vector3 currentVelocity;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        torque = 50f;
        rb.angularDamping = 5f;

        activePowerups = new bool[(int)PowerupType.NUM_POWERUPS];

        // Limit speed
        rb.maxLinearVelocity = maxSpeed;

        InvokeRepeating(nameof(CheckForEdge), 0.5f, 0.2f);
        leftFlameParticles.Stop();
        rightFlameParticles.Stop();
    }

    /// <summary>
    /// Makes the spaceship invulnerable for a grace period after spawning
    /// </summary>
    private void OnEnable()
    {
        isInvulnerable = true;
        // enable allow fire in case the shoot coroutine got interrupted
        allowFire = true;
        invulnerabilityShield.SetActive(true);
        StartCoroutine(HandleGracePeriodRoutine());
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector3 forceToApply = Vector3.zero;

        // Move spaceship forward and show flame effects
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            forceToApply = movementSpeed * transform.forward;
            leftFlameParticles.Play();
            rightFlameParticles.Play();
        }

        // Move spaceship backwards
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            forceToApply = movementSpeed * -transform.forward;

        rb.AddForce(forceToApply);
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

        // Make ship look at mouse position
        var mousePos = Input.mousePosition;
        mousePos.z = CameraController.CAMERA_HEIGHT;
        var lookAt = Camera.main.ScreenToWorldPoint(mousePos);
        transform.LookAt(lookAt);

        // Shoot bullets every x seconds
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && allowFire)
        {
            StartCoroutine(Shoot());
        }
    }

    /// <summary>
    /// Handles collision with asteroids. If the spaceship is not invulnerable, it gets hurt.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid") && !isInvulnerable)
        {
            // The spaceship can make a sacrifice and use itself for destroying an asteroid.
            // Maybe this can become an achievement "The Ultimate Sacrifice"
            var asteroid = collision.gameObject.GetComponent<Asteroid>();
            asteroid.DestroyAsteroid(collision.contacts[0].point);

            AudioManager.instance.PlayAudioFX(AudioFX.ASTEROID_SPACESHIP);
            GameManager.instance.HurtPlayerSpaceship();
        }
    }

    // Teleports spaceship from one edge of the screen to the other
    private void CheckForEdge()
    {
        var posOnScreen = Camera.main.WorldToViewportPoint(transform.position);

        if (posOnScreen.x < 0) posOnScreen.x = 1;
        else if (posOnScreen.x > 1) posOnScreen.x = 0;

        if (posOnScreen.y < 0) posOnScreen.y = 1;
        else if (posOnScreen.y > 1) posOnScreen.y = 0;

        transform.position = Camera.main.ViewportToWorldPoint(posOnScreen);
    }

    // Default shooting method; single bullet
    private void ShootSingleBullet()
    {
        var bullet = Instantiate(bulletPrefab);
        // Give reference to spaceship so the bullet can set its position and rotation.

        // Make it look in the same direction as the ship, by looking at its y rotation
        bullet.transform.SetPositionAndRotation(
            transform.position + transform.forward * 2,
            Quaternion.Euler(0, transform.eulerAngles.y, 0)
        );
    }


    /// <summary>
    /// Shoots three bullets in a spread pattern.
    /// </summary>
    private void ShootTripleBullet()
    {
        // Used for adding some spacing between bullets initially
        var rightOffset = -0.25f;
        // Used for making each bullet point in a different direction, like a cone.
        var angle = -bulletSpread;
        var frontOfShipPos = transform.position + transform.forward * 2;
       
        for (int i = 0; i < 3; i++)
        {
            // Create the bullet and update its transform
            var bullet = Instantiate(bulletPrefab);
            bullet.transform.SetLocalPositionAndRotation(
                frontOfShipPos + transform.right * rightOffset,
                Quaternion.Euler(0, transform.eulerAngles.y + angle, 0)
                );
            // Increase the right offset and angle for the next bullet
            rightOffset += 0.25f;
            angle += bulletSpread;
        }
    }


    /// <summary>
    /// Shoots a bullet every 1/bulletFireRate seconds. 
    /// The type of bullet is shot depending on the active powerups.
    /// Adapted from https://stackoverflow.com/questions/72972653/setting-a-fire-rate-for-a-weapon-in-unity
    /// </summary>
    /// <returns>IEnumerator (coroutine)</returns>
    private IEnumerator Shoot()
    {
        allowFire = false;
        var fireRate = activePowerups[(int) PowerupType.MINIGUN] ? defaultFireRate * 10 : defaultFireRate;

        AudioManager.instance.PlayAudioFX(AudioFX.SPACESHIP_SHOOT);

        if (activePowerups[(int)PowerupType.TRIPLE_SHOT])
        {
            ShootTripleBullet();
        }
        else
        {
            ShootSingleBullet();
        }

        yield return new WaitForSeconds(1 / fireRate);
        allowFire = true;
    }

    /// <summary>
    /// Routine taht makes the spaceship vulnerable after the grace period ends
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleGracePeriodRoutine()
    {
        yield return new WaitForSeconds(gracePeriod / 2);
        var animationTime = gracePeriod / 2;
        const float pulseInterval = 0.125f;

        // Flash the shield until end of grace period
        while (animationTime > 0)
        {
            invulnerabilityShield.SetActive(!invulnerabilityShield.activeSelf);
            yield return new WaitForSeconds(pulseInterval);
            animationTime -= pulseInterval;
        }

        invulnerabilityShield.SetActive(false);
        isInvulnerable = false;
    }

    /// <summary>
    /// Applies a powerup to the player
    /// </summary>
    /// <param name="powerup"></param>
    public void ApplyPowerup(Powerup powerup) => activePowerups[(int)powerup.type] = true;

    /// <summary>
    /// Removes a powerup from the player
    /// </summary>
    /// <param name="powerup"></param>
    public void RemovePowerup(Powerup powerup) => activePowerups[(int)powerup.type] = false;
}
