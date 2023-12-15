using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class LobbySettings : MonoBehaviour
{
    public bool isFullyPreset = false;
    public enum LobbyMode
    {
        Preset,
        UserDefined
    }
    [HideInInspector]
    public bool isVR;
    [Space]
    [Header("Value Modes")]

    //public LobbyMode roundMode = LobbyMode.UserDefined;
    //public LobbyMode teamCountMode = LobbyMode.UserDefined;
    //public LobbyMode playerPerTeamMode = LobbyMode.UserDefined;
    //public LobbyMode roomStateMode = LobbyMode.UserDefined;
    //public LobbyMode mapIDMode = LobbyMode.UserDefined;
    [Space]
    public bool roundIsPreset = false;
    public bool teamCountIsPreset = false;
    public bool playerPerTeamIsPreset = false;
    public bool roomStateIsPreset = false;
    public bool mapIDIsPreset = false;
    [Space]
    [Header("Preset Values - Ignore if mode set to 'User Defined'")]
    [Space]
    public int round = 0;
    public int teamCount = 0;
    public int playerPerTeam = 0;
    public int roomState = 0;
    public int mapID = 0;
    [Space]
    [Header("Value Ranges - Ignore if mode se to 'Preset'")]
    public int maxRound;
    public int minRound;
    [Space]
    public int maxTeam;
    public int minTeam;
    [Space]
    public int maxPPT;
    public int minPPT;

    private void Start()
    {
        //isVR = ConnectToServer.isVR;
    }
}




