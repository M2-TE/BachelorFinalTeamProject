// GENERATED AUTOMATICALLY FROM 'Assets/Third Party Assets/Input System/InputMaster.inputactions'

using System;
using UnityEngine;
using UnityEngine.Experimental.Input;


[Serializable]
public class InputMaster : InputActionAssetReference
{
    public InputMaster()
    {
    }
    public InputMaster(InputActionAsset asset)
        : base(asset)
    {
    }
    [NonSerialized] private bool m_Initialized;
    private void Initialize()
    {
        // Player
        m_Player = asset.GetActionMap("Player");
        m_Player_Movement = m_Player.GetAction("Movement");
        m_Player_Aim = m_Player.GetAction("Aim");
        m_Player_Debug = m_Player.GetAction("Debug");
        m_Player_Jump = m_Player.GetAction("Jump");
        m_Player_Parry = m_Player.GetAction("Parry");
        m_Player_LockAim = m_Player.GetAction("LockAim");
        m_Player_Portal = m_Player.GetAction("Portal");
        m_Player_Shoot = m_Player.GetAction("Shoot");
        m_Player_ProjectileCollect = m_Player.GetAction("ProjectileCollect");
        // General
        m_General = asset.GetActionMap("General");
        m_General_RegisterDevice = m_General.GetAction("RegisterDevice");
        m_General_DPadInput = m_General.GetAction("DPadInput");
        m_General_Start = m_General.GetAction("Start");
        // CEditor
        m_CEditor = asset.GetActionMap("CEditor");
        m_CEditor_LeftStick = m_CEditor.GetAction("LeftStick");
        m_CEditor_RightStick = m_CEditor.GetAction("RightStick");
        m_CEditor_NorthButton = m_CEditor.GetAction("NorthButton");
        m_CEditor_SouthButton = m_CEditor.GetAction("SouthButton");
        m_CEditor_LeftTrigger = m_CEditor.GetAction("LeftTrigger");
        m_CEditor_RightStickPress = m_CEditor.GetAction("RightStickPress");
        m_CEditor_LeftShoulder = m_CEditor.GetAction("LeftShoulder");
        m_CEditor_RightShoulder = m_CEditor.GetAction("RightShoulder");
        m_CEditor_RightTrigger = m_CEditor.GetAction("RightTrigger");
        m_CEditor_EastButton = m_CEditor.GetAction("EastButton");
        m_CEditor_LeftStickPress = m_CEditor.GetAction("LeftStickPress");
        m_CEditor_WestButton = m_CEditor.GetAction("WestButton");
        m_CEditor_Start = m_CEditor.GetAction("Start");
        m_CEditor_Select = m_CEditor.GetAction("Select");
        m_Initialized = true;
    }
    private void Uninitialize()
    {
        m_Player = null;
        m_Player_Movement = null;
        m_Player_Aim = null;
        m_Player_Debug = null;
        m_Player_Jump = null;
        m_Player_Parry = null;
        m_Player_LockAim = null;
        m_Player_Portal = null;
        m_Player_Shoot = null;
        m_Player_ProjectileCollect = null;
        m_General = null;
        m_General_RegisterDevice = null;
        m_General_DPadInput = null;
        m_General_Start = null;
        m_CEditor = null;
        m_CEditor_LeftStick = null;
        m_CEditor_RightStick = null;
        m_CEditor_NorthButton = null;
        m_CEditor_SouthButton = null;
        m_CEditor_LeftTrigger = null;
        m_CEditor_RightStickPress = null;
        m_CEditor_LeftShoulder = null;
        m_CEditor_RightShoulder = null;
        m_CEditor_RightTrigger = null;
        m_CEditor_EastButton = null;
        m_CEditor_LeftStickPress = null;
        m_CEditor_WestButton = null;
        m_CEditor_Start = null;
        m_CEditor_Select = null;
        m_Initialized = false;
    }
    public void SetAsset(InputActionAsset newAsset)
    {
        if (newAsset == asset) return;
        if (m_Initialized) Uninitialize();
        asset = newAsset;
    }
    public override void MakePrivateCopyOfActions()
    {
        SetAsset(ScriptableObject.Instantiate(asset));
    }
    // Player
    private InputActionMap m_Player;
    private InputAction m_Player_Movement;
    private InputAction m_Player_Aim;
    private InputAction m_Player_Debug;
    private InputAction m_Player_Jump;
    private InputAction m_Player_Parry;
    private InputAction m_Player_LockAim;
    private InputAction m_Player_Portal;
    private InputAction m_Player_Shoot;
    private InputAction m_Player_ProjectileCollect;
    public struct PlayerActions
    {
        private InputMaster m_Wrapper;
        public PlayerActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement { get { return m_Wrapper.m_Player_Movement; } }
        public InputAction @Aim { get { return m_Wrapper.m_Player_Aim; } }
        public InputAction @Debug { get { return m_Wrapper.m_Player_Debug; } }
        public InputAction @Jump { get { return m_Wrapper.m_Player_Jump; } }
        public InputAction @Parry { get { return m_Wrapper.m_Player_Parry; } }
        public InputAction @LockAim { get { return m_Wrapper.m_Player_LockAim; } }
        public InputAction @Portal { get { return m_Wrapper.m_Player_Portal; } }
        public InputAction @Shoot { get { return m_Wrapper.m_Player_Shoot; } }
        public InputAction @ProjectileCollect { get { return m_Wrapper.m_Player_ProjectileCollect; } }
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
    }
    public PlayerActions @Player
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new PlayerActions(this);
        }
    }
    // General
    private InputActionMap m_General;
    private InputAction m_General_RegisterDevice;
    private InputAction m_General_DPadInput;
    private InputAction m_General_Start;
    public struct GeneralActions
    {
        private InputMaster m_Wrapper;
        public GeneralActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @RegisterDevice { get { return m_Wrapper.m_General_RegisterDevice; } }
        public InputAction @DPadInput { get { return m_Wrapper.m_General_DPadInput; } }
        public InputAction @Start { get { return m_Wrapper.m_General_Start; } }
        public InputActionMap Get() { return m_Wrapper.m_General; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(GeneralActions set) { return set.Get(); }
    }
    public GeneralActions @General
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new GeneralActions(this);
        }
    }
    // CEditor
    private InputActionMap m_CEditor;
    private InputAction m_CEditor_LeftStick;
    private InputAction m_CEditor_RightStick;
    private InputAction m_CEditor_NorthButton;
    private InputAction m_CEditor_SouthButton;
    private InputAction m_CEditor_LeftTrigger;
    private InputAction m_CEditor_RightStickPress;
    private InputAction m_CEditor_LeftShoulder;
    private InputAction m_CEditor_RightShoulder;
    private InputAction m_CEditor_RightTrigger;
    private InputAction m_CEditor_EastButton;
    private InputAction m_CEditor_LeftStickPress;
    private InputAction m_CEditor_WestButton;
    private InputAction m_CEditor_Start;
    private InputAction m_CEditor_Select;
    public struct CEditorActions
    {
        private InputMaster m_Wrapper;
        public CEditorActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @LeftStick { get { return m_Wrapper.m_CEditor_LeftStick; } }
        public InputAction @RightStick { get { return m_Wrapper.m_CEditor_RightStick; } }
        public InputAction @NorthButton { get { return m_Wrapper.m_CEditor_NorthButton; } }
        public InputAction @SouthButton { get { return m_Wrapper.m_CEditor_SouthButton; } }
        public InputAction @LeftTrigger { get { return m_Wrapper.m_CEditor_LeftTrigger; } }
        public InputAction @RightStickPress { get { return m_Wrapper.m_CEditor_RightStickPress; } }
        public InputAction @LeftShoulder { get { return m_Wrapper.m_CEditor_LeftShoulder; } }
        public InputAction @RightShoulder { get { return m_Wrapper.m_CEditor_RightShoulder; } }
        public InputAction @RightTrigger { get { return m_Wrapper.m_CEditor_RightTrigger; } }
        public InputAction @EastButton { get { return m_Wrapper.m_CEditor_EastButton; } }
        public InputAction @LeftStickPress { get { return m_Wrapper.m_CEditor_LeftStickPress; } }
        public InputAction @WestButton { get { return m_Wrapper.m_CEditor_WestButton; } }
        public InputAction @Start { get { return m_Wrapper.m_CEditor_Start; } }
        public InputAction @Select { get { return m_Wrapper.m_CEditor_Select; } }
        public InputActionMap Get() { return m_Wrapper.m_CEditor; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(CEditorActions set) { return set.Get(); }
    }
    public CEditorActions @CEditor
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new CEditorActions(this);
        }
    }
    private int m_MainGamepadSchemeSchemeIndex = -1;
    public InputControlScheme MainGamepadSchemeScheme
    {
        get

        {
            if (m_MainGamepadSchemeSchemeIndex == -1) m_MainGamepadSchemeSchemeIndex = asset.GetControlSchemeIndex("MainGamepadScheme");
            return asset.controlSchemes[m_MainGamepadSchemeSchemeIndex];
        }
    }
}
