/// <summary>
/// State that represents the game over screen
/// </summary>
public class GameOverState : State
{
    public GameOverState()
    {
        stateName = GameState.GameOver;
    }

    public override void Enter()
    {
        UIManager.instance.UpdateEndOfGameScores();
        UIManager.instance.ShowPanel("GameOverPanel");
        PowerupManager.instance.StopSpawningPowerupBubbles();
        AsteroidManager.instance.StopSpawningAsteroids();
        ScoreManager.instance.StopComboTimer();
        ScoreManager.instance.SaveHighScore();

        AsteroidManager.instance.ClearAsteroids();
    }

    public override void Exit()
    {
        UIManager.instance.HidePanel("GameOverPanel");
        // We could call clear arena here, but this would also happen when the user
        // quits to main menu from the pause menu. The main menu state enter calls clear arena instead.
    }
}