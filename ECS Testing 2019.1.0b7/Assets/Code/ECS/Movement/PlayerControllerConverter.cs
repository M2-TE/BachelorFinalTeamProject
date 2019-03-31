using ECS.Movement.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Movement
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ConvertToEntity))]
	public class PlayerControllerConverter : MonoBehaviour, IConvertGameObjectToEntity
	{
		public int PlayerID;
		public float Movespeed;


		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			// set transform
			dstManager.SetComponentData(entity, new Translation { Value = transform.position });
			dstManager.SetComponentData(entity, new Rotation { Value = transform.rotation });

			if (dstManager.HasComponent<NonUniformScale>(entity))
				dstManager.SetComponentData(entity, new NonUniformScale { Value = transform.lossyScale });
			else dstManager.AddComponentData(entity, new NonUniformScale { Value = transform.lossyScale });

			dstManager.AddComponentData(entity, new PlayerController
			{
				ControlID = PlayerID,
				MovespeedMod = Movespeed
			});
		}
	}
}
