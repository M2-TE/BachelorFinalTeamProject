using System;
using Unity.Entities;

namespace ECS.Movement.Components
{
	[Serializable]
	public struct PlayerController : IComponentData
	{
		public int ControlID;
		public float MovespeedMod;
	}

	public class PlayerControllerProxy : ComponentDataProxy<PlayerController> { }
}