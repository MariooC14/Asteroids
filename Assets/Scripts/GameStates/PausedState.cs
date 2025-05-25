using UnityEngine;

/// <summary>
/// State that represents the game paused state. It shows the pause menu
/// </summary>
public class PausedState : State
{
    public PausedState()
    {
        stateName = GameState.Paused;
    }

    public override void Enter()
    {
        // Pause the game and show the menu
        Time.timeScale = 0;
        UIManager.instance.ShowPanel("PauseMenu");
    }

    public override void Exit()
    {
        // Resume the game and hide the menu
        Time.timeScale = 1;
        UIManager.instance.HidePanel("PauseMenu");
    }
}