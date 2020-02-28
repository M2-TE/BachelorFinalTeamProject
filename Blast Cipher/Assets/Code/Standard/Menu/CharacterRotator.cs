using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRotator : MonoBehaviour
{
    [SerializeField] private float speed;

    public bool Enabled { get; set; }

    private void Start()
    {
        Enabled = true;
    }

    void Update()
    {
        if (Enabled)
        {
            transform.Rotate(0f, speed, 0f);
        }
    }
}
