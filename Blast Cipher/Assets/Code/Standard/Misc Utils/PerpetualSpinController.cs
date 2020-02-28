using UnityEngine;

public class PerpetualSpinController : MonoBehaviour
{
	[SerializeField] private Vector3 spinVec;

	private void Update() => transform.Rotate(spinVec * Time.deltaTime);
}
