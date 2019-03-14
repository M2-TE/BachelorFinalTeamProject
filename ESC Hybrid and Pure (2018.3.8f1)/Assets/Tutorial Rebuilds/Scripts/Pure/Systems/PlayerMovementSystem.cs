using System.Collections;
using System.Collections.Generic;
using Tutorial.Pure.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Tutorial.Pure.Systems
{
	public class PlayerMovementSystem : JobComponentSystem
	{
		private struct PlayerMovementJob : IJobProcessComponentData<Speed, PlayerInput, Position>
		{
			public float DeltaTime;
			public void Execute(ref Speed speed, ref PlayerInput input, ref Position position)
			{
				position.Value.x += speed.Value * input.Horizontal * DeltaTime;
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new PlayerMovementJob
			{
				DeltaTime = Time.deltaTime
			};

			return job.Schedule(this, inputDeps);
		}
	}
}