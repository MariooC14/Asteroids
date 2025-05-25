/// <summary>
/// State that encapsulates the logic for the main menu
/// </summary>
public class MainMenuState : State
{
    public MainMenuState()
    {
        stateName = GameState.MainMenu;
    }

    public override void Enter()
    {
        // Hide the player spaceship on startup or on return from game
        GameManager.instance.DespawnPlayerSpaceship();
        GameManager.instance.ClearArena();
        UIManager.instance.ShowPanel("MainMenu");
    }

    public override void Exit()
    {
        UIManager.instance.HidePanel("MainMenu");
    }
}
