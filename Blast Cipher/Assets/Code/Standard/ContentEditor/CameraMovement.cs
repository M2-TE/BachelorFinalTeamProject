using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LookDirection { North, South, East, West}

public class CameraMovement : MonoBehaviour
{
    [SerializeField][Range(10f, 30f)] private float movementSpeed;
    [SerializeField][Range(10f, 100f)] private float rotationSpeed;

    public LookDirection LookingAt;

    private void LateUpdate()
    {
        if (!CEditorManager.Instance.EditorInput.LeftButton)
        {
            Vector2 position = CEditorManager.Instance.EditorInput.LeftStick.normalized;
            Vector2 rotation = CEditorManager.Instance.EditorInput.RightStick.normalized;
            float upMovement = CEditorManager.Instance.EditorInput.RightTrigger;
            float downMovement = CEditorManager.Instance.EditorInput.LeftTrigger;

            Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1f, 0f, 1f)).normalized * position.y;
            Vector3 right = Vector3.Scale(transform.right, new Vector3(1f, 0f, 1f)).normalized * position.x;
            Vector3 up = Vector3.up * (upMovement - downMovement);

            if (!CEditorManager.Instance.EditorInput.LeftStickPress)
            {
                forward = transform.forward.normalized * position.y;
                right = transform.right * position.x;
                up = transform.up * (upMovement - downMovement);
            }

            transform.position += (forward + right + up).normalized * movementSpeed * Time.deltaTime;
            transform.Rotate(0f, rotation.x * rotationSpeed * Time.deltaTime, 0f, Space.World);
            transform.Rotate(-rotation.y * rotationSpeed * Time.deltaTime, 0f, 0f, Space.Self);
        }

        float y = transform.rotation.eulerAngles.y;

        LookingAt = (y >= 315) ? LookDirection.North : (y >= 225 ? LookDirection.West : (y >= 135 ? LookDirection.South : (y >= 45 ? LookDirection.East : LookDirection.North)));
    }
}
