using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public enum GeneralRules { Header, Rounds, Map, Announcer, Reset, Start }
public enum CooldownRules { Header, Reload, Dash, Shield, Start}
public enum PowerUpRules { Header, Selection, Enabled, Spawnrate, Duration, Start }
public enum Rules { General, Cooldown, PowerUp }

public class RulesManager : MenuManager
{
    [SerializeField] private Rules standartState = 0;
    [Tooltip("[0] = RED/Bomb, [1] = GREEN/Bounce, [2] = BLUE/AutoAim")]
    [SerializeField] private Material[] powerUpMats;

    //General
    [SerializeField] private GameObject genTransform;
    [SerializeField] private MaterialsHolder headerGen, rounds, map, announcer, reset, startGen;
    [SerializeField] private GeneralRules standartStateGen = 0;
    [SerializeField] private Transform[] singleDigits, doubleDigits, maps, announcerChoice;

    //Cooldowns
    [SerializeField] private GameObject cdTransform;
    [SerializeField] private MaterialsHolder headerCD, reload, dash, shield, startCD;
    [SerializeField] private CooldownRules standartStateCD = 0;
    [SerializeField] private Transform[] reloadDigits, dashDigits, shieldDigits;

    //Powerups
    [SerializeField] private GameObject puTransform;
    [SerializeField] private MaterialsHolder headerPU, red, green, blue, enabledPU, spawnrate, duration, startPU, descriptors;
    [SerializeField] private PowerUpRules standartStatePU = 0;
    [SerializeField] private Transform[] enabledChoice, spawnDigits, durationDigits;
    private int currentPU = 0;

    [SerializeField] private LocalLobbyManager localLobbyManager;

    private MatchSettings settings;

    public float RulesAppearSpeed = 10f;

    #region CurrentStates

    private Rules currentState;

    public Rules CurrentState { get => currentState; private set => ChangePage(currentState,currentState = value); }

    private GeneralRules currentStateGen;

    public GeneralRules CurrentStateGen { get => currentStateGen; private set => SetMaterials(currentStateGen, currentStateGen = value); }

    private CooldownRules currentStateCD;

    public CooldownRules CurrentStateCD { get => currentStateCD; private set => SetMaterials(currentStateCD, currentStateCD = value); }

    private PowerUpRules currentStatePU;

    public PowerUpRules CurrentStatePU { get => currentStatePU; private set => SetMaterials(currentStatePU, currentStatePU = value); }

    #endregion

    #region SetMaterials

    private void SetMaterials(Rules currentState)
    {
        switch (currentState)
        {
            case Rules.General:
                CurrentStateGen = standartStateGen;
                break;
            case Rules.Cooldown:
                CurrentStateCD = standartStateCD;
                break;
            case Rules.PowerUp:
                CurrentStatePU = standartStatePU;
                break;
            default:
                break;
        }
    }

    private void SetMaterials(GeneralRules defaultMatState, GeneralRules highlightedMatState)
    {
        SetMaterials(defaultMatState, defaultMat);
        SetMaterials(highlightedMatState, highlightedMat);
    }

