using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public class CEditorManager
{
    private CEditorManager() { }
    private static CEditorManager instance;
    public static CEditorManager Instance { get => instance ?? (instance = new CEditorManager()); }

    private CEditorManagerBootstrapper bootstrapper;
    public CEditorInput cEditorInput { get; private set; }

    private List<Vector3Int> cPositions = new List<Vector3Int>();
    private List<GameObject> cCubes = new List<GameObject>();

    private float currentOperatingHeight = 0f;
    private Vector3 currentOperatingPosition = new Vector3(0f, 0f, 0f);
    private float buttonDelay = 0f;

    public float CurrentOperatingHeight { get => currentOperatingHeight; private set => ChangeOperatingHeight(currentOperatingHeight = value); }
    public Vector3 CurrentOperatingPosition { get => currentOperatingPosition; private set => ChangeOperatingPosition(currentOperatingPosition = value); }

    internal void RegisterBootstrapper(CEditorManagerBootstrapper bootstrapper)
    {
        this.bootstrapper = bootstrapper;
        cEditorInput = new CEditorInput();
        cEditorInput.Start(this.bootstrapper.Input);
        bootstrapper.Input.General.DPadInput.performed += ChangeOperatingPosition;
        bootstrapper.Input.CEditor.LeftShoulder.performed += DecreaseOperationHeight;
        bootstrapper.Input.CEditor.RightShoulder.performed += IncreaseOperatingHeight;
        bootstrapper.Input.CEditor.SouthButton.performed += AddCube;
        bootstrapper.Input.CEditor.EastButton.performed += RemoveCube;
        CEditorStart();
    }
    internal void UnregisterBootstrapper()
    {
        cEditorInput.End();
        bootstrapper.Input.General.DPadInput.performed -= ChangeOperatingPosition;
        bootstrapper.Input.CEditor.LeftShoulder.performed -= DecreaseOperationHeight;
        bootstrapper.Input.CEditor.RightShoulder.performed -= IncreaseOperatingHeight;
        bootstrapper.Input.CEditor.SouthButton.performed -= AddCube;
        bootstrapper.Input.CEditor.EastButton.performed -= RemoveCube;
        cEditorInput = null;
        bootstrapper = null;
    }

    internal void CEditorStart()
    {
        DrawGutter(currentOperatingHeight, bootstrapper.Dimension,bootstrapper.Dimension);
        DrawWireframe(currentOperatingPosition);
    }

    internal void CEditorUpdate()
    {
        if (buttonDelay > 0)
            buttonDelay -= Time.deltaTime;
        bootstrapper.LookDirection.text = bootstrapper.CamMovement.LookingAt.ToString();
    }

    private void ChangeOperatingHeight(float newHeight)
    {
        ClearDrawings();
        DrawGutter(newHeight, bootstrapper.Dimension,bootstrapper.Dimension);
        CurrentOperatingPosition = new Vector3(CurrentOperatingPosition.x, CurrentOperatingHeight, CurrentOperatingPosition.z);
    }

    private void IncreaseOperatingHeight(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = bootstrapper.ButtonDelayAmount;
        ChangeOperatingHeight(++currentOperatingHeight);
    }

    private void DecreaseOperationHeight(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = bootstrapper.ButtonDelayAmount;
        ChangeOperatingHeight(--currentOperatingHeight);
    }

    private void ChangeOperatingPosition(Vector3 position)
    {
        CEditorLineDrawer.ClearWireframe();
        DrawWireframe(position);

        bootstrapper.CurrentWorkingPosition.text = CurrentOperatingPosition.ToString();
    }

    private void ChangeOperatingPosition(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = bootstrapper.ButtonDelayAmount;
        Vector2 input = ctx.ReadValue<Vector2>();

        int y = 0;
        int x = 0;

        switch (bootstrapper.CamMovement.LookingAt)
        {
            case LookDirection.North:
                x = (int)input.x;
                y = (int)input.y;
                break;
            case LookDirection.South:
                x = -(int)input.x;
                y = -(int)input.y;
                break;
            case LookDirection.East:
                y = -(int)input.x;
                x = (int)input.y;
                break;
            case LookDirection.West:
                y = (int)input.x;
                x = -(int)input.y;
                break;
            default:
                break;
        }
        

        CurrentOperatingPosition = new Vector3(currentOperatingPosition.x + x, currentOperatingHeight, currentOperatingPosition.z + y);
    }

    private void AddCube(InputAction.CallbackContext ctx)
    {
        if (!cPositions.Contains(ConvertVec(currentOperatingPosition)))
        {
            cPositions.Add(ConvertVec(currentOperatingPosition));
            var primCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primCube.transform.position = CurrentOperatingPosition;
            cCubes.Add(primCube);
        }
        else
            Debug.Log("Already Exists");
    }

    private void RemoveCube(InputAction.CallbackContext ctx)
    {
        if (cPositions.Contains(ConvertVec(currentOperatingPosition)))
        {
            int index = cPositions.IndexOf(ConvertVec(currentOperatingPosition));
            cPositions.RemoveAt(index);
            GameObject primCube = cCubes[index];
            cCubes.RemoveAt(index);
            GameObject.Destroy(primCube);
        }
        else
            Debug.Log("Nothing to Remove");
    }

    #region LineDrawingMethods

    public static void DrawLine(Vector3 start, Vector3 finish) => CEditorLineDrawer.lines.Add(new CEditorLineDrawer.CEditorLine(start, finish, Color.black));

    private static void DrawLine(float startX, float startY, float endX, float endY, float height, bool top) => DrawLine(new Vector3(startX + 0.5f, height, startY + 0.5f), new Vector3(endX + 0.5f, height, endY + 0.5f));

    public static void DrawGutter(float height, float length, float width)
    {
        for (int i = -((int)width / 2); i < ((int)width / 2)+1; i++)
        {
            DrawLine(i, (int)length / 2, i, -((int)length / 2), height, false);
        }
        for (int i = -((int)length / 2); i < ((int)length / 2)+1; i++)
        {
            DrawLine((int)width / 2, i, -(int)width / 2, i, height, false);
        }
    }

    public static void ClearDrawings() => CEditorLineDrawer.ClearLines();

    public static void DrawWireframe(Vector3 position)
    {
        Vector3 point1 = new Vector3(position.x - 0.5f, position.y - 0.5f, position.z - 0.5f);
        Vector3 point2 = new Vector3(position.x + 0.5f, position.y - 0.5f, position.z - 0.5f);
        Vector3 point3 = new Vector3(position.x + 0.5f, position.y + 0.5f, position.z - 0.5f);
        Vector3 point4 = new Vector3(position.x - 0.5f, position.y + 0.5f, position.z - 0.5f);
                                                                                        
        Vector3 point5 = new Vector3(position.x - 0.5f, position.y - 0.5f, position.z + 0.5f);
        Vector3 point6 = new Vector3(position.x + 0.5f, position.y - 0.5f, position.z + 0.5f);
        Vector3 point7 = new Vector3(position.x + 0.5f, position.y + 0.5f, position.z + 0.5f);
        Vector3 point8 = new Vector3(position.x - 0.5f, position.y + 0.5f, position.z + 0.5f);

        DrawWireframeLine(point1, point2);
        DrawWireframeLine(point2, point3);
        DrawWireframeLine(point3, point4);
        DrawWireframeLine(point4, point1);

        DrawWireframeLine(point5, point6);
        DrawWireframeLine(point6, point7);
        DrawWireframeLine(point7, point8);
        DrawWireframeLine(point8, point5);

        DrawWireframeLine(point1, point5);
        DrawWireframeLine(point2, point6);
        DrawWireframeLine(point3, point7);
        DrawWireframeLine(point4, point8);
    }

    private static void DrawWireframeLine(Vector3 start, Vector3 finish) => CEditorLineDrawer.wireframe.Add(new CEditorLineDrawer.CEditorLine(start, finish, Color.green));

    #endregion

    private static Vector3Int ConvertVec(Vector3 vector)
    {
        return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
    }
}
