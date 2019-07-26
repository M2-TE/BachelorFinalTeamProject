using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public enum EditorEffectSound { ADD, REMOVE, OPENMENU, CLOSEMENU, ERROR, CONFIRM, SAVE, SWIPE, SWIPEERROR, CHANGECOLOR }

public class CEditorManager
{
    private CEditorManager() { }
    private static CEditorManager instance;
    public static CEditorManager Instance { get => instance ?? (instance = new CEditorManager()); }

    private CEditorManagerBootstrapper bootstrapper;
    public CEditorInput EditorInput { get; private set; }

    private CScriptableCharacter character;

    private List<Vector3Int> cPositions = new List<Vector3Int>();
    private List<GameObject> cCubes = new List<GameObject>();

    private float currentOperatingHeight = 0f;
    private Vector3 currentOperatingPosition = new Vector3(0f, 0f, 0f);
    private float buttonDelay = 0f;

    private int currentCharColor = 0;

    private bool changesSinceLastSave = false;

    public float CurrentOperatingHeight { get => currentOperatingHeight; private set => ChangeOperatingHeight(currentOperatingHeight = value); }
    public Vector3 CurrentOperatingPosition { get => currentOperatingPosition; private set => ChangeOperatingPosition(currentOperatingPosition = value); }

    internal void RegisterBootstrapper(CEditorManagerBootstrapper bootstrapper)
    {
        this.bootstrapper = bootstrapper;
        EditorInput = new CEditorInput();
        EditorInput.Start(this.bootstrapper.Input);
        bootstrapper.Input.General.DPadInput.performed += ChangeOperatingPosition;
        bootstrapper.Input.CEditor.LeftShoulder.performed += DecreaseOperationHeight;
        bootstrapper.Input.CEditor.RightShoulder.performed += IncreaseOperatingHeight;
        bootstrapper.Input.CEditor.SouthButton.performed += AddCube;
        bootstrapper.Input.CEditor.EastButton.performed += RemoveCube;
        bootstrapper.Input.CEditor.WestButton.performed += DecreaseColor;
        bootstrapper.Input.CEditor.NorthButton.performed += IncreaseColor;
        CEditorStart();
    }

    internal void UnregisterBootstrapper()
    {
        EditorInput.End();
        bootstrapper.Input.General.DPadInput.performed -= ChangeOperatingPosition;
        bootstrapper.Input.CEditor.LeftShoulder.performed -= DecreaseOperationHeight;
        bootstrapper.Input.CEditor.RightShoulder.performed -= IncreaseOperatingHeight;
        bootstrapper.Input.CEditor.SouthButton.performed -= AddCube;
        bootstrapper.Input.CEditor.EastButton.performed -= RemoveCube;
        bootstrapper.Input.CEditor.WestButton.performed -= DecreaseColor;
        bootstrapper.Input.CEditor.NorthButton.performed -= IncreaseColor;
        EditorInput = null;
        bootstrapper = null;
    }

    internal void CEditorStart()
    {
        DrawGutter(currentOperatingHeight, bootstrapper.Dimension,bootstrapper.Dimension);
        DrawWireframe(currentOperatingPosition);
        currentCharColor = 0;
        bootstrapper.EditorMenu.SetActive(EditorInput.LeftButton);
        character = ScriptableObject.CreateInstance<CScriptableCharacter>();
        character.GenerateNewGuid();
    }

    internal void CEditorUpdate()
    {
        if (buttonDelay > 0)
            buttonDelay -= Time.deltaTime;
        bootstrapper.LookDirection.text = bootstrapper.CamMovement.LookingAt.ToString();
    }

    internal void ManageMenu()
    {
        bootstrapper.EditorMenu.SetActive(EditorInput.LeftButton);
        PlayEditorSound(EditorInput.LeftButton ? EditorEffectSound.OPENMENU : EditorEffectSound.CLOSEMENU);
    }

