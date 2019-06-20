using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField][Range(0.1f, 0.9f)] private float movementSpeed;
    [SerializeField][Range(0.1f, 0.9f)] private float rotationSpeed;

    private void LateUpdate()
    {
        Vector2 Rotation = CEditorManager.Instance.cEditorInput.LeftStick.normalized;
        Vector2 Position = CEditorManager.Instance.cEditorInput.RightStick.normalized * movementSpeed;

        transform.position += new Vector3(Position.x,Position.y);
        transform.Rotate(new Vector2(Rotation.y,Rotation.x), rotationSpeed, Space.World);

    }
}
