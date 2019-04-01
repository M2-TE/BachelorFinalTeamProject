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
		public int LockedScaling;
	}

	[DisallowMultipleComponent]
	public class AudioVisualizationSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
	{
		public enum Mode { Standard, CircularCentered, CubeTower }
		public GameObject PrefabGO;
		public int3 Size;
		public float3 PrefabSize;
		public bool LockedScaling;
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
				SpawnMode = (int)SpawnMode,
				LockedScaling = LockedScaling ? 1 : 0
			};
			dstManager.AddComponentData(entity, spawnerData);
		}
	}
}