using System.Collections;
using System.ComponentModel;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Manages the score and score multiplier of the game, including the camera space UI for the multiplier.
/// </summary>
public class ScoreManager : BaseManager
{
    public static ScoreManager instance;

    // Inspector settings
    [Description("The amount of time the player has to get a combo from shooting an asteroid")]
    public float comboTimeLimit = 3f;
    public float maxScoreMultiplier = 5;
    public float scoreMultiplierAnimationTime = 0.5f;
    //

    private int score = 0;
    private int scoreMultiplier = 1;
    public int prevHighScore;
    private int highScore;

    private readonly Color[] colors = new Color[]
    {
        Color.white,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.yellow
    };

    private float comboTimeElapsed = 0;
    // bool used for adding a multiplier only for destroying a second asteroid
    private bool pendingFirstCombo = true;

    public Canvas scoreMultiplierCanvas;
    public TextMeshPro cameraSpaceMultiplierText;

    // The original font size of the camera space multiplier text (set in the inspector)
    private float originalFontSize;
    private IEnumerator timerRoutine;
    private IEnumerator animationRoutine;

    public int Score
    {
        get => score;
        set
        {
            if (value >= 0)
                score = value;

            if (score > highScore)
                HighScore = score;
            // Update UI
            UIManager.instance.UpdateScore(score);
        }
    }

    public int HighScore
    {
        get => highScore;
        set
        {
            highScore = value;
            // Update UI
            UIManager.instance?.UpdateHighScore(highScore);
        }
    }

    public int ScoreMultiplier
    {
        get => scoreMultiplier;
        set
        {
            // Increase multiplier and cap it to x5
            scoreMultiplier = (int) math.clamp(value, 1, maxScoreMultiplier);
            // Apply the color to the text
            cameraSpaceMultiplierText.color = colors[scoreMultiplier - 1];
            cameraSpaceMultiplierText.text = $"x{scoreMultiplier}";
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;

        HighScore = ReadHighScore();
        cameraSpaceMultiplierText.text = "X1";
        cameraSpaceMultiplierText.gameObject.SetActive(false);
        originalFontSize = cameraSpaceMultiplierText.fontSize;

        timerRoutine = ComboTimerRoutine();

        base.SetUp();
    }

    /// <summary>
    /// Starts the combo timer.
    /// </summary>
    public void StartComboTimer()
    {
        StartCoroutine(timerRoutine);
    }

    /// <summary>
    /// Stops the combo timer
    /// </summary>
    public void StopComboTimer()
    {
        StopCoroutine(timerRoutine);
    }

    /// <summary>
    /// Checks the time taken between asteroid destructions to determine if the player gets a combo.
    /// </summary>
    private IEnumerator ComboTimerRoutine()
    {
        // Timer will update every 0.2 seconds.
        float timeToAdd = 0.2f;
        var waitTime = new WaitForSeconds(timeToAdd);

        while (true)
        {
            comboTimeElapsed += timeToAdd;
            // if the player takes too long to destroy an asteroid, reset the multiplier
            if (comboTimeElapsed >= comboTimeLimit)
            {
                ScoreMultiplier = 1;
                pendingFirstCombo = true;
                comboTimeElapsed = 0;
            }
            yield return waitTime;
        }
    }

    /// <summary>
    /// Adds a score to the player's score including the multiplier.
    /// </summary>
    /// <param name="amount"></param>
    public void AddScore(int amount)
    {
        if (pendingFirstCombo)
        {
            pendingFirstCombo = false;
        }
        else if (scoreMultiplier < maxScoreMultiplier)
            ScoreMultiplier++;

        Score += amount * scoreMultiplier;
        comboTimeElapsed = 0;
    }

    /// <summary>
    /// Adds a score to the player's score, showing the multiplier text on the camera space UI
    /// </summary>
    public void AddScore(int amount, Vector3 pos)
    {
        AddScore(amount);

        cameraSpaceMultiplierText.transform.position = pos;
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        // Only show the multiplier scores for x2 and above
        if (scoreMultiplier > 1)
        {
            animationRoutine = ScoreMultiplierAnimation(pos);
            StartCoroutine(animationRoutine);
        }
    }

    /// <summary>
    /// Resets the score
    /// </summary>
    public void ResetScore()
    {
        Score = 0;
        // Cheap trick to make sure the UI gets updated (updating it in Start won't work since
        // UIManager might not have been initialised yet)
        HighScore = HighScore;
        prevHighScore = HighScore;
    }

    /// <summary>
    /// Shows an animation for the score multiplier text
    /// </summary>
    private IEnumerator ScoreMultiplierAnimation(Vector3 pos)
    {
        // Set position to near the collision point
        var pointOnScreen = Camera.main.WorldToViewportPoint(pos);
        pointOnScreen.z = scoreMultiplierCanvas.planeDistance;
        var pointInWorld = Camera.main.ViewportToWorldPoint(pointOnScreen);
        cameraSpaceMultiplierText.transform.position = pointInWorld;
        cameraSpaceMultiplierText.rectTransform.localPosition += new Vector3(50, -50, 0);

        // Set up the multiplier text scaling effect
        float originalScale = originalFontSize;
        float currentScale = originalScale;
        // Make animation bigger for higher multipliers
        float finalFontSize = originalScale * 0.75f * scoreMultiplier;
        // multiply by two as we have two phases: grow and shrink
        float animationSpeed = (finalFontSize - originalScale) / scoreMultiplierAnimationTime * 2;

        cameraSpaceMultiplierText.gameObject.SetActive(true);

        // Double the font size over time
        while (currentScale <= finalFontSize)
        {
            currentScale += animationSpeed * Time.deltaTime;
            cameraSpaceMultiplierText.fontSize = currentScale;
            yield return null;
        }

        // decrease it back to 1.5 times its original size
        while (currentScale >= originalScale * 1.5)
        {
            currentScale -= animationSpeed * Time.deltaTime;
            cameraSpaceMultiplierText.fontSize = currentScale;
            yield return null;
        }

        // Hide it after half a second
        yield return new WaitForSeconds(0.5f);
        cameraSpaceMultiplierText.gameObject.SetActive(false);

        yield break;
    }

    /// <summary>
    /// Gets the high score from player prefs
    /// </summary>
    /// <returns></returns>
    private int ReadHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>
    /// Writes the high score to player prefs if higher than the old high score
    /// </summary>
    public void SaveHighScore()
    {
        if (highScore > ReadHighScore())
        {
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }
}