    private void SetMaterials(GeneralRules state, Material mat)
    {
        switch (state)
        {
            case GeneralRules.Rounds:
                rounds.SetMaterials(mat);
                break;
            case GeneralRules.Start:
                startGen.SetMaterials(mat);
                break;
            case GeneralRules.Header:
                headerGen.SetMaterials(mat);
                break;
            case GeneralRules.Map:
                map.SetMaterials(mat);
                break;
            case GeneralRules.Announcer:
                announcer.SetMaterials(mat);
                break;
            case GeneralRules.Reset:
                reset.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void SetMaterials(CooldownRules defaultMatState, CooldownRules highlightedMatState)
    {
        SetMaterials(defaultMatState, defaultMat);
        SetMaterials(highlightedMatState, highlightedMat);
    }

    private void SetMaterials(CooldownRules state, Material mat)
    {
        switch (state)
        {
            case CooldownRules.Header:
                headerCD.SetMaterials(mat);
                break;
            case CooldownRules.Reload:
                reload.SetMaterials(mat);
                break;
            case CooldownRules.Dash:
                dash.SetMaterials(mat);
                break;
            case CooldownRules.Shield:
                shield.SetMaterials(mat);
                break;
            case CooldownRules.Start:
                startCD.SetMaterials(mat);
                break;
        }
    }

    private void SetMaterials(PowerUpRules defaultMatState, PowerUpRules highlightedMatState)
    {
        SetMaterials(defaultMatState, defaultMat);
        SetMaterials(highlightedMatState, highlightedMat);
    }

    private void SetMaterials(PowerUpRules state, Material mat)
    {
        switch (state)
        {
            case PowerUpRules.Header:
                headerPU.SetMaterials(mat);
                break;
            case PowerUpRules.Selection:
                red.SetMaterials(mat);
                green.SetMaterials(mat);
                blue.SetMaterials(mat);
                break;
            case PowerUpRules.Enabled:
                enabledPU.SetMaterials(mat);
                break;
            case PowerUpRules.Spawnrate:
                spawnrate.SetMaterials(mat);
                break;
            case PowerUpRules.Duration:
                duration.SetMaterials(mat);
                break;
            case PowerUpRules.Start:
                startPU.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void SetPowerUpMaterials()
    {
        descriptors.SetMaterials(powerUpMats[currentPU]);
    }

    #endregion

    #region Change States (not proud of these)

    private void ChangeStateVertical(bool increment)
    {
        int audioTemp = 0;
        int audioAfterTemp = 0;
        switch (currentState)
        {
            case Rules.General:
                audioTemp = (int)CurrentStateGen;
                if (increment)
                    CurrentStateGen = ((int)CurrentStateGen) + 1 >= System.Enum.GetValues(typeof(GeneralRules)).Length ? CurrentStateGen : CurrentStateGen + 1;
                else
                    CurrentStateGen = ((int)CurrentStateGen) - 1 < 0 ? CurrentStateGen : CurrentStateGen - 1;
                audioAfterTemp = (int)CurrentStateGen;
                break;
            case Rules.Cooldown:
                audioTemp = (int)CurrentStateCD;
                if (increment)
                    CurrentStateCD = ((int)CurrentStateCD) + 1 >= System.Enum.GetValues(typeof(CooldownRules)).Length ? CurrentStateCD : CurrentStateCD + 1;
                else
                    CurrentStateCD = ((int)CurrentStateCD) - 1 < 0 ? CurrentStateCD : CurrentStateCD - 1;
                audioAfterTemp = (int)CurrentStateCD;
                break;
            case Rules.PowerUp:
                audioTemp = (int)CurrentStatePU;
                if (increment)
                    CurrentStatePU = ((int)CurrentStatePU) + 1 >= System.Enum.GetValues(typeof(PowerUpRules)).Length ? CurrentStatePU : CurrentStatePU + 1;
                else
                    CurrentStatePU = ((int)CurrentStatePU) - 1 < 0 ? CurrentStatePU : CurrentStatePU - 1;
                audioAfterTemp = (int)CurrentStatePU;
                break;
            default:
                break;
        }
        if (audioAfterTemp != audioTemp)
            mainManager.PlayAudioClip(AudioClipType.Swipe);
        else
            mainManager.PlayAudioClip(AudioClipType.SwipeError);
    }

    private void ChangeStateHorizontal(bool increment)
    {
        switch (currentState)
        {
            case Rules.General:
                ChangeStateHorizontalGen(increment);
                break;
            case Rules.Cooldown:
                ChangeStateHorizontalCD(increment);
                break;
            case Rules.PowerUp:
                ChangeStateHorizontalPU(increment);
                break;
            default:
                break;
        }
    }

    private void ChangeStateHorizontalGen(bool increment)
    {
        switch (currentStateGen)
        {
            case GeneralRules.Header:
                CurrentState = increment ? Rules.Cooldown : Rules.PowerUp;
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case GeneralRules.Rounds:
                ChangeRounds(increment);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case GeneralRules.Map:
                ChangeMap(increment);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case GeneralRules.Announcer:
                ChangeAnnouncer();
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            default:
                mainManager.PlayAudioClip(AudioClipType.SwipeError);
                break;
        }
    }

    private void ChangeStateHorizontalCD(bool increment)
    {
        switch (currentStateCD)
        {
            case CooldownRules.Header:
                CurrentState = increment ? Rules.PowerUp : Rules.General;
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case CooldownRules.Reload:
                ChangeReload(increment);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case CooldownRules.Dash:
                ChangeDash(increment);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case CooldownRules.Shield:
                ChangeShield(increment);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            default:
                mainManager.PlayAudioClip(AudioClipType.SwipeError);
                break;
        }
    }

    private void ChangeStateHorizontalPU(bool increment)
    {
        switch (currentStatePU)
        {
            case PowerUpRules.Header:
                CurrentState = increment ? Rules.General : Rules.Cooldown;
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case PowerUpRules.Selection:
                ChangePowerUP(increment);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case PowerUpRules.Spawnrate:
                ChangeSpawnrate(increment, false);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case PowerUpRules.Duration:
                ChangeDuration(increment, false);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            case PowerUpRules.Enabled:
                ChangePUEnabled(false);
                mainManager.PlayAudioClip(AudioClipType.Swipe);
                break;
            default:
                mainManager.PlayAudioClip(AudioClipType.SwipeError);
                break;
        }
    }

    private void ChangePage(Rules current, Rules next)
    {
        SetMaterials(next);
        genTransform.SetActive(next.Equals(Rules.General));
        cdTransform.SetActive(next.Equals(Rules.Cooldown));
        puTransform.SetActive(next.Equals(Rules.PowerUp));
    }

    private void ChangeRounds(bool increment)
    {
        if (increment)
        {
            settings.Rounds++;
            if (settings.Rounds >= 100)
                settings.Rounds = 1;
        }
        else
        {
            settings.Rounds--;
            if (settings.Rounds <= 0)
                settings.Rounds = 99;
        }

        int singleDigit = settings.Rounds % 10;
        int doubleDigit = Mathf.FloorToInt(settings.Rounds / 10);

        for (int i = 0; i < 10; i++)
        {
            if (i != singleDigit)
                singleDigits[i].gameObject.SetActive(false);
            else
                singleDigits[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < 10; i++)
        {
            if (i != doubleDigit)
                doubleDigits[i].gameObject.SetActive(false);
            else
                doubleDigits[i].gameObject.SetActive(true);
        }
    }

    private void ChangeMap(bool increment)
    {
        if (increment)
        {
            settings.MapID++;
            if (settings.MapID >= maps.Length)
                settings.MapID = 0;
        }
        else
        {
            settings.MapID--;
            if (settings.MapID < 0)
                settings.MapID = maps.Length - 1;
        }

        for (int i = 0; i < maps.Length; i++)
        {
            maps[i].gameObject.SetActive(i == settings.MapID);
        }
    }

    private void ChangeAnnouncer()
    {
        settings.AnnouncerVoices = !settings.AnnouncerVoices;

        announcerChoice[0].gameObject.SetActive(settings.AnnouncerVoices);
        announcerChoice[1].gameObject.SetActive(!settings.AnnouncerVoices);
    }

    private void ChangeReload(bool increment)
    {
        if (increment)
        {
            settings.ReloadTime += .1f;
            if (settings.ReloadTime >= /*reloadDigits.Length*/1f)
                settings.ReloadTime = 0;
        }
        else
        {
            settings.ReloadTime -= .1f;
            if (settings.ReloadTime < 0f)
                settings.ReloadTime = ((float)reloadDigits.Length - 1f) * .1f;
        }

        for (int i = 0; i < reloadDigits.Length; i++)
        {
            reloadDigits[i].gameObject.SetActive(i == (int)(settings.ReloadTime * 10f));
		}
    }

    private void ChangeDash(bool increment)
    {
        if (increment)
        {
            settings.DashCD++;
            if (settings.DashCD >= dashDigits.Length)
                settings.DashCD = 0;
        }
        else
        {
            settings.DashCD--;
            if (settings.DashCD < 0)
                settings.DashCD = dashDigits.Length - 1;
        }

        for (int i = 0; i < dashDigits.Length; i++)
        {
            dashDigits[i].gameObject.SetActive(i == settings.DashCD);
        }
    }

    private void ChangeShield(bool increment)
    {
        if (increment)
        {
            settings.ShieldCD++;
            if (settings.ShieldCD >= shieldDigits.Length)
                settings.ShieldCD = 0;
        }
        else
        {
            settings.ShieldCD--;
            if (settings.ShieldCD < 0)
                settings.ShieldCD = shieldDigits.Length - 1;
        }

        for (int i = 0; i < shieldDigits.Length; i++)
        {
            shieldDigits[i].gameObject.SetActive(i == settings.ShieldCD);
        }
    }

    private void ChangePowerUP(bool increment)
    {

        if (increment)
        {
            currentPU++;
            if (currentPU >= 3)
                currentPU = 0;
        }
        else
        {
            currentPU--;
            if (currentPU < 0)
                currentPU = 2;
        }

        red.gameObject.SetActive(currentPU == 0);
        green.gameObject.SetActive(currentPU == 1);
        blue.gameObject.SetActive(currentPU == 2);

        SetPowerUpMaterials();

        ChangePUEnabled(true);
        ChangeSpawnrate(true, true);
        ChangeDuration(true, true);
    }

    private void ChangePUEnabled(bool onSwitch)
    {
        if(!onSwitch)
            settings.EnabledPowerups[currentPU] = !settings.EnabledPowerups[currentPU];

        enabledChoice[0].gameObject.SetActive(settings.EnabledPowerups[currentPU]);
        enabledChoice[1].gameObject.SetActive(!settings.EnabledPowerups[currentPU]);
    }

    private void ChangeSpawnrate(bool increment, bool onSwitch)
    {
        if (!onSwitch)
        {
            if (increment)
            {
                settings.Spawnrates[currentPU]++;
                if (settings.Spawnrates[currentPU] >= spawnDigits.Length)
                    settings.Spawnrates[currentPU] = 0;
            }
            else
            {
                settings.Spawnrates[currentPU]--;
                if (settings.Spawnrates[currentPU] < 0)
                    settings.Spawnrates[currentPU] = spawnDigits.Length - 1;
            }
        }
        for (int i = 0; i < spawnDigits.Length; i++)
        {
            spawnDigits[i].gameObject.SetActive(i == settings.Spawnrates[currentPU]);
        }
    }

    private void ChangeDuration(bool increment, bool onSwitch)
    {
        if (!onSwitch)
        {
            if (increment)
            {
                settings.Durations[currentPU] += 10;
                if (settings.Durations[currentPU] / 10 >= durationDigits.Length)
                    settings.Durations[currentPU] = 0;
            }
            else
            {
                settings.Durations[currentPU] -= 10;
                if (settings.Durations[currentPU] < 0)
                    settings.Durations[currentPU] = (durationDigits.Length - 1) * 10;
            }
        }
        for (int i = 0; i < durationDigits.Length; i++)
        {
            durationDigits[i].gameObject.SetActive(i == (int)(settings.Durations[currentPU] / 10));
        }
    }

    #endregion

    private void ManageConfirmation()
    {
        switch (currentState)
        {
            case Rules.General:
                if (currentStateGen.Equals(GeneralRules.Start))
                    StartLocalGame();
                else if (currentStateGen.Equals(GeneralRules.Reset))
                    Reset();
                else if (currentStateGen.Equals(GeneralRules.Announcer))
                {
                    ChangeAnnouncer();
                    mainManager.PlayAudioClip(AudioClipType.Swipe);
                }
                break;
            case Rules.Cooldown:
                if (currentStateCD.Equals(CooldownRules.Start))
                    StartLocalGame();
                break;
            case Rules.PowerUp:
                if (currentStatePU.Equals(PowerUpRules.Start))
                    StartLocalGame();
                else if (currentStatePU.Equals(PowerUpRules.Enabled))
                {
                    ChangePUEnabled(false);
                    mainManager.PlayAudioClip(AudioClipType.Swipe);
                }
                break;
            default:
                mainManager.PlayAudioClip(AudioClipType.Error);
                break;
        }
        
    }

    private void Start()
    {
        settings = new MatchSettings(15, 0, true, new bool[3] { true, true, true }, new int[3] { 9, 9, 9 }, new int[3] { 3, 3, 3 }, 4, 4, 2);
        CurrentState = standartState;
        CurrentStateGen = standartStateGen;
        CurrentStateCD = standartStateCD;
        CurrentStatePU = standartStatePU;
        Setup();
    }

    private void Setup()
    {
        // Rounds
        int singleDigit = settings.Rounds % 10;
        int doubleDigit = Mathf.FloorToInt(settings.Rounds / 10);

        for (int i = 0; i < 10; i++)
        {
            if (i != singleDigit)
                singleDigits[i].gameObject.SetActive(false);
            else
                singleDigits[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < 10; i++)
        {
            if (i != doubleDigit)
                doubleDigits[i].gameObject.SetActive(false);
            else
                doubleDigits[i].gameObject.SetActive(true);
        }

        // Map
        for (int i = 0; i < maps.Length; i++)
        {
            maps[i].gameObject.SetActive(i == settings.MapID);
        }

        // Announcer
        announcerChoice[0].gameObject.SetActive(settings.AnnouncerVoices);
        announcerChoice[1].gameObject.SetActive(!settings.AnnouncerVoices);

        // Reload Time
        for (int i = 0; i < reloadDigits.Length; i++)
        {
            reloadDigits[i].gameObject.SetActive(i == (int)(settings.ReloadTime * 10f));
        }

        // Dash Time
        for (int i = 0; i < dashDigits.Length; i++)
        {
            dashDigits[i].gameObject.SetActive(i == settings.DashCD);
        }

        // Shield Time
        for (int i = 0; i < shieldDigits.Length; i++)
        {
            shieldDigits[i].gameObject.SetActive(i == settings.ShieldCD);
        }

        // PowerUP
        red.gameObject.SetActive(currentPU == 0);
        green.gameObject.SetActive(currentPU == 1);
        blue.gameObject.SetActive(currentPU == 2);
        SetPowerUpMaterials();

        // EnabledPowerUp
        ChangePUEnabled(true);

        // Spawnrate
        ChangeSpawnrate(true, true);

        // Duration
        ChangeDuration(true, true);
    }

    private void Reset()
    {
        settings = new MatchSettings(15, 0, true, new bool[3] { true, true, true }, new int[3] { 9, 9, 9 }, new int[3] { 5, 5, 5 }, 4, 4, 4);
        Setup();
        mainManager.PlayAudioClip(AudioClipType.Confirm);
    }

    private void StartLocalGame()
    {
        mainManager.PlayAudioClip(AudioClipType.GameStart);
        GameManager.Instance.SetSettings(settings);
        GameManager.Instance.LoadScene(settings.MapID >= 1 ? 4 : 3);
    }

    public override void OnConfirmation(InputAction.CallbackContext ctx)
    {
        ManageConfirmation();
    }

    public override void OnDecline(InputAction.CallbackContext ctx)
    {
        mainManager.PlayAudioClip(AudioClipType.Decline);
        localLobbyManager.Rules = false;
    }

    public override void OnDPadInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        if (inputValue.y > 0f)
            ChangeStateVertical(false);
        else if (inputValue.y < 0f)
            ChangeStateVertical(true);
        else if (inputValue.x > 0f)
            ChangeStateHorizontal(true);
        else if (inputValue.x < 0f)
            ChangeStateHorizontal(false);
    }

    public override void OnStartPressed(InputAction.CallbackContext ctx)
    {
        mainManager.PlayAudioClip(AudioClipType.Error);
    }
}
