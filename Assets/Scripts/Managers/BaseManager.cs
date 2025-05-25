using UnityEngine;

/// <summary>
/// Base class for all managers in the game.
/// The Game Manager is responsible for ensuring all other managers are set up before starting the game.
/// The methods below allow it to check for that.
/// </summary>
public class BaseManager : MonoBehaviour
{
    public bool IsSetup { get; protected set; }

    private void Start()
    {
        IsSetup = false;
    }

    // Declare the manager as ready to go
    public virtual void SetUp()
    {
        IsSetup = true;
    }
}