using UnityEngine;

/// <summary>
/// State that represents the game playing state
/// </summary>
public class PlayState : State
{
    public PlayState()
    {
        stateName = GameState.Playing;
    }

    public override void Enter()
    {
        // Reset time scale (just in case)
        Time.timeScale = 1;
        UIManager.instance.ShowPanel("GameOverlay");
        UIManager.instance.UseTargetCursor();
    }

    public override void Exit()
    {
        UIManager.instance.HidePanel("GameOverlay");
        UIManager.instance.UseDefaultCursor();
    }
}