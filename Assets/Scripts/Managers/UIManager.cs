using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the UI of the game, including the score, high score, lives, and round over overlay.
/// It does not manage the display of the score multiplier as that is done by the score manager
/// </summary>
public class UIManager : BaseManager
{
    public static UIManager instance;
    bool fullScreen = true;

    // Helper inner class for identifying a Unity panel with a string. It allows us to define a name
    // different to its game object name
    [System.Serializable]
    public class Panel
    {
        public string name;
        public GameObject panelObject;
    }

    private Dictionary<string, GameObject> panelDict;
    // Inspector settings
    public List<Panel> panels;
    [Description("Digits per second to update the score by.")]
    public float scoreAnimationSpeed = 100;

    public Texture2D targetCursorTexture;

    // Reference to the UI text (for updating UI)
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public TMP_Text livesText;
    public TMP_Text roundOverlayText;
    public TMP_Text gameOverScoreText;
    public TMP_Text gameOverHighScoreText;
    //

    // Helper variables for running the score and highscore animation
    private int currentScoreDisplay = 0;
    private int currentHighScoreDisplay = 0;
    private int targetScore = 0;
    private int targetHighScore = 0;
    private bool animatingScore = false;

    // No need for SetUpMethod in this
    void Start()
    {
        SetUp();
    }

    // Manager setup
    public override void SetUp()
    {
        instance = this;
        fullScreen = true;
        var res = Screen.currentResolution;
        Screen.SetResolution(res.width, res.height, fullScreen);

        panelDict = new Dictionary<string, GameObject>();
        panels.ForEach(panel => panelDict[panel.name] = panel.panelObject);

        base.SetUp();
    }

    /// <summary>
    /// Hides all panels
    /// </summary>
    public void HideAllPanels()
    {
        panels.ForEach(panel => panel.panelObject.SetActive(false));
    }

    /// <summary>
    /// Toggles between fullscreen and windowed mode
    /// </summary>
    public void ToggleFullScreenMode()
    {
        fullScreen = !fullScreen;
        if (fullScreen)
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    /// <summary>
    /// Turns a panel on so it can be seen on the canvas
    /// This is used instead of a toggle function to allow Unity inspector to add this function to button listeners
    /// </summary>
    /// <param name="name"></param>
    public void ShowPanel(string name)
    {
        panelDict[name].SetActive(true);
    }

    /// <summary>
    /// Turns a panel off so it can not longer be seen on the canvas
    /// </summary>
    /// <param name="name"></param>
    public void HidePanel(string name)
    {
        panelDict[name].SetActive(false);
    }

    /// <summary>
    /// Updates the scoretext UI on the overlay
    /// </summary>
    /// <param name="score"></param>
    public void UpdateScore(int newScore)
    {
        targetScore = newScore;

        if (newScore == 0)
        {
            scoreText.text = "Score: 0";
            currentScoreDisplay = 0;
        }
        // Go up from the previous score to the new score with animation
        else if (!animatingScore)
        {
            StartCoroutine(AnimateScore());
        }
    }

    /// <summary>
    /// Updates the high score text UI on the overlay.
    /// </summary>
    /// <param name="highScore"></param>
    public void UpdateHighScore(int highScore)
    {
        // Set the text without animation if the target high score is 0
        if (targetHighScore == 0)
        {
            highScoreText.text = highScore.ToString();
            currentHighScoreDisplay = highScore;
        }

        targetHighScore = highScore;
    }

    /// <summary>
    /// Updates the lives text UI on the overlay
    /// </summary>
    /// <param name="lives"></param>
    public void UpdateLives(int lives)
    {
        livesText.text = lives.ToString();
    }

    /// <summary>
    /// Updates the round over overlay UI text
    /// </summary>
    /// <param name="text"></param>
    public void SetRoundOverOverlayText(string text)
    {
        roundOverlayText.text = text;
    }

    public void UpdateEndOfGameScores()
    {
        var score = ScoreManager.instance.Score;
        var highScore = ScoreManager.instance.HighScore;

        gameOverHighScoreText.text = $"High Score: {highScore}";

        if (highScore > ScoreManager.instance.prevHighScore)
        {
            gameOverHighScoreText.text += " (New high score!)";
        }
        gameOverScoreText.text = $"Your Score: {score}";
    }

    /// <summary>
    /// Gradually increases the display score to the real score, as well as the high score
    /// </summary>
    private IEnumerator AnimateScore()
    {
        animatingScore = true;
        var waitTime = new WaitForSeconds(1 / scoreAnimationSpeed);
        var originalFontSize = scoreText.fontSize;

        while (currentScoreDisplay < targetScore || currentHighScoreDisplay < targetHighScore)
        {
            // Update the score
            if (currentScoreDisplay < targetScore)
            {
                currentScoreDisplay++;
                scoreText.text = $"Score: {currentScoreDisplay}";
                scoreText.fontSize = originalFontSize + Mathf.Sin(currentScoreDisplay);
            }
            
            // Update the high score
            if (currentHighScoreDisplay < targetHighScore)
            {
                currentHighScoreDisplay++;
                highScoreText.text = currentHighScoreDisplay.ToString();
                highScoreText.fontSize = originalFontSize + Mathf.Sin(currentHighScoreDisplay);
            }
                
            yield return waitTime;
        }

        scoreText.fontSize = originalFontSize;
        animatingScore = false;
        yield break;
    }

    /// <summary>
    /// Switches the mouse cursor to a gun sight texture
    /// </summary>
    public void UseTargetCursor()
    {
        // Set the target cursor's hot spot to its centre instead of top left
        Vector2 hotSpot = new Vector2(targetCursorTexture.width / 2, targetCursorTexture.height / 2);
        Cursor.SetCursor(targetCursorTexture, hotSpot, CursorMode.Auto);
    }

    /// <summary>
    /// Switches the mouse cursor back to the default Unity cursor
    /// </summary>
    public void UseDefaultCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
