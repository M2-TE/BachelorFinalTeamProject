using System.Collections;
using System.Collections.Generic;
using Tutorial.Pure.Components;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Tutorial.Pure.Systems
{
	public class PlayerInputSystem : JobComponentSystem
	{
		private struct PlayerInputJob : IJobProcessComponentData<PlayerInput>
		{
			public float Horizontal;

			public void Execute(ref PlayerInput input)
			{
				input.Horizontal = Horizontal;
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new PlayerInputJob
			{
				Horizontal = Input.GetAxis("Horizontal")
			};

			return job.Schedule(this, inputDeps);
		}
	}
}