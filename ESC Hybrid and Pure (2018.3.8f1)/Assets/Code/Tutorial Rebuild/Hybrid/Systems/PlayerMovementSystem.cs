using Tutorial.Hybrid.Components;
using Unity.Entities;
using UnityEngine;

namespace Tutorial.Hybrid.Systems
{
	public class PlayerMovementSystem : ComponentSystem
	{
		private struct Group
		{
			public Transform Transform;
			public PlayerInput PlayerInput;
			public Speed Speed;
		}

		protected override void OnUpdate()
		{
			foreach (var entity in GetEntities<Group>())
			{
				Vector3 position = entity.Transform.position;
				position.x += entity.Speed.Value * Time.deltaTime * entity.PlayerInput.Horizontal;
				entity.Transform.position = position;
			}
		}
	}
}