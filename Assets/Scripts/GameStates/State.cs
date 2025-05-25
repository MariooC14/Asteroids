/// <summary>
/// Abstract class that defines the fields and methods a state should have.
/// GameState is an enum that represents the name of the state.
/// </summary>
public abstract class State
{
    internal GameState stateName;
    public virtual GameState GetState() { return stateName; }

    // Specify what happens when the state is entered
    public virtual void Enter() { }
    // Specify what happens when state changes to another state
    public virtual void Exit() { }
}