using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS.Movement.Components
{
	public struct PlayerSpawner : IComponentData
	{
		public Entity Prefab;
		public int PlayerID;
	}

	[DisallowMultipleComponent]
	public class PlayerSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
	{
		public GameObject Prefab;
		public int PlayerID;

		public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
		{
			referencedPrefabs.Add(Prefab);
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			var spawnerData = new PlayerSpawner
			{
				Prefab = conversionSystem.GetPrimaryEntity(Prefab),
				PlayerID = PlayerID
			};
			dstManager.AddComponentData(entity, spawnerData);
		}
	}
}
