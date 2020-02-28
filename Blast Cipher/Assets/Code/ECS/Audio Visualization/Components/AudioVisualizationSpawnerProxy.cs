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
		public float3 Spacing;
		public int SpawnMode;
		public int LockedScaling;

		public float2 TowerSize;
	}

	[DisallowMultipleComponent, RequireComponent(typeof(ConvertToEntity))]
	public class AudioVisualizationSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
	{
		public enum Mode { Standard, CircularCentered, CubeTower }

		public GameObject PrefabGO;
		public int3 Size;
		public float3 PrefabSize;
		public float3 Spacing;
		public bool LockedScaling;
		public Mode SpawnMode;

		[Header("CubeTowerOnly")]
		public float2 TowerSize;

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
				Spacing = Spacing,
				PrefabSize = PrefabSize,
				SpawnMode = (int)SpawnMode,
				LockedScaling = LockedScaling ? 1 : 0,
				TowerSize = TowerSize
				
			};
			dstManager.AddComponentData(entity, spawnerData);
		}
	}
}