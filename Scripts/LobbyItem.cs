using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class LobbyItem : MonoBehaviour
{
    public TMP_Text roomName;
    public TMP_Text currentPlayers;
    LobbyManager manager;
    public RoomInfo RoomInfo { get; private set; }

    private void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        roomName.text = roomInfo.Name;
        currentPlayers.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
    }

    public void SetCurrentPlayers(RoomInfo roomInfo)
    {
        currentPlayers.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
    }

    public void OnClickItem()
    {
        manager.JoinRoom(roomName.text);
    }
}
