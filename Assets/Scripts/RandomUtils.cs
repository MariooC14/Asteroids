using UnityEngine;

/// <summary>
/// class that contains random useful utils
/// </summary>
public class RandomUtils
{
    // Returns a Vector3 where all 3 values are random values between rangeMin and rangeMax
    public static Vector3 RandomVec3(float rangeMin, float rangeMax)
    {
        var v3 = Random.Range(rangeMin, rangeMax);
        var v2 = Random.Range(rangeMin, rangeMax);
        var v1 = Random.Range(rangeMin, rangeMax);
        return new Vector3(v1, v2, v3);
    }
}