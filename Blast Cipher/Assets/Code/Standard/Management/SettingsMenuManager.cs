using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Experimental.Input;

public enum PauseMenuState { Continue, Music, Sounds, MainMenu }

public class SettingsMenuManager : MonoBehaviour
{
	[SerializeField] private AudioMixerGroup soundMixer, musicMixer;
    [SerializeField] private GameObject toggleNode;
    [SerializeField] private InputMaster input;
    [SerializeField] private Material defaultMat, hightlightedMat;
    [SerializeField] private MaterialsHolder continueGame,musicVolume, soundsVolume, toMainMenu;
    [SerializeField] private GameObject[] sounds, music;
    [SerializeField] private AudioClip changeVolumeSound, swipe, swipeError;
    [SerializeField] private float inputDelay = 0.7f;

    private int soundSteps = 9, musicSteps = 9;

    private float buttonDelay = 0f;

    private PauseMenuState currentState = 0;

    public PauseMenuState CurrentState { get => currentState; private set => SetMaterials(currentState, currentState = value); }

    public float SoundEffectsVolume => soundSteps / 10f;
    public float MusicVolume => musicSteps / 10f;

    private void Awake()
    {
        toggleNode.SetActive(false);
	}

    private void OnEnable()
    {
        input.General.Start.performed += OnStartPressed;
    }

    private void OnDisable()
    {
        input.General.Start.performed -= OnStartPressed;
    }

    private void Update()
    {
        if (buttonDelay > 0)
            buttonDelay -= Time.unscaledDeltaTime;
    }

    private void SetMaterials(PauseMenuState defaultMatState, PauseMenuState highlightedMatState)
    {
        SetMaterial(defaultMatState, defaultMat);
        SetMaterial(highlightedMatState, hightlightedMat);
    }

    private void SetMaterial(PauseMenuState state, Material mat)
    {
        switch (state)
        {
            case PauseMenuState.Continue:
                continueGame.SetMaterials(mat);
                break;
            case PauseMenuState.Music:
                musicVolume.SetMaterials(mat);
                break;
            case PauseMenuState.Sounds:
                soundsVolume.SetMaterials(mat);
                break;
            case PauseMenuState.MainMenu:
                toMainMenu.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    public void OnStartPressed(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = inputDelay;

        if (!toggleNode.activeInHierarchy)
            Activate();
    }

    public void OnConfirm(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = inputDelay;

        switch (currentState)
        {
            case PauseMenuState.Continue:
                Deactivate();
                break;
            case PauseMenuState.MainMenu:
                BackToMainMenu();
                break;
            default:
                break;
        }
    }

    public void OnDPadInput(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = inputDelay;

        Vector2 input = ctx.ReadValue<Vector2>();
         if (input.y > 0)
             ChangeState(false);
         else if (input.y < 0)
             ChangeState(true);
         else if (input.x < 0 && currentState.Equals(PauseMenuState.Music))
             Decrease(true);
         else if (input.x < 0 && currentState.Equals(PauseMenuState.Sounds))
             Decrease(false);
         else if (input.x > 0 && currentState.Equals(PauseMenuState.Music))
             Increase(true);
         else if (input.x > 0 && currentState.Equals(PauseMenuState.Sounds))
             Increase(false);
    }

    private void Activate()
    {
        Time.timeScale = 0f;
        CurrentState = PauseMenuState.Continue;
        toggleNode.SetActive(true);
        input.CEditor.SouthButton.performed += OnConfirm;
        input.General.DPadInput.performed += OnDPadInput;
    }

    private void Deactivate()
    {
        input.CEditor.SouthButton.performed -= OnConfirm;
        input.General.DPadInput.performed -= OnDPadInput;
        toggleNode.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ChangeState(bool increment)
    {
        int audioTemp = (int)currentState;
        if (increment)
        {
            CurrentState = (int)currentState + 1 >= System.Enum.GetValues(typeof(PauseMenuState)).Length ? currentState : currentState + 1;
        }
        else
        {
            CurrentState = (int)currentState - 1 < 0 ? currentState : currentState - 1;
        }
        OneShotAudioManager.PlayOneShotAudio(audioTemp == (int)currentState ? swipeError : swipe, transform.position, SoundEffectsVolume);
    }

    private void Decrease(bool isMusic)
    {
        if (isMusic)
        {
            musicSteps--;
            if (musicSteps < 0)
                musicSteps++;
            else
                music[musicSteps+1].SetActive(false);
            OneShotAudioManager.PlayOneShotAudio(changeVolumeSound, transform.position, MusicVolume);

			float vol = Mathf.Lerp(-70f, -20f, MusicVolume);
			musicMixer.audioMixer.SetFloat("MusicVolume", vol);

		}
        else
        {
            soundSteps--;
            if (soundSteps < 0)
                soundSteps++;
            else
                sounds[soundSteps + 1].SetActive(false);
            OneShotAudioManager.PlayOneShotAudio(changeVolumeSound, transform.position, SoundEffectsVolume);

			float vol = Mathf.Lerp(-40f, 10f, SoundEffectsVolume);
			musicMixer.audioMixer.SetFloat("SfxVolume", vol);
		}
    }

    private void Increase(bool isMusic)
    {
        if (isMusic)
        {
            musicSteps++;
            if (musicSteps > 9)
                musicSteps--;
            else
                music[musicSteps].SetActive(true);
            OneShotAudioManager.PlayOneShotAudio(changeVolumeSound, transform.position, MusicVolume);


			float vol = Mathf.Lerp(-70f, -20f, MusicVolume);
			musicMixer.audioMixer.SetFloat("MusicVolume", vol);
		}
        else
        {
            soundSteps++;
            if (soundSteps > 9)
                soundSteps--;
            else
                sounds[soundSteps].SetActive(true);
            OneShotAudioManager.PlayOneShotAudio(changeVolumeSound, transform.position, SoundEffectsVolume);

			float vol = Mathf.Lerp(-40f, 10f, SoundEffectsVolume);
			musicMixer.audioMixer.SetFloat("SfxVolume", vol);
		}
    }

    private void BackToMainMenu()
    {
        gameObject.SetActive(false);
        GameManager.Instance.LoadScene(0);
    }

}
