using UnityEngine;

/// <summary>
/// Script that gives the asteroid particle a random direction to move in
/// </summary>
public class AsteroidParticle : MonoBehaviour
{
    public float movementSpeed = 50f;

    // Start is called before the first frame update
    private void Start()
    {
        var direction = RandomUtils.RandomVec3(-10, 10);
        direction.y = 0;
        GetComponent<Rigidbody>().AddForce(direction * movementSpeed);
    }
}
