using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LookDirection { North, South, East, West}

public class CameraMovement : MonoBehaviour
{
    [SerializeField][Range(0.1f, 0.9f)] private float movementSpeed;
    [SerializeField][Range(0.1f, 0.9f)] private float rotationSpeed;

    public LookDirection LookingAt;

    private void LateUpdate()
    {
        Vector2 position = CEditorManager.Instance.cEditorInput.LeftStick.normalized;
        Vector2 rotation = CEditorManager.Instance.cEditorInput.RightStick.normalized;
        float upMovement = CEditorManager.Instance.cEditorInput.RightTrigger;
        float downMovement = CEditorManager.Instance.cEditorInput.LeftTrigger;

        Vector3 forward = Vector3.Scale(transform.forward,new Vector3(1f,0f,1f)).normalized * position.y;
        Vector3 right = Vector3.Scale(transform.right, new Vector3(1f,0f,1f)).normalized * position.x;
        Vector3 up = Vector3.up * (upMovement - downMovement);

        if (!CEditorManager.Instance.cEditorInput.LeftStickPress)
        {
            forward = transform.forward.normalized * position.y;
            right = transform.right * position.x;
            up = transform.up * (upMovement - downMovement);
        }

        transform.position += (forward + right + up).normalized * movementSpeed;
        transform.Rotate(0f, rotation.x * rotationSpeed, 0f,Space.World);
        transform.Rotate(-rotation.y * rotationSpeed, 0f, 0f, Space.Self);

        float y = transform.rotation.eulerAngles.y;

        LookingAt = (y >= 315) ? LookDirection.North : (y >= 225 ? LookDirection.West : (y >= 135 ? LookDirection.South : (y >= 45 ? LookDirection.East : LookDirection.North)));
    }
}
