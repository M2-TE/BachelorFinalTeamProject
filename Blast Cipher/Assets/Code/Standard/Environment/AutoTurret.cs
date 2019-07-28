using System.Collections;
using UnityEngine;

public class AutoTurret : MonoBehaviour
{
	[SerializeField] private PlayerCharacterSettings settings;
	[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private Material globalColor, greyedColor;
	[SerializeField] private Renderer[] renderers;
	[SerializeField] private Transform[] launchPositions;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private int amtPerDir;
	[SerializeField] private int beatDelay;
	[SerializeField] private int initialBeatDelay = 2;
	
	private MusicManager music;
	private GameManager manager;
	private Renderer[] oldIterationRenderers;
	private int currentIteration;

	private void Start()
	{
		manager = GameManager.Instance;
		music = MusicManager.Instance;
		music.RegisterCallOnNextBeat(OnBeat, initialBeatDelay, false);
		currentIteration = 0;

		for(int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material = greyedColor;
		}
		oldIterationRenderers = new Renderer[amtPerDir];
	}

	private void Update()
	{
		transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
	}

	private void OnBeat()
	{
		// cache
		Renderer renderer;

		// disable old dir
		for (int i = 0; i < oldIterationRenderers.Length; i++)
		{
			renderer = oldIterationRenderers[i];
			if (renderer == null) break;
			renderer.material = greyedColor;
		}

		// enable current dir
		int startIndex = amtPerDir * (currentIteration = currentIteration > 2 ? 0 : currentIteration + 1);
		for(int i = 0; i < amtPerDir; i++)
		{
			renderer = renderers[startIndex + i];
			renderer.material = globalColor;
			oldIterationRenderers[i] = renderer;
		}

		// shoot projectile
		{
			var projectile = manager.SpawnObject(projectilePrefab).GetComponent<Projectile>();

			projectile.transform.position = launchPositions[currentIteration].position;
			projectile.rgb.WakeUp();
			projectile.rgb.useGravity = true;
			projectile.ownCollider.enabled = true;

			// launch projectile in aim direction
			var forceVec = launchPositions[currentIteration].position - transform.position;
			forceVec.y = 0f;

			Vector3 shotVec = forceVec.normalized * settings.ShotStrength;
			projectile.rgb.AddForce(shotVec, ForceMode.Impulse);
			projectile.actualVelocity = shotVec; // for portal/wall bounces

			projectile.InitialShooter = null;
			projectile.wallHitVol = .4f;
			OneShotAudioManager.PlayOneShotAudio(settings.ProjectileShotSounds, launchPositions[currentIteration].position, .4f);
		}
		if(Time.timeScale < .01f)
		{
			StartCoroutine(OnBeatDelayed());
		}
		else
		{
			music.RegisterCallOnNextBeat(OnBeat, beatDelay, false);
		}
	}

	private IEnumerator OnBeatDelayed()
	{
		while(Time.timeScale < .01f) yield return null;
		music.RegisterCallOnNextBeat(OnBeat, beatDelay, false);
	}
}
