using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// The different types of powerups that can be spawned in the game
/// </summary>
public enum PowerupType
{
    TRIPLE_SHOT,
    EXTRA_LIFE,
    MINIGUN,
    NUM_POWERUPS
}

/// <summary>
/// Class used for managing powerups. It is responsible for spawning the bubbles, applying the 
/// powerup to the player (when a bubble collides with the player), and removing the powerup from the player
/// once the duration expired.
/// </summary>
public class PowerupManager : BaseManager
{
    public static PowerupManager instance;

    // Inspector stuff
    public GameObject powerupBubblePrefab;
    public GameObject tripleShotIconPrefab;
    public GameObject extraLifeIconPrefab;
    public GameObject minigunIconPrefab;

    public GameObject playerSpaceship;

    [Description("The default probability of spawning a powerup at any given second")]
    public float baseSpawnChance = 0.01f;
    //

    // List of powerups spawned on the map.
    private LinkedList<GameObject> powerupBubbles;
    // List of the powerups that the player could have. This is used for managing the duration left on 
    // each powerup
    Powerup[] powerups;

    private void Start()
    {
        SetUp();
    }

    // Manager set up
    public override void SetUp()
    {
        instance = this;
        powerupBubbles = new LinkedList<GameObject>();

        // Set up the list of powerup effects
        powerups = new Powerup[(int)PowerupType.NUM_POWERUPS];
        for (int i = 0; i < (int)PowerupType.NUM_POWERUPS; i++)
        {
            powerups[i] = new Powerup((PowerupType)i, 0);
            powerups[i].Active = false;
        }
        base.SetUp();
    }

    // Starts spawning powerups randomly, based on the base spawn chance.
    public void StartSpawningPowerupBubbles()
    {
        StartCoroutine(PowerupBubbleSpawnerRoutine());
        StartCoroutine(nameof(PowerupCountdownRoutine));
    }

    /// <summary>
    /// Periodically checks for the duration of powerups
    /// </summary>
    /// <returns></returns>
    IEnumerator PowerupCountdownRoutine()
    {
        var waitTime = new WaitForSeconds(1);

        while (true)
        {
            yield return waitTime;
            CheckPowerupDurations();
        }
    }

    /// <summary>
    /// Stops the spawning of powerups and clears the list of active powerups.
    /// </summary>
    public void StopSpawningPowerupBubbles()
    {
        StopAllCoroutines();
        ClearPowerupBubbles();
    }

    /// <summary>
    /// Routine that checks every second for the duration of the powerups. 
    /// If a powerup's duration is 0, it removes it from the player.
    /// </summary>
    /// <returns></returns>
    private void CheckPowerupDurations()
    {
        foreach (var powerup in powerups)
        {
            if (powerup.Active)
            {
                powerup.Duration -= 1;
                if (powerup.Duration <= 0)
                {
                    RemovePowerup(powerup);
                }
            }
        }
    }

    /// <summary>
    /// Coroutine for spawning asteroids. It rolls a die between 0 and 1, and if the roll
    /// is greater than or equal to the base spawn chance, it spawns a powerup.
    /// It does so every second, until the isSpawningPowerup flag is set to false.
    /// </summary>
    private IEnumerator PowerupBubbleSpawnerRoutine()
    {
        var waitPeriod = new WaitForSeconds(1);

        while (true)
        {
            var spawnChanceRoll = Random.Range(0f, 1f);
            if (spawnChanceRoll <= baseSpawnChance)
            {
                // Spawn powerup
                SpawnPowerupBubble();
            }
            yield return waitPeriod;
        }
    }

    /// <summary>
    /// Spawns a random powerup ubble at a random location in the arena
    /// </summary>
    private void SpawnPowerupBubble()
    {
        var newPowerupBubble = Instantiate(powerupBubblePrefab);

        // Choose at random the type of powerup to spawn
        PowerupType randomPowerupType = (PowerupType)Random.Range(0, (int)PowerupType.NUM_POWERUPS);

        Powerup powerupToAdd = new Powerup(randomPowerupType, 5);

        AddPowerupIcon(randomPowerupType, newPowerupBubble);

        newPowerupBubble.GetComponent<PowerupBubble>().powerup = powerupToAdd;

        powerupBubbles.AddLast(newPowerupBubble);
        var xPos = Random.Range(0.1f, 0.9f);
        var yPos = Random.Range(0.1f, 0.9f);

        newPowerupBubble.transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(xPos, yPos, CameraController.CAMERA_HEIGHT));
    }

    /// <summary>
    /// Applies a powerup to the player spaceship
    /// </summary>
    /// <param name="powerup">The powerup to apply</param>
    public void ApplyPowerup(Powerup powerup)
    {
        if (powerup.type == PowerupType.EXTRA_LIFE)
        {
            GameManager.instance.Lives++;
        }
        else
        {
            powerups[(int)powerup.type].Active = true;
            powerups[(int)powerup.type].Duration = powerup.Duration;
            playerSpaceship.GetComponent<SpaceshipController>().ApplyPowerup(powerup);
        }
    }

    /// <summary>
    /// Applies the appropriate icon to show inside the bubble based on the powerup type
    /// </summary>
    /// <param name="type">the powerup type</param>
    /// <param name="bubble">the bubble prefab</param>
    private void AddPowerupIcon(PowerupType type, GameObject bubble)
    {
        var prefabToInstantiate = type switch
        {
            PowerupType.TRIPLE_SHOT => tripleShotIconPrefab,
            PowerupType.EXTRA_LIFE => extraLifeIconPrefab,
            PowerupType.MINIGUN => minigunIconPrefab,
            _ => null
        };

        if (prefabToInstantiate)
        {
            var icon = Instantiate(prefabToInstantiate);
            icon.transform.SetParent(bubble.transform);
            icon.transform.position = bubble.transform.position;
        }
    }

    /// <summary>
    /// Removes a powerup from the player's active powerups. It simply disables the powerup's active state
    /// </summary>
    /// <param name="powerup"></param>
    public void RemovePowerup(Powerup powerup)
    {
        powerups[(int)powerup.type].Active = false;
        powerups[(int)powerup.type].Duration = 0;
        if (playerSpaceship != null)
            playerSpaceship.GetComponent<SpaceshipController>().RemovePowerup(powerup);
    }

    /// <summary>
    /// Removes a powerup buble game object from the list of active powerup bubbles
    /// </summary>
    /// <param name="go">the powerup bubble game object</param>
    public void RemovePowerupBubble(GameObject go)
    {
        powerupBubbles.Remove(go);
    }

    /// <summary>
    /// Destroys all powerup game objects
    /// </summary>
    public void ClearPowerupBubbles()
    {
        // Stop spawning powerups routine (in case it wasn't already stopped)
        StopCoroutine(PowerupBubbleSpawnerRoutine());
        foreach (var powerup in powerupBubbles)
        {
            Destroy(powerup);
        }
    }
}
