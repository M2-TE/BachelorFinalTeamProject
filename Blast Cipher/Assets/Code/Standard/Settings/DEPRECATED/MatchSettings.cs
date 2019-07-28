using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MatchSettings
{
    public int Rounds, MapID;
    public bool AnnouncerVoices;
    /// <summary>
    /// [0] => Red Powerup | [1] => GreenPowerup | [2] => Blue Powerup
    /// </summary>
    public bool[] EnabledPowerups;
    /// <summary>
    /// [0] => Red Powerup | [1] => GreenPowerup | [2] => Blue Powerup
    /// </summary>
    public int[] Spawnrates, Dutrations;
    public int ShieldCD, DashCD, ReloadTime;

    public MatchSettings(int rounds, int mapID, bool announcerVoices, bool[] enabledPowerups, int[] spawnrates, int[] dutrations, int shieldCD, int dashCD, int reloadTime)
    {
        Rounds = rounds;
        MapID = mapID;
        AnnouncerVoices = announcerVoices;
        EnabledPowerups = enabledPowerups;
        Spawnrates = spawnrates;
        Dutrations = dutrations;
        ShieldCD = shieldCD;
        DashCD = dashCD;
        ReloadTime = reloadTime;
    }
}
