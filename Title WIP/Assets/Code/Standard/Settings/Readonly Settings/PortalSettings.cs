using UnityEngine;

[CreateAssetMenu(fileName = "PortalSettings", menuName = "Settings/Portal", order = 2)]
public class PortalSettings : ReadonlySettings
{
	public GameObject portalConnectorPrefab;
	public AudioClip[] teleportationSounds;

	[Space]
	public float PostTeleportForce = 30f;
	public float AutoTargetPostForce = 17f;
}
