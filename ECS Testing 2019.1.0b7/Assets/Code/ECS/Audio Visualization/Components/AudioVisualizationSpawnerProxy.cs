using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	public struct AudioVisualizationSpawner : IComponentData
	{
		public Entity PrefabEntity;
		public float3 Size;
		public float3 PrefabSize;
		public int SpawnMode;
	}

	[DisallowMultipleComponent]
	public class AudioVisualizationSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
	{
		public enum Mode { Standard, CircularCentered }
		public GameObject PrefabGO;
		public int3 Size;
		public float3 PrefabSize;
		public Mode SpawnMode;

		public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
		{
			referencedPrefabs.Add(PrefabGO);
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			var spawnerData = new AudioVisualizationSpawner
			{
				PrefabEntity = conversionSystem.GetPrimaryEntity(PrefabGO),
				Size = Size,
				PrefabSize = PrefabSize,
				SpawnMode = (int)SpawnMode
			};
			dstManager.AddComponentData(entity, spawnerData);
		}
	}
}