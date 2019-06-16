using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEditorManager : MonoBehaviour
{
    private CEditorManager() { }
    private static CEditorManager instance;
    public static CEditorManager Instance { get => instance ?? (instance = new CEditorManager()); }
}
