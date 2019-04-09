// GENERATED AUTOMATICALLY FROM 'Assets/Input System/Input Actions.inputactions'

using System;
using UnityEngine;
using UnityEngine.Experimental.Input;


[Serializable]
public class InputActions : InputActionAssetReference
{
    public InputActions()
    {
    }
    public InputActions(InputActionAsset asset)
        : base(asset)
    {
    }
    [NonSerialized] private bool m_Initialized;
    private void Initialize()
    {
        // Main
        m_Main = asset.GetActionMap("Main");
        m_Main_MovementHorizontal = m_Main.GetAction("Movement Horizontal");
        m_Main_Shoot = m_Main.GetAction("Shoot");
        m_Initialized = true;
    }
    private void Uninitialize()
    {
        m_Main = null;
        m_Main_MovementHorizontal = null;
        m_Main_Shoot = null;
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
    // Main
    private InputActionMap m_Main;
    private InputAction m_Main_MovementHorizontal;
    private InputAction m_Main_Shoot;
    public struct MainActions
    {
        private InputActions m_Wrapper;
        public MainActions(InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @MovementHorizontal { get { return m_Wrapper.m_Main_MovementHorizontal; } }
        public InputAction @Shoot { get { return m_Wrapper.m_Main_Shoot; } }
        public InputActionMap Get() { return m_Wrapper.m_Main; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(MainActions set) { return set.Get(); }
    }
    public MainActions @Main
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new MainActions(this);
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get

        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.GetControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    private int m_KeyboardAndMouseSchemeIndex = -1;
    public InputControlScheme KeyboardAndMouseScheme
    {
        get

        {
            if (m_KeyboardAndMouseSchemeIndex == -1) m_KeyboardAndMouseSchemeIndex = asset.GetControlSchemeIndex("KeyboardAndMouse");
            return asset.controlSchemes[m_KeyboardAndMouseSchemeIndex];
        }
    }
}
