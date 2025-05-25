using UnityEngine;

public class RoundOverState : State
{
	public RoundOverState()
	{
		stateName = GameState.RoundOver;
	}

	public override void Enter()
	{
		UIManager.instance.ShowPanel("RoundOverPanel");
		ScoreManager.instance.StopComboTimer();
		GameManager.instance.StartRoundOverAnimation();
    }

	public override void Exit()
	{
		UIManager.instance.HidePanel("RoundOverPanel");
		GameManager.instance.StartNextLevel();
    }
}