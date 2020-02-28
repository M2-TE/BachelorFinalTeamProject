using UnityEngine;

[CreateAssetMenu(fileName = "Global Material Refs", menuName = "Global Mat Refs", order = 0)]
public class GlobalMatRefs : ScriptableObject
{
	public Material GlobalMat;
	public Material GlobalDimMat;

	[Header("Colors")]
	[ColorUsage(true, true)] public Color red;
	[ColorUsage(true, true)] public Color green;
	[ColorUsage(true, true)] public Color blue;

	[Header("Dim Colors")]
	[ColorUsage(true, true)] public Color dimRed;
	[ColorUsage(true, true)] public Color dimGreen;
	[ColorUsage(true, true)] public Color dimBlue;
}
