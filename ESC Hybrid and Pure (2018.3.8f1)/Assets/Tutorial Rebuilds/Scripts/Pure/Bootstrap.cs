using Tutorial.Pure.Components;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
	public float Speed;
	public Mesh Mesh;
	public Material Material;

	private void Start()
	{
		var entityManager = World.Active.GetOrCreateManager<EntityManager>();

		var playerEntity = entityManager.CreateEntity(
			ComponentType.Create<Speed>(),
			ComponentType.Create<PlayerInput>(),
			ComponentType.Create<Position>(),
			ComponentType.Create<RenderMesh>());

		entityManager.SetComponentData(playerEntity, new Speed { Value = Speed });
		entityManager.SetSharedComponentData(playerEntity, new RenderMesh
		{
			mesh = Mesh,
			material = Material
		});
	}
}
