public abstract class Manager
{
	protected Manager() => GameManager.Instance.extendedUpdates.Add(ExtendedUpdate);

	protected virtual void Register()
	{

	}

	protected abstract void ExtendedUpdate();
}
