using UnityEngine;

/// <summary>
/// Script that makes a game object self destroy after a set amount of time
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    public float lifeTime = 3f;

    private void Start()
    {
        // Destroy gameObject after lifetime
        Destroy(gameObject, lifeTime);
    }
}