    internal void SaveCharacter()
    {
        if (cPositions.Count > 0 && changesSinceLastSave)
        {
            character.CharacterScaling = GetCharacterScaling();
            character.CharacterColor = currentCharColor;
            character.Offset = GetCharacterOffset();
            character.CubePositions = cPositions.ToArray();
            GameManager.Instance.ContentHolder.AddCharacter(character);
            GameManager.Instance.SaveStreamingAssets();
            PlayEditorSound(EditorEffectSound.SAVE);
        }
        else
            PlayEditorSound(EditorEffectSound.ERROR);
    }

    internal void DeleteCharacter()
    {
        GameManager.Instance.ContentHolder.RemoveCharacter(character);
        currentCharColor = 0;
        foreach (var cube in cCubes)
        {
            GameObject.Destroy(cube);
        }
        character.GenerateNewGuid();
        cPositions = new List<Vector3Int>();
        cCubes = new List<GameObject>();
        PlayEditorSound(EditorEffectSound.CONFIRM);
    }

    internal void CopyCharacter()
    {
        SaveCharacter();
        character.GenerateNewGuid();
        PlayEditorSound(EditorEffectSound.CONFIRM);
    }

    internal void ReloadScene()
    {
        PlayEditorSound(EditorEffectSound.CONFIRM);
        bootstrapper.EditorMenu.SetActive(EditorInput.LeftButton);
        GameManager.Instance.LoadScene(2);
    }

    private void ChangeOperatingHeight(float newHeight)
    {
        ClearDrawings();
        DrawGutter(newHeight, bootstrapper.Dimension,bootstrapper.Dimension);
        CurrentOperatingPosition = new Vector3(CurrentOperatingPosition.x, CurrentOperatingHeight, CurrentOperatingPosition.z);
    }

