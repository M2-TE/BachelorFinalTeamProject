public abstract class Manager<T> where T : Manager<T>, new()
{
	public Manager() => GameManager.Instance.extendedUpdates.Add(ExtendedUpdate);

	private static T instance;
	public static T Instance { get => instance ?? (instance = new T()); }

	protected abstract void ExtendedUpdate();
}
