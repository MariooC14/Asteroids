/// <summary>
/// Class used for managing the state of the game
/// </summary>
public class StateMachine
{
    State activeState;

    public StateMachine(State activeState)
    {
        this.activeState = activeState;
        activeState.Enter();
    }

    /// <summary>
    /// Changes from its old state to the new state
    /// </summary>
    /// <param name="newState"></param>
    public void TransitionTo(State newState)
    {
        activeState.Exit();
        activeState = newState;
        activeState.Enter();
    }

    public GameState GetState()
    {
        return activeState.GetState();
    }
}

