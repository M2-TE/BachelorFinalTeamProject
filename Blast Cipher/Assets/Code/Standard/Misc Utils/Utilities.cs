using UnityEngine;

public sealed class Utilities
{
	public static void CountDownVal(ref float val) => val = val > 0f ? val - Time.deltaTime : 0f;
	public static float CountDownVal(float val) => val > 0f ? val - Time.deltaTime : 0f;

    public static T PickAtRandom<T>(T[] array)
    {
        return array[Random.Range(0, (int)array.Length)];
    }
}
