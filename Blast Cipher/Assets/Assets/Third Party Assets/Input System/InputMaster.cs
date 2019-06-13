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
        m_Player_PortalOne = m_Player.GetAction("PortalOne");
        m_Player_PortalTwo = m_Player.GetAction("PortalTwo");
        m_Player_Shoot = m_Player.GetAction("Shoot");
        m_Player_Decline = m_Player.GetAction("Decline");
        // General
        m_General = asset.GetActionMap("General");
        m_General_RegisterDevice = m_General.GetAction("RegisterDevice");
        m_General_DPadInput = m_General.GetAction("DPadInput");
        m_General_Start = m_General.GetAction("Start");
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
        m_Player_PortalOne = null;
        m_Player_PortalTwo = null;
        m_Player_Shoot = null;
        m_Player_Decline = null;
        m_General = null;
        m_General_RegisterDevice = null;
        m_General_DPadInput = null;
        m_General_Start = null;
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
    private InputAction m_Player_PortalOne;
    private InputAction m_Player_PortalTwo;
    private InputAction m_Player_Shoot;
    private InputAction m_Player_Decline;
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
        public InputAction @PortalOne { get { return m_Wrapper.m_Player_PortalOne; } }
        public InputAction @PortalTwo { get { return m_Wrapper.m_Player_PortalTwo; } }
        public InputAction @Shoot { get { return m_Wrapper.m_Player_Shoot; } }
        public InputAction @Decline { get { return m_Wrapper.m_Player_Decline; } }
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
