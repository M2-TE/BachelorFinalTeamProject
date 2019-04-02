using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class BobbingController : MonoBehaviour
{
	[SerializeField] private AnimationCurve bobbingCurve;

	private Transform bobbingTransform;
	private float baseBobbingTransHeight;

	private void Awake()
	{
		bobbingTransform = transform;
		baseBobbingTransHeight = transform.localPosition.y;
	}

	private void Update()
	{
		var bob = bobbingTransform.localPosition;
		bob.y = baseBobbingTransHeight + bobbingCurve.Evaluate(Time.time);
		bobbingTransform.localPosition = bob;
	}
}
