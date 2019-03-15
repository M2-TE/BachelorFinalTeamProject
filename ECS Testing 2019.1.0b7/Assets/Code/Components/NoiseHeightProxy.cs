using Unity.Entities;

public class NoiseHeightProxy : SharedComponentDataProxy<NoiseHeight> { }

//public class NoiseHeightProxy : MonoBehaviour, IConvertGameObjectToEntity
//{
//	public float Amplification;
//	public float Resolution;

//	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
//	{
//		var spawnerData = new NoiseHeight
//		{
//			Amplification = Amplification,
//			Resolution = Resolution
//		};
//		dstManager.AddComponentData(entity, spawnerData);
//	}
//}
