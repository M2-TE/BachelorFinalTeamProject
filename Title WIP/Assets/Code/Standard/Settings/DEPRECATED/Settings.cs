using System;

[Serializable]
public class Settings
{
	public Settings()
	{
		RuntimeSettings = new RuntimeSettings();
		ClientSettings = new ClientSettings();
		PlayerSettings = new PlayerSettings[]
		{
			new PlayerSettings(),
			new PlayerSettings()
		};
	}

	public RuntimeSettings RuntimeSettings;
	public ClientSettings ClientSettings;
	public PlayerSettings[] PlayerSettings;

	public void LoadSettings()
	{
		// TODO
	}

	public void SaveSettings()
	{
		// TODO
	}
}
