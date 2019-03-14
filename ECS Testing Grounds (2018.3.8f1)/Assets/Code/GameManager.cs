using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GameManager
{
	private GameManagerBootstrapper bootstrapper;
	private EntityManager entityManager;

	public void Register(GameManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;
		entityManager = World.Active.GetOrCreateManager<EntityManager>();
		Start();
	}

	#region Singleton Implementation
	private GameManager() { }
	private static GameManager instance;
	public static GameManager Instance { get => instance ?? new GameManager(); }
	#endregion

	private void Start()
	{
		NativeArray<Entity> entities = new NativeArray<Entity>(1, Allocator.Temp);
		entityManager.Instantiate(bootstrapper.EntityPrefab, entities);

		for(int i = 0; i < 1; i++)
		{
			entityManager.SetComponentData(entities[i], new Position { Value = new float3(0f, 0f, 0f) });
			entityManager.SetComponentData(entities[i], new Rotation { Value = new quaternion(0f, 1f, 0f, 0f) });
		}
		entities.Dispose();
	}
}
