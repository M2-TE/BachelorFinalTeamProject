using System;
using Unity.Entities;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	public struct AudioVisualizationInit : ISharedComponentData { }

	class AudioVisualizationInitProxy : SharedComponentDataProxy<AudioVisualizationInit> { }
}
