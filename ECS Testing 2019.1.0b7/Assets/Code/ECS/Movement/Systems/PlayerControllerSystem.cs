using ECS.Movement.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Movement.Systems
{
	public class PlayerControllerSystem : JobComponentSystem
	{
		[BurstCompile]
		struct PlayerControlJob : IJobProcessComponentData<PlayerController, PhysicsVelocity, Rotation>
		{
			[ReadOnly] public float3 movementInput;

			public void Execute([ReadOnly] ref PlayerController playerController, ref PhysicsVelocity velocity, ref Rotation rotation)
			{
				movementInput *= playerController.MovespeedMod;
				velocity.Linear = movementInput + new float3(0f, velocity.Linear.y, 0f);
				rotation.Value = quaternion.identity;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			Vector3 movementInput = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1f) * Time.deltaTime;
			return new PlayerControlJob
			{
				movementInput = movementInput
			}.Schedule(this, inputDeps);
		}
	}
}
