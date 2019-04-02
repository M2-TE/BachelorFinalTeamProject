using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tool { Additive, Subtractive, Save }

public class CAction
{
    public Tool tool;
    public Vector3 position;
}
