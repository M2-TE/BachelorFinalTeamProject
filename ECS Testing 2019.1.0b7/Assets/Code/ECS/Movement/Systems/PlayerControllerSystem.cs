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
		private float3 lookDir = new float3(1f, 0f, 0f);
		
		[BurstCompile]
		struct PlayerControlJob : IJobProcessComponentData<PlayerController, PhysicsVelocity, Rotation, Translation>
		{
			[ReadOnly] public float3 movementInput;
			[ReadOnly] public float3 lookDir;

			public void Execute([ReadOnly] ref PlayerController playerController, ref PhysicsVelocity velocity, ref Rotation rotation, ref Translation translation)
			{
				movementInput *= playerController.MovespeedMod;
				velocity.Linear = movementInput + new float3(0f, velocity.Linear.y, 0f);

				//var pos = translation.Value;
				//translation.Value = (pos.y = 0);

				//rotation.Value = quaternion.identity;
				rotation.Value = quaternion.LookRotationSafe(lookDir, new float3(0f, 1f, 0f));
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			Vector3 movementInput = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1f) * Time.deltaTime;
			if (movementInput.sqrMagnitude != 0f) lookDir = movementInput;
			return new PlayerControlJob
			{
				movementInput = movementInput,
				lookDir = lookDir
			}.Schedule(this, inputDeps);
		}
	}
}
