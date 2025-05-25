using System.Collections;
using UnityEngine;

/// <summary>
/// The different states of the game
/// </summary>
public enum GameState { MainMenu, Playing, Paused, RoundOver, NewRound, GameOver };

/// <summary>
/// The Master Manager that manages the game itself.
/// </summary>
public class GameManager : BaseManager
{
    public static GameManager instance;

    // Inspector stuff
    public BaseManager[] managers;
    public GameObject playerSpaceShip;
    public int currentGameLevel;
    //

    public static FullScreenMode fullScreenMode;
    public StateMachine stateMachine;

    Camera mainCam;
    public CameraController cameraController;

    private int lives;
    public int Lives
    {
        get => lives;
        set
        {
            if (value >= 0)
                lives = value;
            // Update UI
            UIManager.instance.UpdateLives(lives);
        }
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        UIManager.instance.HideAllPanels();
        UIManager.instance.ShowPanel("LoadingPanel");
        yield return StartCoroutine(WaitForManagersToSetUp());
        SetUp();
        UIManager.instance.HidePanel("LoadingPanel");
    }

    // Manager set up
    public override void SetUp()
    {
        instance = this;
        mainCam = Camera.main;
        cameraController = mainCam.GetComponent<CameraController>();
        DespawnPlayerSpaceship();
        fullScreenMode = FullScreenMode.Windowed;

        stateMachine = new StateMachine(new MainMenuState());
    }

    /// <summary>
    /// Waits until every other manager has set up
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForManagersToSetUp()
    {
        foreach (var manager in managers)
            yield return new WaitUntil(() => manager.IsSetup);
    }

    private void Update()
    {
        // Listen for escape key to pause or unpause the game
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (stateMachine.GetState() == GameState.Playing)
                stateMachine.TransitionTo(new PausedState());
            else if (stateMachine.GetState() == GameState.Paused)
                stateMachine.TransitionTo(new PlayState());
        }
    }

    /// <summary>
    /// Changes the current state of the game
    /// </summary>
    /// <param name="newState"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    public void ChangeState(GameState newState)
    {
        State state = newState switch
        {
            GameState.MainMenu => new MainMenuState(),
            GameState.Playing => new PlayState(),
            GameState.Paused => new PausedState(),
            GameState.RoundOver => new RoundOverState(),
            GameState.GameOver => new GameOverState(),
            _ => throw new System.NotImplementedException()
        };
        stateMachine.TransitionTo(state);
    }

    /// <summary>
    /// Starts a new game. Resets the score, lives, and starts the first level
    /// </summary>
    public void StartNewGame()
    {
        currentGameLevel = 0;
        ScoreManager.instance.ResetScore();
        Lives = 3;
        SpawnPlayerSpaceship();
        StartNextLevel();
        stateMachine.TransitionTo(new PlayState());
    }

    /// <summary>
    /// Starts the next level. It respawns the spaceship, decides
    /// how many asteroids to spawn based on the current level, and also starts spawning powerups
    /// </summary>
    public void StartNextLevel()
    {
        PowerupManager.instance.StopSpawningPowerupBubbles();
        currentGameLevel++;
        AsteroidManager.instance.StartSpawningAsteroids();
        PowerupManager.instance.StartSpawningPowerupBubbles();
        ScoreManager.instance.StartComboTimer();
    }

    /// <summary>
    /// Spawns the player spaceship.
    /// If the spaceship has been 'despawned' by calling DespawnPlayerSpaceship, it will be reactivated
    /// </summary>
    private void SpawnPlayerSpaceship()
    {
        // Reset the spaceship's position and velocity. Happens when we start a new level
        playerSpaceShip.transform.position = Vector3.zero;
        playerSpaceShip.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        playerSpaceShip.SetActive(true);
    }

    /// <summary>
    /// When the spaceship collides with an asteroid, it takes away a life.
    /// It triggers a game ending when lives reaches 0
    /// </summary>
    public void HurtPlayerSpaceship()
    {
        Lives -= 1;
        if (Lives <= 0)
            ChangeState(GameState.GameOver);
        else
            RespawnPlayerSpaceship();
    }

    /// <summary>
    /// Respawns the player spaceship at the center of the arena
    /// </summary>
    public void RespawnPlayerSpaceship()
    {
        StartCoroutine(RespawnPlayerSpaceshipRoutine());
    }

    /// <summary>
    /// Coroutine that respawns the player spaceship after a delay
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawnPlayerSpaceshipRoutine()
    {
        DespawnPlayerSpaceship();
        yield return new WaitForSeconds(2);
        SpawnPlayerSpaceship();
    }


    /// <summary>
    /// Freezes the spaceship in its position
    /// </summary>
    public void FreezeSpaceship()
    {
        playerSpaceShip.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    /// <summary>
    /// Unfreezes the spaceship, allowing it to move in the X and Z direction
    /// </summary>
    public void UnfreezeSpaceship()
    {
        playerSpaceShip.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
    }

    /// <summary>
    /// Removes the spacesip from the arena (by toggling its active state to false)
    /// </summary>
    public void DespawnPlayerSpaceship()
    {
        playerSpaceShip.SetActive(false);
    }

    /// <summary>
    /// Helper function that pauses the game.
    /// Can be used by buttons in the UI (onclick in inspector)
    /// </summary>
    public void PauseGame()
    {
        ChangeState(GameState.Paused);
    }

    /// <summary>
    /// Helper function that resumes the game.
    /// </summary>
    public void ResumeGame()
    {
        ChangeState(GameState.Playing);
    }

    /// <summary>
    /// Helper function that returns the user to the main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        ChangeState(GameState.MainMenu);
    }

    /// <summary>
    /// Starts the round over animation
    /// </summary>
    public void StartRoundOverAnimation()
    {
        StartCoroutine(RoundOverAnimationRoutine());
    }

    /// <summary>
    /// Coroutine that shows the round over animation
    /// </summary>
    /// <returns></returns>
    private IEnumerator RoundOverAnimationRoutine()
    {
        UIManager.instance.SetRoundOverOverlayText("Round Over");
        yield return new WaitForSeconds(2);

        // show current level + 1 as startNextLevel is called after this animation
        UIManager.instance.SetRoundOverOverlayText("Starting round " + (currentGameLevel+1));
        yield return new WaitForSeconds(1);

        for (int i = 3; i > 0; i--)
        {
            UIManager.instance.SetRoundOverOverlayText(i.ToString());
            yield return new WaitForSeconds(1);
        }

        UIManager.instance.SetRoundOverOverlayText("Go!");
        ChangeState(GameState.Playing);
    }

    /// <summary>
    /// Clears the arena of asteroids and powerup bubbles.
    /// </summary>
    public void ClearArena()
    {
        AsteroidManager.instance.ClearAsteroids();
        PowerupManager.instance.ClearPowerupBubbles();
    }

    /// <summary>
    /// Adds a temporary camera shake effect
    /// </summary>
    public void ShakeCamera()
    {
        if (cameraController != null)
            cameraController.ShakeCamera();
    }

    /// <summary>
    /// Exits the game. It allows the exit to button to quit the game in the editor
    /// Code taken from https://stackoverflow.com/a/70439071/5586832
    /// </summary>
    public void ExitGame()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
