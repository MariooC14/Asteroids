using UnityEngine;

/// <summary>
/// Bullet script to handle bullet movement and collision with asteroids
/// </summary>
public class Bullet : MonoBehaviour
{
    public float movementSpeed = 1500f;

    // Start is called before the first frame update
    private void Start()
    {
        // Make it move forward
        GetComponent<Rigidbody>().AddForce(transform.forward * movementSpeed);

        // Check for offscreen
        InvokeRepeating(nameof(CheckForOffBounds), 0.2f, 0.2f);
    }

    /// <summary>
    ///  Destroy self if collided with bullet
    ///  Do not destroy on collide with spaceship as it can cause the bullet to destroy when shoting
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid")) {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Periodically check if the bullet is offscreen and destroy it if so
    /// </summary>
    private void CheckForOffBounds()
    {
        var posOnScreen = Camera.main.WorldToViewportPoint(transform.position);

        // Destroy bullet if it's offscreen
        if (posOnScreen.x < 0 || posOnScreen.x > 1 || posOnScreen.y < 0 || posOnScreen.y > 1)
        {
            Destroy(gameObject);
        }
    }
}
