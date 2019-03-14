using Tutorial.Hybrid.Components;
using Unity.Entities;
using UnityEngine;

namespace Tutorial.Hybrid.Systems
{
	public class PlayerInputSystem : ComponentSystem
	{
		private struct Group
		{
			public PlayerInput PlayerInput;
		}

		protected override void OnUpdate()
		{
			foreach (var entity in GetEntities<Group>())
			{
				entity.PlayerInput.Horizontal = Input.GetAxis("Horizontal");
			}
		}
	}
}
