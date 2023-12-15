using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;

public class PlayerItem : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public TMP_Text playerName;
    public Image readyPanel;
    public Image readyPanelImage;
    public Image playerPortrait;
    public GameObject nextButton;
    public GameObject backButton;
    //public bool isVR;
    public bool doOnce = false;
    public bool doOnce1 = false;

    private Color readyColor;
    private Color notReadyColor;
    private static bool toggle;
    LobbyManager lobbyManager;
   
    public Player player;
    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#FDC955", out readyColor);
        ColorUtility.TryParseHtmlString("#727272", out notReadyColor);
        lobbyManager = FindObjectOfType<LobbyManager>();
        //DontDestroyOnLoad(gameObject);


        //isVR = ConnectToServer.isVR;
        //isVR = ConnectToServer.isVR;
        //isVR = ConnectToServer.isVR;
    }

    void Start() 
    {
        playerPortrait.sprite = Resources.Load<Sprite>("Sprites/HumanPlayerPortrait");
        //SetNotReadyStateIsReady();
            
        // else if (transform.parent.tag == "PenguinSlot")
        // {
        //     playerPortrait.sprite = Resources.Load<Sprite>("Sprites/PenguinPlayerPortrait");
        // }
    }

    void Update() 
    {
        if (gameObject.name != "Bot")
        {
            // if (player.IsMasterClient)
            // {
            //    // Debug.Log(PhotonNetwork.MasterClient);
            //     readyPanelImage.sprite = Resources.Load<Sprite>("Sprites/crown");
            //     readyPanel.color = new Color32(253, 201, 85, 76);
            //     readyPanelImage.color = readyColor;

            //     // ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.MasterClient.CustomProperties;
            //     // playerProperties.Remove("isReady");
            //     // playerProperties.Add("isReady", 1);
            //     // PhotonNetwork.SetPlayerCustomProperties(playerProperties);
            // }

            // else
            // {
                readyPanelImage.sprite = Resources.Load<Sprite>("Sprites/check");
                if (doOnce == false)
                {
                    doOnce = true;
                    //SetNotReadyStateIsReady();
                }
            //}
        }



    }

    public IEnumerator NewMethod()
    {
        yield return new WaitForSeconds(0.2f);
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties.Remove("isReady");
        playerProperties.Add("isReady", 1);
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public IEnumerator NewMethod1()
    {
        yield return new WaitForSeconds(0.2f);
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties.Remove("isReady");
        playerProperties.Add("isReady", 0);
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;

        player = _player;
        UpdatePlayerItem(player);
    }

    public void ToggleReadyState()
    {
        if (toggle)
        {
            toggle = false;
            playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            playerProperties["isReady"] = 0;
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        }
        else
        {
            toggle = true;
            playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            playerProperties["isReady"] = 1;
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        }
    }

    public void SetReadyStateIsReady()
    {
        toggle = true;
        playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties["isReady"] = 1;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void SetNotReadyStateIsReady()
    {
        toggle = false;
        playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties["isReady"] = 0;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void isMasterClient(bool isMaster)
    {
        if (isMaster)
        {
            GetComponent<Button>().enabled = true;
            //SetReadyStateIsReady();
        }
        else
        {
            GetComponent<Button>().enabled = false;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    public void UpdatePlayerItem(Player player)
    {
        // if (transform.parent.tag == "HumanSlot")
        // {
        //     playerPortrait.sprite = Resources.Load<Sprite>("Sprites/HumanPlayerPortrait");
        // }

        // else if (transform.parent.tag == "PenguinSlot")
        // {
        //     playerPortrait.sprite = Resources.Load<Sprite>("Sprites/PenguinPlayerPortrait");
        // }

        // if (player.IsMasterClient)
        // {
            //readyPanelImage.sprite = Resources.Load<Sprite>("Sprites/crown");
        //}

        // else
        // {
            readyPanelImage.sprite = Resources.Load<Sprite>("Sprites/check");
        //}

        if (player.CustomProperties.ContainsKey("isReady"))
        {
            if ((int)player.CustomProperties["isReady"] == 1)
            {
                lobbyManager.ToggleReadyButtonColor(player, true);
                readyPanel.color = readyColor;
                readyPanelImage.color = readyColor;
            }

            else
            {
                lobbyManager.ToggleReadyButtonColor(player, false);
                readyPanel.color = notReadyColor;
                readyPanelImage.color = notReadyColor;
            }
            readyPanel.color = new Color(readyPanel.color.r, readyPanel.color.g, readyPanel.color.b, 0.3f);
            //playerProperties["isReady"] = (int)player.CustomProperties["isReady"];
        }
        else
        {
            playerProperties["isReady"] = 0;
        }
    }

    public override void OnLeftRoom()
    {
        toggle = false;
        playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties["isReady"] = 0;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void OnClickItem()
    {
        if (!player.IsMasterClient)
        {
            lobbyManager.OnClickManagePlayer(player);
        }
    }

    void IOnEventCallback.OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 203) //If player got kicked - PunEvent.CloseConnection (not public)
        {
            lobbyManager.SetPopupPanel("You have been kicked out from the room.");
        }
    }
}