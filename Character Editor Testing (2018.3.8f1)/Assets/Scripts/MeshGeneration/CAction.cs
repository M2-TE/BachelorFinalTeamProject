using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tool { Additive, Subtractive, White, Red, Green, Blue }

public class CAction
{
    public Tool tool;
    public Vector3 position;
}
