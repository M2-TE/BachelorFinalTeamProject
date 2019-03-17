using System;
using Unity.Entities;

namespace ECS.Movement.Components
{
	[Serializable]
	public struct PlayerControl : IComponentData
	{
		public int ControlID;
		public float VerticalSpeedMod;
		public float HorizontalSpeedMod;
	}

	public class PlayerControlProxy : ComponentDataProxy<PlayerControl> { }
}