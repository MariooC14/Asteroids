using UnityEngine;

/// <summary>
/// Script responsible for handling an asteroid instance
/// </summary>
public class Asteroid : MonoBehaviour
{
    // Prefabs
    public GameObject prefabAsteroidParticle;
    public GameObject prefabAsteroid;
    public GameObject bigAsteroidExplosionParticle;
    public GameObject smallAsteroidExplosionParticle;

    public float speed;
    private Rigidbody rb;

    // the number of asteroid particles to spawn on a collision
    public int numAsteroidParticles = 3;

    // Used for handling whether an asteroid should break up into smaller asteroids or fully destroy
    public bool isSmall;

    // used for handling multiple collisions with the same bullet
    private bool hasCollidedWithBullet;

    // Score for destroying an asteroid, for both small and large asteroids
    public int collisionScore = 10;

    // Start is called before the first frame update
    private void Start()
    {
        speed = 300 + Random.value * 300;
        rb = GetComponent<Rigidbody>();
        rb.AddTorque(RandomUtils.RandomVec3(10, 50));

        // Freshly spawned asteroids that are spawned at the edge are set to move somewhat towards the center
        if (!isSmall)
        {
            // Move the asteroid towards the center plus some offset.
            var offset = RandomUtils.RandomVec3(-50, 50);
            rb.AddForce((-transform.position + offset).normalized * speed);
        }
        // Asteroids spawned from an asteroid being shot have a random direction and their size is halved
        else
        {
            transform.localScale = transform.localScale / 2;
            rb.AddForce(RandomUtils.RandomVec3(-50, 50).normalized * speed);
        }

        InvokeRepeating(nameof(CheckForEdge), 2f, 0.2f);
    }

    /// <summary>
    /// Destroys the asteroid if it hits a bullet creates 
    /// a number of particles on collision with another asteroid
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // Check for bullet or player collision. Make sure it doesn't collide with the same bullet multiple times.
        if (collision.gameObject.CompareTag("Bullet") && !hasCollidedWithBullet) 
        {
            hasCollidedWithBullet = true;
            DestroyAsteroid(collision.contacts[0].point);
        }
        else
        {
            // Show some particles on collision with another asteroid
            CreateAsteroidParticles(collision.contacts[0].point);
        }
    }

    /// <summary>
    /// Destroys an asteroid through some collision, showing some particles and adding score.
    /// </summary>
    /// <param name="collisionPoint"></param>
    public void DestroyAsteroid(Vector3 collisionPoint)
    {
        if (isSmall)
        {
            CreateSmallAsteroidExplosion(collisionPoint);
        }
        else
        {
            // Spawn a few small particles on full destroy
            CreateBigAsteroidExplosion(collisionPoint);
            AsteroidManager.instance.SpawnMiniAsteroidsAt(transform.position, 4);
        }

        // Add score for destroying (not called in OnDestroy as this could mess with the score
        // if the Asteroid manager destroys it on cleanup).
        ScoreManager.instance.AddScore(collisionScore, collisionPoint);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        AudioManager.instance.PlayAudioFX(AudioFX.ASTEROID_BULLET);
        GameManager.instance.ShakeCamera();
        AsteroidManager.instance.RemoveAsteroid(gameObject);
    }


    /// <summary>
    /// Creates a number of asteroid particles at a given position
    /// </summary>
    /// <param name="point">the point to spawn particles at</param>
    private void CreateAsteroidParticles(Vector3 point)
    {
        for (var i = 0; i < numAsteroidParticles; i++)
        {
            var asteroidParticle = Instantiate(prefabAsteroidParticle);
            asteroidParticle.transform.position = point;
        }
    }

    /// <summary>
    /// Spawns a big asteroid explosion at a given point.
    /// The explosion is a particle effect that auto self destroys
    /// </summary>
    /// <param name="point"></param>
    private void CreateBigAsteroidExplosion(Vector3 point)
    {
        Instantiate(bigAsteroidExplosionParticle, point, Quaternion.identity);
    }

    /// <summary>
    /// Spawns a small asteroid explosion at a given point
    /// The explosion is a particle effect that auto self destroys
    /// </summary>
    /// <param name="point"></param>
    private void CreateSmallAsteroidExplosion(Vector3 point)
    {
        Instantiate(smallAsteroidExplosionParticle, point, Quaternion.identity);
    }

    /// <summary>
    /// Checks for the asteroid being off the camera bounds and moves it to the opposite side if it is.
    /// </summary>
    private void CheckForEdge()
    {
        var posOnScreen = Camera.main.WorldToViewportPoint(transform.position);

        // If the asteroid has moved offscreen, move it to the opposite side of the screen. Keep same velocity
        if (posOnScreen.x < -0.1)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(1, 1 - posOnScreen.y, 30));
        }
        else if (posOnScreen.x > 1.1)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0, 1 - posOnScreen.y, 30));
        }

        if (posOnScreen.y < -0.1)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(1 - posOnScreen.x, 1, 30));
        }
        else if (posOnScreen.y > 1.1)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(1 - posOnScreen.x, 0, 30));
        }
    }
}