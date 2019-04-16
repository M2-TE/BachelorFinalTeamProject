using UnityEngine;

public abstract class Bootstrapper<T> : MonoBehaviour where T : Manager
{
	private T bootstrapper;

	protected virtual void Awake()
	{
		
	}
}
