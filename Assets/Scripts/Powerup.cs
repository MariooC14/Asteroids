/// <summary>
/// Represents powerup that can be applied to the player.
/// </summary>
public class Powerup
{
    public PowerupType type { get; }
    public float Duration { get; set; }
    public bool Active { get; set; }

    public Powerup(PowerupType type, float duration)
    {
        this.type = type;
        Duration = duration;
        Active = false;
    }
}