    private void IncreaseOperatingHeight(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0 || EditorInput.LeftButton)
            return;
        else
            buttonDelay = bootstrapper.ButtonDelayAmount;
        ChangeOperatingHeight(++currentOperatingHeight);
    }

    private void DecreaseOperationHeight(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0 ||  EditorInput.LeftButton)
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

        if (EditorInput.LeftButton)
        {
            y = (int)input.y;
            int pos = bootstrapper.EditorMenu.MenuPosition;
            if (y != 0)
                bootstrapper.EditorMenu.MenuPosition = y < 0 ? (int)(Mathf.Min(bootstrapper.EditorMenu.MenuOptions.Length - 1, bootstrapper.EditorMenu.MenuPosition + 1)) : (int)(Mathf.Max(bootstrapper.EditorMenu.MenuPosition - 1, 0));
            if (pos == bootstrapper.EditorMenu.MenuPosition)
                PlayEditorSound(EditorEffectSound.SWIPEERROR);
            else
                PlayEditorSound(EditorEffectSound.SWIPE);
            return;
        }


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

    private void IncreaseColor(InputAction.CallbackContext ctx)
    {
        currentCharColor++;
        currentCharColor = currentCharColor >= GameManager.Instance.CharacterMaterials.Length ? 0 : currentCharColor;
        PlayEditorSound(EditorEffectSound.CHANGECOLOR);
        ChangeColor();
        changesSinceLastSave = true;
    }

    private void DecreaseColor(InputAction.CallbackContext ctx)
    {
        currentCharColor--;
        currentCharColor = currentCharColor < 0 ? GameManager.Instance.CharacterMaterials.Length - 1 : currentCharColor;
        PlayEditorSound(EditorEffectSound.CHANGECOLOR);
        ChangeColor();
        changesSinceLastSave = true;
    }

    private void ChangeColor()
    {
        Material nextMaterial = GameManager.Instance.CharacterMaterials[currentCharColor];
        foreach (var cube in cCubes)
        {
            cube.GetComponent<MeshRenderer>().material = nextMaterial;
        }
    }
    
    private void AddCube(InputAction.CallbackContext ctx)
    {
        if (CEditorManager.Instance.EditorInput.LeftButton)
        {
            bootstrapper.EditorMenu.Confirm(this);
            return;
        }
        if (!cPositions.Contains(ConvertVec(currentOperatingPosition)))
        {
            PlayEditorSound(EditorEffectSound.ADD);
            cPositions.Add(ConvertVec(currentOperatingPosition));
            var primCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primCube.transform.position = CurrentOperatingPosition;
            primCube.GetComponent<MeshRenderer>().material = GameManager.Instance.CharacterMaterials[currentCharColor];
            cCubes.Add(primCube);
            changesSinceLastSave = true;
        }
        else
            PlayEditorSound(EditorEffectSound.ERROR);
    }

    private void RemoveCube(InputAction.CallbackContext ctx)
    {
        if (CEditorManager.Instance.EditorInput.LeftButton)
        {
            bootstrapper.EditorMenu.Decline(this);
            return;
        }
        if (cPositions.Contains(ConvertVec(currentOperatingPosition)))
        {
            PlayEditorSound(EditorEffectSound.REMOVE);
            int index = cPositions.IndexOf(ConvertVec(currentOperatingPosition));
            cPositions.RemoveAt(index);
            GameObject primCube = cCubes[index];
            cCubes.RemoveAt(index);
            GameObject.Destroy(primCube);
            changesSinceLastSave = true;
        }
        else
            PlayEditorSound(EditorEffectSound.ERROR);
    }

    private int GetCharacterScaling()
    {
        int negY = cPositions[0].y, posY = cPositions[0].y, negX = cPositions[0].x, posX = cPositions[0].x, negZ = cPositions[0].z, posZ = cPositions[0].z;
        foreach (var item in cPositions)
        {
            negY = negY > item.y ? item.y : negY;
            posY = posY < item.y ? item.y : posY;
            negX = negX > item.x ? item.x : negX;
            posX = posX < item.x ? item.x : posX;
            negZ = negZ > item.z ? item.z : negZ;
            posZ = posZ < item.z ? item.z : posZ;
        }
        int y = posY - negY + 1;
        int x = posX - negX + 1;
        int z = posZ - negZ + 1;
        Debug.Log("XDiff: " + x + " | YDiff: " + y + " | ZDiff: " + z);
        y = (y + (y % 2)) / 2;
        int scaling = y;
        scaling = scaling < x ? x : scaling;
        scaling = scaling < z ? z : scaling;
        return scaling;
    }

    private Vector3 GetCharacterOffset()
    {
        int negY = cPositions[0].y, negX = cPositions[0].x, posX = cPositions[0].x, negZ = cPositions[0].z, posZ = cPositions[0].z;
        foreach (var item in cPositions)
        {
            negY = negY > item.y ? item.y : negY;
            negX = negX > item.x ? item.x : negX;
            posX = posX < item.x ? item.x : posX;
            negZ = negZ > item.z ? item.z : negZ;
            posZ = posZ < item.z ? item.z : posZ;
        }
        int x = posX - negX + 1;
        int z = posZ - negZ + 1;
        float xOff = x!= 1 ? (negX + ((x-1f) / 2f)) : 0f;
        float zOff = z!= 1 ? (negZ + ((z-1f) / 2f)) : 0f;
        return new Vector3(xOff,negY, zOff);
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

    public void PlayEditorSound(EditorEffectSound sound)
    {
        AudioClip clip = bootstrapper.Error[0];
        Vector3 position = bootstrapper.mainCamera.position + Vector3.forward;
        switch (sound)
        {
            case EditorEffectSound.ADD:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.AddCube);
                position = currentOperatingPosition;
                break;
            case EditorEffectSound.REMOVE:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.RemoveCube);
                position = currentOperatingPosition;
                break;
            case EditorEffectSound.OPENMENU:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.OpenMenu);
                break;
            case EditorEffectSound.CLOSEMENU:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.CloseMenu);
                break;
            case EditorEffectSound.ERROR:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.Error);
                break;
            case EditorEffectSound.CONFIRM:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.Confirm);
                break;
            case EditorEffectSound.SAVE:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.Save);
                break;
            case EditorEffectSound.SWIPE:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.Swipe);
                break;
            case EditorEffectSound.SWIPEERROR:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.SwipeError);
                break;
            case EditorEffectSound.CHANGECOLOR:
                clip = Utilities.PickAtRandom<AudioClip>(bootstrapper.ChangeColor);
                break;
            default:
                break;
        }
        OneShotAudioManager.PlayOneShotAudio(clip, position,1);
    }
}
