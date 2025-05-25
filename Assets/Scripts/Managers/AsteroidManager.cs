using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the spawning and destruction of asteroids
/// </summary>
public class AsteroidManager : BaseManager
{
    public static AsteroidManager instance;
    public GameObject asteroidPrefab;
    Camera mainCam;

    int numAsteroidsToSpawn;

    LinkedList<GameObject> asteroids;

    // Start is called before the first frame update
    private void Start()
    {
       SetUp();
    }

    // Manager set up
    public override void SetUp()
    {
        instance = this;
        mainCam = Camera.main;
        asteroids = new LinkedList<GameObject>();
        base.SetUp();
    }

    /// <summary>
    /// Starts spawning asteroids based on the game's level
    /// </summary>
    public void StartSpawningAsteroids()
    {
        numAsteroidsToSpawn = (int)Mathf.Floor(GameManager.instance.currentGameLevel * 1.5f);
        StartCoroutine(SpawnAsteroids());
    }

    /// <summary>
    /// Stops spawning asteroids
    /// </summary>
    public void StopSpawningAsteroids()
    {
        StopCoroutine(SpawnAsteroids());
    }

    /// <summary>
    /// Spawns a number of asteroids overtime. An asteroid is spawned every second.
    /// </summary>
    IEnumerator SpawnAsteroids()
    {
        var waitTime = new WaitForSeconds(1);
        for (var i = 0; i < numAsteroidsToSpawn; i++)
        {
            var asteroid = Instantiate(asteroidPrefab);
            asteroids.AddLast(asteroid);

            var spawnPointOnLine = Random.value;
            // Choose the side of the viewport to spawn it from
            // 0-1 is left edge, 1-2 is top edge, 2-3 is right edge, 3-4 is bottom edge
            var side = Random.value * 4;
            asteroid.transform.position = side switch
            {
                < 1 => mainCam.ViewportToWorldPoint(new Vector3(0, spawnPointOnLine, CameraController.CAMERA_HEIGHT)),
                < 2 => mainCam.ViewportToWorldPoint(new Vector3(spawnPointOnLine, 1, CameraController.CAMERA_HEIGHT)),
                < 3 => mainCam.ViewportToWorldPoint(new Vector3(1, spawnPointOnLine, CameraController.CAMERA_HEIGHT)),
                _ => mainCam.ViewportToWorldPoint(new Vector3(spawnPointOnLine, 0, CameraController.CAMERA_HEIGHT))
            };
            yield return waitTime;
        }
    }

    /// <summary>
    /// Adds an asteroid to the list of spawned asteroids
    /// </summary>
    /// <param name="asteroid">the asteroid game object to add</param>
    public void AddAsteroid(GameObject asteroid)
    {
        asteroids.AddLast(asteroid);
    }

    /// <summary>
    /// Removes an asteroid from the list of spawned asteroids.
    /// Triggers the next wave if there are no more asteroids left.
    /// </summary>
    /// <param name="asteroid">The asteroid game object to remove</param>
    public void RemoveAsteroid(GameObject asteroid)
    {
        asteroids.Remove(asteroid);
        // Check if there are any asteroids left
        if (asteroids.First == null && GameManager.instance.stateMachine.GetState() == GameState.Playing)
        {
            // Start next wave
            GameManager.instance.ChangeState(GameState.RoundOver);
        }
    }

    /// <summary>
    /// Spawns a number of mini asteroids at a location, when a big asteroid is destroyed
    /// </summary>
    /// <param name="location">the location to spawn the mini asteroids at</param>
    /// <param name="amountToSpawn">The number of mini asteroids to spawn</param>
    public void SpawnMiniAsteroidsAt(Vector3 location, int amountToSpawn)
    {
        for (var i = 0; i < amountToSpawn; i++)
        {
            var asteroid = Instantiate(asteroidPrefab);
            asteroid.transform.position = location;
            asteroid.GetComponent<Asteroid>().isSmall = true;
            asteroids.AddLast(asteroid);
        }
    }

    /// <summary>
    /// Clears the arena of asteroids.
    /// </summary>
    public void ClearAsteroids()
    {
        foreach (var asteroid in asteroids)
        {
            Destroy(asteroid);
        }
        // We do not remove the asteroid from the list here as the asteroid's OnDestroy method does that
    }
}
