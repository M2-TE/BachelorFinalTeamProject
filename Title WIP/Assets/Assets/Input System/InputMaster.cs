// GENERATED AUTOMATICALLY FROM 'Assets/Input System/InputMaster.inputactions'

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
        m_Player_Shoot = m_Player.GetAction("Shoot");
        m_Player_Aim = m_Player.GetAction("Aim");
        // General
        m_General = asset.GetActionMap("General");
        m_General_RegisterDevice = m_General.GetAction("RegisterDevice");
        m_Initialized = true;
    }
    private void Uninitialize()
    {
        m_Player = null;
        m_Player_Movement = null;
        m_Player_Shoot = null;
        m_Player_Aim = null;
        m_General = null;
        m_General_RegisterDevice = null;
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
    private InputAction m_Player_Shoot;
    private InputAction m_Player_Aim;
    public struct PlayerActions
    {
        private InputMaster m_Wrapper;
        public PlayerActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement { get { return m_Wrapper.m_Player_Movement; } }
        public InputAction @Shoot { get { return m_Wrapper.m_Player_Shoot; } }
        public InputAction @Aim { get { return m_Wrapper.m_Player_Aim; } }
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
    public struct GeneralActions
    {
        private InputMaster m_Wrapper;
        public GeneralActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @RegisterDevice { get { return m_Wrapper.m_General_RegisterDevice; } }
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
}