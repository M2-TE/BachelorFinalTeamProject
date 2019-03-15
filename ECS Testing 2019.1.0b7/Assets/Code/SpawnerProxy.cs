using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
	public GameObject Prefab;
	public int2 Size;

	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(Prefab);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		var spawnerData = new Spawner
		{
			Prefab = conversionSystem.GetPrimaryEntity(Prefab),
			SizeX = Size.x,
			SizeY = Size.y
		};
		dstManager.AddComponentData(entity, spawnerData);
	}
}
