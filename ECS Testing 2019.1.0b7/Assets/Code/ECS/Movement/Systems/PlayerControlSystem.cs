using ECS.Movement.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Movement.Systems
{
	public class PlayerControlSystem : JobComponentSystem
	{
		[BurstCompile]
		struct PlayerControlJob : IJobProcessComponentData<Translation, PlayerControl>
		{
			[ReadOnly] public float hInput;
			[ReadOnly] public float vInput;
			[ReadOnly] public float DeltaTime;

			public void Execute(ref Translation translation, [ReadOnly] ref PlayerControl playerControl)
			{
				translation.Value += new float3(hInput * playerControl.HorizontalSpeedMod * DeltaTime, 0f, vInput * playerControl.VerticalSpeedMod * DeltaTime);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new PlayerControlJob
			{
				hInput = Input.GetAxis("Horizontal"),
				vInput = Input.GetAxis("Vertical"),
				DeltaTime = Time.deltaTime
			}.Schedule(this, inputDeps);
		}
	}
}
