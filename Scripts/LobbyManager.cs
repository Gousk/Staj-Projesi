using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public LobbySettings lobbySettings;

    public PhotonView view;
    [SerializeField] private byte maxPlayers = 6;

    [Space(10)]

    [Header("Search Menu")]
    public TMP_Text currentTimeText;
    public TMP_Text searchPanelStateText;

    [Header("Lobby Menu")]
    public Transform contentObject;
    public GameObject noLobbiesText;

    [Header("Room Menu")]
    public GameObject botPanel;
    public GameObject settingsPanel;
    public GameObject startButton;
    public GameObject loadingCircle;
    public GameObject loadingPulsing;
    public Transform t1player1ItemParent;
    public Transform t1player2ItemParent;
    public Transform t1player3ItemParent;
    public Transform t2player4ItemParent;
    public Transform t2player5ItemParent;
    public Transform t2player6ItemParent;
    public Transform playerList;
    public Image readyButtonImage;
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public TMP_Text roomPanelStateText;
    public TMP_Text roomPrivateStateText;
    public TMP_Text roundText;
    public TMP_Text mapText;
    public TMP_Text TPRText;
    public List<GameObject> teamSlots = new List<GameObject>();
    public List<GameObject> team1Slots = new List<GameObject>();
    public List<GameObject> team2Slots = new List<GameObject>();
    public GameObject teamSelectPanel;
    public GameObject selectButtons;
    public GameObject selectButton1;
    public GameObject selectButton2;
    public GameObject selectButton3;
    public GameObject teamRoot;
    public int playerInTeamCount;
    public int readyPlayerCount = 0;
    public GameObject playerSlot;
    public GameObject teamSlot;
    public GameObject selectButtonPrefab;

    [Header("Create Menu")]
    public int ppt = 1;
    public int team = 2;
    public TMP_Text roundTxt;
    public TMP_Text teamTxt;
    public TMP_Text PPTeamTxt;
    public TMP_Text roomStateTxt;
    public GameObject mapPanel;
    public GameObject roundPanel;
    public GameObject teamCountPanel;
    public GameObject PPTPanel;
    public GameObject roomStatePanel;
    public GameObject createButton;


    public Image mapImage;
    public List<Sprite> mapImages = new List<Sprite>();
    public List<string> mapNames = new List<string>();
    public List<Transform> playerItemSpawns = new List<Transform>();
    public List<GameObject> AddButtons = new List<GameObject>();
    public List<GameObject> RemoveButtons = new List<GameObject>();
    private int currentMap;
    private int round = 1;
    private float timePerRound = 1f;
    private bool isPrivate = false;
    private bool isAudienceAllowed = false;
    PlayerItem lastMasterItem1;
    PlayerItem lastMasterItem2;
    Player lastMaster;
    public AnimationCurve moveCurve;

    [Header("Host Game Menu")]
    public GameObject hostGamePanel;
    public TMP_InputField hostGameInputField;

    [Header("Manage Menu")]
    public GameObject managePanel;
    public TMP_Text managePanelPlayerNameText;

    [Header("Options Menu")]
    public TMP_InputField playerNameInput;
    public TMP_Text changeNameButtonText;

    [Header("Join Menu")]
    public TMP_InputField roomCodeInput;

    [Header("Popup Menu")]
    public GameObject popupPanel;
    public TMP_Text popupPanelMessageText;

    [Header("Game Start Panel")]
    public TMP_Text countdownText;
    public GameObject gameStartPanel;

    [Header("Prefabs")]
    public LobbyItem roomItemPrefab;
    public PlayerItem playerItemPrefab;
    public PlayerItem BotPlayerItemPrefab;

    private List<PlayerItem> playerItemsList = new List<PlayerItem>();
    public List<LobbyItem> roomItemsList = new List<LobbyItem>();
    public List<GameObject> settingButton = new List<GameObject>();
    public List<string> idList = new List<string>();

    private bool timerActive = false;
    float currentTime;

    private bool readyToggle = false;
    private bool doOnce = false;
    private bool doOnce3 = false;

    private bool createRoomOnFailedProgress = false;
    private Color color;

    public static Player playerToManage;
    public Player leftPlayer;

    public string randomID;
    private Transform spawnParent;

    public static string channelPrefix = "dev_";
    public static string goToRoom;
    public GameObject lobbyCanvas;
    public GameObject cam;
    public Transform targetCamPos;
    public AnimationCurve curve;

    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected) //Sahne yüklendiğinde photona bağlı değil ise bağlantının sağlandığı sahne yüklenir.
        {
            if(lobbySettings.isVR == true)
            {
                SceneManager.LoadScene("MainVR");
                return;
            }
            else
            {
                SceneManager.LoadScene("Main");
                return;
            }
        } 
    }

    public void OnClickCreateRoom()
    {
        StartCoroutine(CreateRoom());
    }

    private void Start()
    {
        mapText.text = mapNames[0];
        mapImage.sprite = mapImages[0];
        
        //Debug.Log(Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["isVR"]));
        PhotonNetwork.JoinLobby();
        PhotonNetwork.EnableCloseConnection = true;
        //PhotonNetwork.OfflineMode = false;

        managePanel.SetActive(false);
        hostGamePanel.SetActive(false);

        StartCoroutine(CreateOrJoin());

        PhotonNetwork.AutomaticallySyncScene = true;
        //Debug.Log(ConnectToServer.isVR);

        
    }

    public IEnumerator CreateOrJoin()
    {
        if (ConnectToServer.createOrJoin == true)
        {
            MenuManager.Instance.OpenMenu("join");
        }
        else
        {
            if (!lobbySettings.isFullyPreset)
            {
                if (lobbySettings.roundIsPreset == true)
                {
                    roundPanel.gameObject.SetActive(false);
                }   
                if (lobbySettings.teamCountIsPreset == true)
                {
                    teamCountPanel.SetActive(false);
                } 
                if (lobbySettings.playerPerTeamIsPreset == true)
                {
                    PPTPanel.gameObject.SetActive(false);
                } 
                if (lobbySettings.roomStateIsPreset == true)
                {
                    roomStatePanel.SetActive(false);
                } 
                if (lobbySettings.mapIDIsPreset == true)
                {
                    mapPanel.SetActive(false);
                }
                MenuManager.Instance.OpenMenu("create");
            }
            else
            {
                string roomName = idGenerator();
                goToRoom = roomName;
                yield return new WaitForSeconds(0.2f);
                PhotonNetwork.CreateRoom(roomName,
                    new RoomOptions() { MaxPlayers = Convert.ToByte(team * ppt), BroadcastPropsChangeToAll = true, EmptyRoomTtl = 0, IsVisible = true });

                yield return new WaitUntil(() => PhotonNetwork.CurrentRoom != null);
                customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                customProperties.Remove("isPrivate");
                customProperties.Add("isPrivate", lobbySettings.roomState);

                customProperties.Remove("PPTCount");
                customProperties.Add("PPTCount", lobbySettings.playerPerTeam);

                customProperties.Remove("TeamCount");
                customProperties.Add("TeamCount", lobbySettings.teamCount);

                customProperties.Remove("RoundCount");
                customProperties.Add("RoundCount", lobbySettings.round);

                customProperties.Remove("MapID");
                customProperties.Add("MapID", lobbySettings.mapID);
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

                for (int i = 0; i < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["TeamCount"]); i++)
                {
                    if (lobbySettings.isVR == true)
                    {
                        GameObject team = PhotonNetwork.Instantiate("TeamSlotVR", teamRoot.transform.position, teamRoot.transform.rotation) as GameObject;
                        team.gameObject.name = "TS";
                        team.transform.SetParent(teamRoot.transform);
                        teamSlots.Add(team);
                    }
                    else
                    {
                        GameObject team = PhotonNetwork.Instantiate("TeamSlot", teamRoot.transform.position, teamRoot.transform.rotation) as GameObject;
                        team.gameObject.name = "TS";
                        team.transform.SetParent(teamRoot.transform);
                        teamSlots.Add(team);
                    }

                    for (int x = 0; x < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]); x++)
                    {
                        if (lobbySettings.isVR == true)
                        {
                            GameObject playerItem = PhotonNetwork.Instantiate("PlayerSlotVR", teamRoot.transform.GetChild(i).transform.position, teamRoot.transform.GetChild(i).transform.rotation);
                            playerItem.gameObject.name = "PS";
                            playerItem.transform.SetParent(teamRoot.transform.GetChild(i).transform);
                        }
                        else
                        {
                            GameObject playerItem = PhotonNetwork.Instantiate("PlayerSlot", teamRoot.transform.GetChild(i).transform.position, teamRoot.transform.GetChild(i).transform.rotation);
                            playerItem.gameObject.name = "PS";
                            playerItem.transform.SetParent(teamRoot.transform.GetChild(i).transform);
                        }
                        //GameObject playerItem = Instantiate(playerSlot, teamRoot.transform.GetChild(i).transform);

                        //teamSlots[i].transform.GetChild(x).gameObject.SetActive(true);
                        // i + (itemCount - (i*2))
                        // 0 + (6-0) 1 + (6-2) 2 + (6-4) 3 + (6-6) 4 + (6-8) 5 + (6-10) 6 + (6-12)
                    }
                }

                for (int i = 0; i < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["TeamCount"]); i++)
                {
                    if (lobbySettings.isVR == true)
                    {
                        GameObject button = PhotonNetwork.Instantiate("TeamSelectButtonVR", selectButtons.transform.position, selectButtons.transform.rotation) as GameObject;
                        button.transform.SetParent(selectButtons.transform);
                        button.GetComponent<Button>().onClick.AddListener(FindObjectOfType<LobbyManager>().SelectTeam);
                        button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Team" + (i + 1);
                        Debug.Log("select");
                    }
                    else
                    {
                        GameObject button = PhotonNetwork.Instantiate("TeamSelectButton", selectButtons.transform.position, selectButtons.transform.rotation) as GameObject;
                        button.transform.SetParent(selectButtons.transform);
                        button.GetComponent<Button>().onClick.AddListener(FindObjectOfType<LobbyManager>().SelectTeam);
                        button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Team" + (i + 1);
                        Debug.Log("select");
                    }

                    //selectButtons.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }
    }

    private void Update()
    {
        if (timerActive == true) { SearchGameTimer(); }

        CheckRoomState();
        CheckPlayerIdentity();

        for (int i = 0; i < playerItemsList.Count; i++)
        {
            if (Convert.ToInt32(playerItemsList[i].player.CustomProperties["NoInTeam"]) + 1 == Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]))
            {
                selectButtons.transform.GetChild(Convert.ToInt32(playerItemsList[i].player.CustomProperties["Team"])-1).gameObject.GetComponent<Button>().interactable = false;
                //if (Convert.ToInt32(playerItemsList[i].player.CustomProperties["Team"]) == 1)
                //{
                //    selectButton1.GetComponent<Button>().interactable = false;
                //    break;
                //}
                //else if (Convert.ToInt32(playerItemsList[i].player.CustomProperties["Team"]) == 2)
                //{
                //    selectButton2.GetComponent<Button>().interactable = false;
                //    break;
                //}
            }         
        }

        if (PhotonNetwork.CurrentRoom != null) //Max oyuncunun ayarlanması.
        {
            int currentPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
            int maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;
            playerCountText.text = playerItemsList.Count + "/" + maxPlayer;
        }

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            // for (int i = 0; i < playerItemSpawns.Count; i++)
            // {
            //     if (playerItemSpawns[i].childCount > 0 && playerItemSpawns[i].GetChild(0).gameObject.name != "Bot" && !playerItemSpawns[i].GetChild(0).gameObject.GetComponent<PlayerItem>().player.IsMasterClient)
            //     {
            //         makeHostButtons.transform.GetChild(i).gameObject.SetActive(true);
            //     } 
            //     else
            //     {
            //         makeHostButtons.transform.GetChild(i).gameObject.SetActive(false);
            //     }
            // }

            // if (playerVRItemParent.childCount > 0 && playerVRItemParent.GetChild(0).gameObject.name != "Bot" && !playerVRItemParent.GetChild(0).gameObject.GetComponent<PlayerItem>().player.IsMasterClient)
            // {
            //     makeHostButtons.transform.GetChild(5).gameObject.SetActive(true);
            // } 
            // else
            // {
            //     makeHostButtons.transform.GetChild(5).gameObject.SetActive(false);
            // }
        }
        else
        {
            //Debug.Log(PhotonNetwork.MasterClient.NickName);
            //makeHostButtons.SetActive(false);
        }

        if (settingsPanel.activeSelf) //Oda gizlilik ayarı.
        {
            if (isPrivate)
            {
                roomPrivateStateText.text = "Private";
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }
            else
            {
                roomPrivateStateText.text = "Public";
                if (PhotonNetwork.CurrentRoom != null)
                {
                    PhotonNetwork.CurrentRoom.IsVisible = true;
                }
            }

            if (isAudienceAllowed)
            {
                TPRText.text = "Allowed";
            }
            else
            {
                TPRText.text = "Not Allowed";
            }
        }

        //for (int i = 0; i < playerItemSpawns.Count; i++)
        //{
        //    if (playerItemSpawns[i].childCount > 0)
        //    {
        //        //AddButtons[i].SetActive(false);
        //        if (playerItemSpawns[i].GetChild(0).name == "Bot")
        //        {
        //            //RemoveButtons[i].SetActive(true);
        //        }
        //    } 
        //    else
        //    {
        //        //AddButtons[i].SetActive(true);
        //        //RemoveButtons[i].SetActive(false);
        //    }   
        //}

        mapText.text = mapNames[currentMap];
        mapImage.sprite = mapImages[currentMap];




    }

    public string idGenerator() //Oda id generator.
    {
        var chars = "0123456789";
        var stringChars = new char[6];
        var random = new System.Random();
        //StartCoroutine(UpdatePlayerList());

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        randomID = new String(stringChars);
        
        if (roomItemsList.Count >= 1)
        {
            for (int i = 0; i < roomItemsList.Count; i++)
            {
                if(roomItemsList[i].roomName.text == randomID)
                { 
                    idGenerator();
                    return null;
                }
            } 

            // if(roomItemsList.Where(x=>x.roomName.text == randomID).Count > 0) {
            //         idGenerator();
            //         return null;
            // } 
        }
        
        idList.Add(randomID);    
        return randomID;
    }

    public IEnumerator CreateRoom()
    {
        createButton.GetComponent<Button>().interactable = false;
        string roomName = idGenerator();
        goToRoom = roomName;
        yield return new WaitForSeconds(0.2f);
        PhotonNetwork.CreateRoom(roomName,
            new RoomOptions() { MaxPlayers = Convert.ToByte(team * ppt), BroadcastPropsChangeToAll = true, EmptyRoomTtl = 0, IsVisible = true });

        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom != null);
        customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        //customProperties.Remove("isPrivate");
        //customProperties.Add("isPrivate", isPrivate);

        //customProperties.Remove("PPTCount");
        //customProperties.Add("PPTCount", PPTeamTxt.text);

        //customProperties.Remove("TeamCount");
        //customProperties.Add("TeamCount", teamTxt.text);

        //customProperties.Remove("RoundCount");
        //customProperties.Add("RoundCount", roundTxt.text);

        //customProperties.Remove("MapID");
        //customProperties.Add("MapID", currentMap);

        if (lobbySettings.roundIsPreset == true)
        {
            customProperties.Remove("RoundCount");
            customProperties.Add("RoundCount", lobbySettings.round);
        }
        else
        {
            customProperties.Remove("RoundCount");
            customProperties.Add("RoundCount", roundTxt.text);
        }

        if (lobbySettings.teamCountIsPreset == true)
        {
            customProperties.Remove("TeamCount");
            customProperties.Add("TeamCount", lobbySettings.teamCount);
        }
        else
        {
            customProperties.Remove("TeamCount");
            customProperties.Add("TeamCount", teamTxt.text);
        }

        if (lobbySettings.playerPerTeamIsPreset == true)
        {
            customProperties.Remove("PPTCount");
            customProperties.Add("PPTCount", lobbySettings.playerPerTeam);
        }
        else
        {
            customProperties.Remove("PPTCount");
            customProperties.Add("PPTCount", PPTeamTxt.text);
        }

        if (lobbySettings.roomStateIsPreset == true)
        {
            customProperties.Remove("isPrivate");
            customProperties.Add("isPrivate", lobbySettings.roomState);
        }
        else
        {
            customProperties.Remove("isPrivate");
            customProperties.Add("isPrivate", isPrivate);
        }

        if (lobbySettings.mapIDIsPreset == true)
        {
            customProperties.Remove("MapID");
            customProperties.Add("MapID", lobbySettings.mapID);
        }
        else
        {
            customProperties.Remove("MapID");
            customProperties.Add("MapID", currentMap);
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

        for (int i = 0; i < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["TeamCount"]); i++)
        {
            if (lobbySettings.isVR == true)
            {
                GameObject team = PhotonNetwork.Instantiate("TeamSlotVR", teamRoot.transform.position, teamRoot.transform.rotation) as GameObject;
                team.gameObject.name = "TS";
                team.transform.SetParent(teamRoot.transform);
                teamSlots.Add(team);
            }
            else
            {
                GameObject team = PhotonNetwork.Instantiate("TeamSlot", teamRoot.transform.position, teamRoot.transform.rotation) as GameObject;
                team.gameObject.name = "TS";
                team.transform.SetParent(teamRoot.transform);
                teamSlots.Add(team);
            }

            for (int x = 0; x < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]); x++)
            {
                if (lobbySettings.isVR == true)
                {
                    GameObject playerItem = PhotonNetwork.Instantiate("PlayerSlotVR", teamRoot.transform.GetChild(i).transform.position, teamRoot.transform.GetChild(i).transform.rotation);
                    playerItem.gameObject.name = "PS";
                    playerItem.transform.SetParent(teamRoot.transform.GetChild(i).transform);
                }
                else
                {
                    GameObject playerItem = PhotonNetwork.Instantiate("PlayerSlot", teamRoot.transform.GetChild(i).transform.position, teamRoot.transform.GetChild(i).transform.rotation);
                    playerItem.gameObject.name = "PS";
                    playerItem.transform.SetParent(teamRoot.transform.GetChild(i).transform);
                }
                //GameObject playerItem = Instantiate(playerSlot, teamRoot.transform.GetChild(i).transform);

                //teamSlots[i].transform.GetChild(x).gameObject.SetActive(true);
                // i + (itemCount - (i*2))
                // 0 + (6-0) 1 + (6-2) 2 + (6-4) 3 + (6-6) 4 + (6-8) 5 + (6-10) 6 + (6-12)
            }
        }

        for (int i = 0; i < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["TeamCount"]); i++)
        {
            if (lobbySettings.isVR == true)
            {
                GameObject button = PhotonNetwork.Instantiate("TeamSelectButtonVR", selectButtons.transform.position, selectButtons.transform.rotation) as GameObject;
                button.transform.SetParent(selectButtons.transform);
                button.GetComponent<Button>().onClick.AddListener(FindObjectOfType<LobbyManager>().SelectTeam);
                button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Team" + (i + 1);
                Debug.Log("select");
            }
            else
            {
                GameObject button = PhotonNetwork.Instantiate("TeamSelectButton", selectButtons.transform.position, selectButtons.transform.rotation) as GameObject;
                button.transform.SetParent(selectButtons.transform);
                button.GetComponent<Button>().onClick.AddListener(FindObjectOfType<LobbyManager>().SelectTeam);
                button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Team" + (i + 1);
                Debug.Log("select");
            }

            //selectButtons.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void SearchGameTimer()
    {
        currentTime = currentTime + Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currentTimeText.text = time.ToString(@"mm\:ss");
    }

    private bool IsEveryoneReady()
    {
        if(PhotonNetwork.PlayerList.Length == (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]) * 2))
        {
            foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
            {
                if (player.Value.CustomProperties.ContainsKey("isReady"))
                {
                    //    Debug.Log((int)player.Value.CustomProperties["isReady"]);
                    if ((int)player.Value.CustomProperties["isReady"] == 0)
                    {
                        return false;
                    }
                    else
                    {
                        readyPlayerCount++;
                    }
                }
                else
                    return false;
            }
            return true;
        }
        return false;
    }

    private void CheckPlayerIdentity()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //botPanel.SetActive(true);
            startButton.SetActive(true);
            readyButtonImage.gameObject.SetActive(true);
            for (int i = 0; i < settingButton.Count; i++)
            {
                settingButton[i].SetActive(true);    
            }
        }

        else
        {
            //botPanel.SetActive(false);
            startButton.SetActive(false);
            readyButtonImage.gameObject.SetActive(true);
            for (int i = 0; i < settingButton.Count; i++)
            {
                settingButton[i].SetActive(false);    
            }
        }
    }

    public void OnClickStartGameButton()
    {
        if (IsEveryoneReady())
        {
            if (doOnce3 == false)
                view.RPC("StartGamePanel", RpcTarget.All);

            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    private void CheckRoomState()
    {
        if (PhotonNetwork.CurrentRoom != null && playerItemsList.Count > 1)
        {
            if (IsEveryoneReady())
            {
                string stateText = "Waiting for host to start game";
                SetRoomState(stateText, true); 
                roomPanelStateText.text = "Ready to start";
                //if(doOnce3 == false)
                //view.RPC("StartGamePanel", RpcTarget.All);

                //view.RPC("Sendmessage", RpcTarget.Others, playerItemsList[PhotonNetwork.LocalPlayer.ActorNumber - 1].robotNameID.text.ToString(), playerItemsList[PhotonNetwork.LocalPlayer.ActorNumber - 1].robotNameText.text.ToString());
            }

            else
            {
                string stateText = "Waiting for everyone to ready up";
                SetRoomState(stateText, false);
            }
        }
        else
        {
            string stateText = "Waiting for other players";
            SetRoomState(stateText, false);
        }
    }

    private void SetRoomState(string text, bool isReadyToStart)
    {
        if (isReadyToStart)
        {
            loadingCircle.SetActive(false);
            loadingPulsing.SetActive(true);
            roomPanelStateText.text = text;
            //StartButtonState(0.25f, true);
            startButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            loadingCircle.SetActive(true);
            loadingPulsing.SetActive(false);
            roomPanelStateText.text = text;
            //StartButtonState(0.25f, false);
            startButton.GetComponent<Button>().interactable = false;
        }
    }

    private void StartButtonState(float alpha, bool enabled)
    {
        color = startButton.GetComponent<Image>().color;
        startButton.GetComponent<Image>().color = new Color(color.r, color.g, color.b, alpha);

        color = startButton.GetComponentInChildren<TextMeshProUGUI>().color;
        startButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, alpha);
        startButton.GetComponent<Button>().enabled = enabled;
    }

    // public void OnClickAddBotVR()
    // {
    //     //AddBotVR();
    //     //view.RPC("AddBotVR", RpcTarget.All); 
    // }
    public void OnClickAddBot1()
    {
        //AddBot(0);
        view.RPC("AddBot", RpcTarget.All, 0); 
    }
    public void OnClickAddBot2()
    {
        //AddBot(1);
        view.RPC("AddBot", RpcTarget.All, 1);        
    }

    [PunRPC]
    private void AddBot(int i)
    {
        if (playerItemSpawns[i].transform.childCount == 0)
        {
            PlayerItem newPlayerItem = Instantiate(BotPlayerItemPrefab, playerItemSpawns[i]);
            newPlayerItem.gameObject.name = "Bot";
            newPlayerItem.playerName.text = "Bot";
            playerItemsList.Add(newPlayerItem);
        }
    }

    // [PunRPC]
    // private void AddBotVR()
    // {
    //     if (playerVRItemParent.transform.childCount == 0)
    //     {
    //         PlayerItem newPlayerItem = Instantiate(BotPlayerItemPrefab, playerVRItemParent);
    //         newPlayerItem.gameObject.name = "Bot";
    //         newPlayerItem.playerName.text = "Bot";
    //         playerItemsList.Add(newPlayerItem);
    //     }
    // }

    // public void OnClickRemoveBotVR()
    // {
    //     //AddBot(0);
    //     view.RPC("RemoveBot", RpcTarget.All, 5); 
    // }
    public void OnClickRemoveBot1()
    {
        //AddBot(0);
        view.RPC("RemoveBot", RpcTarget.All, 0); 
    }
    public void OnClickRemoveBot2()
    {
        //AddBot(1);
        view.RPC("RemoveBot", RpcTarget.All, 1);        
    }

    [PunRPC]
    private void RemoveBot(int i)
    {
        Destroy(playerItemSpawns[i].GetChild(0).gameObject, 0f);
        RemoveButtons[i].SetActive(false);
        playerItemsList.Remove(playerItemSpawns[i].GetChild(0).GetComponent<PlayerItem>());
    }

    public void OnClickIncreaseRound()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            view.RPC("ClickRound", RpcTarget.AllBuffered, true, 1, 7, 2);
            customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Remove("RoundCount");
            customProperties.Add("RoundCount", roundText.text);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        else
        {
            ClickRound(true, lobbySettings.minRound, lobbySettings.maxRound, 2);
        }
    }

    public void OnClickDecreaseRound()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            view.RPC("ClickRound", RpcTarget.AllBuffered, false, 1, 7, 2);
            customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Remove("RoundCount");
            customProperties.Add("RoundCount", roundText.text);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        else
        {
            ClickRound(false, lobbySettings.minRound, lobbySettings.maxRound, 2); 
        }
    }

    [PunRPC]
    private void ClickRound(bool isInc, int minValue, int maxValue, int value)
    {
        if (isInc)
        {
            if (round < maxValue)
                round += value;
        }
        else
        {
            if (round > minValue)
                round -= value;
        }
        roundText.text = round.ToString();
        roundTxt.text = round.ToString();
    }

    public void OnClickIncreaseTeam()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            view.RPC("ClickTeam", RpcTarget.AllBuffered, true, 2, 4, 1);
            customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Remove("TeamCount");
            customProperties.Add("TeamCount", teamTxt.text);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        else
        {
            ClickTeam(true, lobbySettings.minTeam, lobbySettings.maxTeam, 1);
        }
    }

    public void OnClickDecreaseTeam()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            view.RPC("ClickTeam", RpcTarget.AllBuffered, false, 2, 4, 1);
            customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Remove("TeamCount");
            customProperties.Add("TeamCount", teamTxt.text);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        else
        {
            ClickTeam(false, lobbySettings.minTeam, lobbySettings.maxTeam, 1);
        }
    }

    [PunRPC]
    private void ClickTeam(bool isInc, int minValue, int maxValue, int value)
    {
        if (isInc)
        {
            if (team < maxValue)
                team += value;
        }
        else
        {
            if (team > minValue)
                team -= value;
        }
        teamTxt.text = team.ToString();
    }

    public void OnClickIncreasePPT()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            view.RPC("ClickPPT", RpcTarget.AllBuffered, true, 1, 3, 1);
            customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Remove("PPTCount");
            customProperties.Add("PPTCount", PPTeamTxt.text);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        else
        {
            ClickPPT(true, lobbySettings.minPPT, lobbySettings.maxPPT, 1);
        }
    }

    public void OnClickDecreasePPT()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            view.RPC("ClickPPT", RpcTarget.AllBuffered, false, 1, 3, 1);
            customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Remove("PPTCount");
            customProperties.Add("PPTCount", PPTeamTxt.text);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        else
        {
            ClickPPT(false, lobbySettings.minPPT, lobbySettings.maxPPT, 1);
        }
    }

    [PunRPC]
    private void ClickPPT(bool isInc, int minValue, int maxValue, int value)
    {
        if (isInc)
        {
            if (ppt < maxValue)
                ppt += value;
        }
        else
        {
            if (ppt > minValue)
                ppt -= value;
        }
        PPTeamTxt.text = ppt.ToString();
    }

    public void OnClickIncreaseMapInt()
    {
        view.RPC("ClickIncreaseMapInt", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void ClickIncreaseMapInt()
    {
        if (currentMap < mapNames.Count-1)
            currentMap += 1;
    }

    public void OnClickDecreaseMapInt()
    {
        view.RPC("ClickDecreaseMapInt", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void ClickDecreaseMapInt()
    {
        if (currentMap > 0)
            currentMap -= 1;
    }

    public void OnClickRoomState()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            view.RPC("StateChange", RpcTarget.AllBuffered);
            customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Remove("isPrivate");
            customProperties.Add("isPrivate", isPrivate);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        else
        {
            StateChange();
            if (isPrivate)
                roomStateTxt.text = "Private";
            else
                roomStateTxt.text = "Public";
        }
    }

    [PunRPC]
    private void StateChange()
    {
        isPrivate = !isPrivate;
    }

    public void OnClickAudienceStateChange()
    {
        view.RPC("AudienceStateChange", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void AudienceStateChange()
    {
        isAudienceAllowed = !isAudienceAllowed;
    }

    public void OnClickFindGame()
    {
        MenuManager.Instance.OpenMenu("search");
        currentTime = 0;
        timerActive = true;
        searchPanelStateText.text = "LOOKING FOR LOBBY...";
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnClickJoinButton()
    {
        PhotonNetwork.JoinRoom(roomCodeInput.text);
    }

    public void OnClickFindGameClose()
    {
        MenuManager.Instance.OpenMenu("join");
        timerActive = false;
        createRoomOnFailedProgress = false;
    }

    public void OnClickJoinMenu()
    {
        MenuManager.Instance.OpenMenu("join");
    }

    IEnumerator CameraMoverLobby(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = cam.transform.position;
        CanvasGroup canvasGroup = lobbyCanvas.GetComponent<CanvasGroup>();
        float startAlpha = 1;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, curve.Evaluate(time / duration));
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, curve.Evaluate(time / duration));
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        cam.transform.position = targetPosition;

        yield return new WaitForSeconds(0.1f);

        PhotonNetwork.LeaveRoom(true);

    }

    IEnumerator CameraMoverJoin(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = cam.transform.position;
        CanvasGroup canvasGroup = lobbyCanvas.GetComponent<CanvasGroup>();
        float startAlpha = 1;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, curve.Evaluate(time / duration));
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, curve.Evaluate(time / duration));
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        cam.transform.position = targetPosition;

        yield return new WaitForSeconds(0.1f);

        if(lobbySettings.isVR == true)
        {
            SceneManager.LoadScene("MainVR");
        }
        else
        {
            SceneManager.LoadScene("Main");
        }
    }

    public void OnClickBackButton()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("Team");
        customProperties.Add("Team", null);
        customProperties.Remove("PPTCount");
        customProperties.Add("PPTCount", null);
        customProperties.Remove("NoInTeam");
        customProperties.Add("NoInTeam", null);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        StartCoroutine(CameraMoverJoin(targetCamPos.position, 0.4f));
    }

    public void OnClickOptions()
    {
        changeNameButtonText.text = "CHANGE NAME";
        MenuManager.Instance.OpenMenu("options");
        playerNameInput.text = PhotonNetwork.NickName;
    }

    public void OnClickOptionsClose()
    {
        MenuManager.Instance.OpenMenu("main");
    }

    public void OnClickChangeName()
    {
        if (playerNameInput.text.Length > 0 && playerNameInput.text != PhotonNetwork.NickName)
        {
            PhotonNetwork.NickName = playerNameInput.text;
            changeNameButtonText.text = "SUCCESS";
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        StartCoroutine(OnFailed(returnCode, message));
    }

    IEnumerator OnFailed(short returnCode, string message)
    {
        if (returnCode == 32760) //No match found
        {
            searchPanelStateText.text = message + "...";
            createRoomOnFailedProgress = true;
            yield return new WaitForSeconds(0.5f); //Wait to read message

            searchPanelStateText.text = "NO ROOM AVAILABLE...";
            MenuManager.Instance.OpenMenu("join");
            SetPopupPanel("NO AVAILABLE ROOM FOUND");

            // searchPanelStateText.text = "CREATING LOBBY...";
            // string roomName = idGenerator();

            // yield return new WaitForSeconds(0.5f);

            // if (createRoomOnFailedProgress)
            //     PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = maxPlayers, EmptyRoomTtl = 0, IsVisible = true });
        }
    }

    public void OnClickLobbyBrowser()
    {
        MenuManager.Instance.OpenMenu("lobby");
    }

    public void OnClickLobbyBrowserClose()
    {
        MenuManager.Instance.OpenMenu("main");
    }

    public void OnClickHostGame()
    {
        hostGameInputField.text = null;
        hostGamePanel.SetActive(true);
    }

    public void OnClickHostGameClose()
    {
        hostGamePanel.SetActive(false);
    }

    public void OnClickCreate()
    {
        hostGamePanel.SetActive(false);
        string roomID = idGenerator();
        if (hostGameInputField.text.Length >= 1)
            PhotonNetwork.CreateRoom(roomID, 
                new RoomOptions() { MaxPlayers = maxPlayers, BroadcastPropsChangeToAll = true, EmptyRoomTtl = 0, IsVisible = true });
    }

    public override void OnJoinedRoom()
    {
        timerActive = false;
        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = "Lobby: " + PhotonNetwork.CurrentRoom.Name;
        for (int i = 0; i < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["TeamCount"]); i++)
        {
            if (lobbySettings.isVR == true)
            {
                GameObject team = PhotonNetwork.Instantiate("TeamSlotVR", teamRoot.transform.position, teamRoot.transform.rotation) as GameObject;
                team.gameObject.name = "TS";
                team.transform.SetParent(teamRoot.transform);
                teamSlots.Add(team);
            }
            else
            {
                GameObject team = PhotonNetwork.Instantiate("TeamSlot", teamRoot.transform.position, teamRoot.transform.rotation) as GameObject;
                team.gameObject.name = "TS";
                team.transform.SetParent(teamRoot.transform);
                teamSlots.Add(team);
            }

            for (int x = 0; x < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]); x++)
            {
                if (lobbySettings.isVR == true)
                {
                    GameObject playerItem = PhotonNetwork.Instantiate("PlayerSlotVR", teamRoot.transform.GetChild(i).transform.position, teamRoot.transform.GetChild(i).transform.rotation);
                    playerItem.gameObject.name = "PS";
                    playerItem.transform.SetParent(teamRoot.transform.GetChild(i).transform);
                }
                else
                {
                    GameObject playerItem = PhotonNetwork.Instantiate("PlayerSlot", teamRoot.transform.GetChild(i).transform.position, teamRoot.transform.GetChild(i).transform.rotation);
                    playerItem.gameObject.name = "PS";
                    playerItem.transform.SetParent(teamRoot.transform.GetChild(i).transform);
                }
                //GameObject playerItem = Instantiate(playerSlot, teamRoot.transform.GetChild(i).transform);
                
                //teamSlots[i].transform.GetChild(x).gameObject.SetActive(true);
                // i + (itemCount - (i*2))
                // 0 + (6-0) 1 + (6-2) 2 + (6-4) 3 + (6-6) 4 + (6-8) 5 + (6-10) 6 + (6-12)
            }
        }

        for (int i = 0; i < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["TeamCount"]); i++)
        {
            if (lobbySettings.isVR == true)
            {
                GameObject button = PhotonNetwork.Instantiate("TeamSelectButtonVR", selectButtons.transform.position, selectButtons.transform.rotation) as GameObject;
                button.transform.SetParent(selectButtons.transform);
                button.GetComponent<Button>().onClick.AddListener(FindObjectOfType<LobbyManager>().SelectTeam);
                button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Team" + (i + 1);
                Debug.Log("select");
            }
            else
            {
                GameObject button = PhotonNetwork.Instantiate("TeamSelectButton", selectButtons.transform.position, selectButtons.transform.rotation) as GameObject;
                button.transform.SetParent(selectButtons.transform);
                button.GetComponent<Button>().onClick.AddListener(FindObjectOfType<LobbyManager>().SelectTeam);
                button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Team" + (i + 1);
                Debug.Log("select");
            }
            
            //selectButtons.transform.GetChild(i).gameObject.SetActive(true);
        }
        StartCoroutine(UpdatePlayerList(0));
        ClearRoomList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                int index = roomItemsList.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(roomItemsList[index].gameObject);
                    roomItemsList.RemoveAt(index);
                }
            }
            else
            {
                int index = roomItemsList.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index == -1)
                {
                    LobbyItem newRoom = Instantiate(roomItemPrefab, contentObject);
                    newRoom.SetRoomInfo(info);
                    roomItemsList.Add(newRoom);
                }
                else
                {
                    roomItemsList[index].SetCurrentPlayers(info);
                }
            }
        }

        if (roomItemsList.Count == 0)
            noLobbiesText.SetActive(true);
        else
            noLobbiesText.SetActive(false);
    }

    private void ClearRoomList()
    {
        foreach (LobbyItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("Team");
        customProperties.Add("Team", null);
        customProperties.Remove("PPTCount");
        customProperties.Add("PPTCount", null);
        customProperties.Remove("NoInTeam");
        customProperties.Add("NoInTeam", null);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        StartCoroutine(CameraMoverLobby(targetCamPos.position, 0.4f));
    }

    public override void OnLeftRoom()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("Team");
        customProperties.Add("Team", null);
        customProperties.Remove("PPTCount");
        customProperties.Add("PPTCount", null);
        customProperties.Remove("NoInTeam");
        customProperties.Add("NoInTeam", null);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        //MenuManager.Instance.OpenMenu("join");
        PhotonNetwork.Disconnect();
        if (lobbySettings.isVR == true)
        {
            SceneManager.LoadScene("MainVR");
        }
        else
        {
            SceneManager.LoadScene("Main");
        }

        readyToggle = false;
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
        
    }

    public void SelectTeam()
    {
        Debug.Log("ney1"+ EventSystem.current.currentSelectedGameObject.name);
        for (int i = 0; i < selectButtons.transform.childCount; i++)
        {

            if (selectButtons.transform.GetChild(i).gameObject == EventSystem.current.currentSelectedGameObject)
            {
                Debug.Log("ney2");
                customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
                customProperties.Remove("Team");
                customProperties.Add("Team", i+1);
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
                view.RPC("UpdateRPC", RpcTarget.All);
                teamSelectPanel.SetActive(false);
            }
        }
    }

    public void SelectTeam1()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("Team");
        customProperties.Add("Team", 1);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        view.RPC("UpdateRPC", RpcTarget.All);
        teamSelectPanel.SetActive(false);
    }

    public void SelectTeam2()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("Team");
        customProperties.Add("Team", 2);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        view.RPC("UpdateRPC", RpcTarget.All);
        teamSelectPanel.SetActive(false);
    }

    public void SelectTeam3()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("Team");
        customProperties.Add("Team", 3);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        view.RPC("UpdateRPC", RpcTarget.All);
        teamSelectPanel.SetActive(false);
    }

    public void SelectTeam4()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("Team");
        customProperties.Add("Team", 4);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        view.RPC("UpdateRPC", RpcTarget.All);
        teamSelectPanel.SetActive(false);
    }

    [PunRPC] 
    public void RPCAsyncLoadScene(string sceneName)
		{
        StartCoroutine(LoadYourAsyncScene(sceneName));
        }
    public static IEnumerator LoadYourAsyncScene(string sceneName)
        {


     
        //PhotonNetwork.LoadLevel(sceneName);
        //// Move the GameObject (you attach this in the Inspector) to the newly loaded Scene   
        //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return null;
        
       // PhotonNetwork.JoinOrCreateRoom(channelPrefix + goToRoom, new RoomOptions(), TypedLobby.Default);

        // Unload the previous Scene
        }
    private IEnumerator UpdatePlayerList(float timer)
    {
        
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.CustomProperties != null);
        int PPTCount = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]);
        int TeamCount = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["TeamCount"]);

        //for (int i = 0; i < teamSlots.Count; i++)
        //{
        //    //PhotonNetwork.Destroy(teamSlots[i]);
        //    Destroy(teamSlots[i]);
        //}
        //teamSlots.Clear();

        

        //for (int i = 0; i < PPTCount; i++)
        //{
        //    team1Slots[i].SetActive(true);
        //    team2Slots[i].SetActive(true);
        //}

        foreach (PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(timer);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties["Team"] != null)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {

                    if (player == PhotonNetwork.PlayerList[i])
                    {
                        //  Debug.Log("benim: "+ player.ActorNumber);
                        // if (Convert.ToInt32(player.CustomProperties["isVR"]) == 1)
                        // {
                        //     if (playerVRItemParent.childCount == 0)
                        //     {
                        //         spawnParent = playerVRItemParent;
                        //         break;
                        //     }
                        //     else if (playerVRItemParent.GetChild(0).gameObject.tag == "Bot")
                        //     {
                        //         Destroy(playerVRItemParent.transform.GetChild(0).gameObject, 0f);
                        //         spawnParent = playerVRItemParent;
                        //         break;
                        //     }
                        // }

                        yield return new WaitUntil(() => player.CustomProperties != null);
                        yield return new WaitUntil(() => player.CustomProperties["Team"] != null);

                        for (int x = 0; x < Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]); x++)
                        {
                            if (teamRoot.transform.GetChild(Convert.ToInt32(player.CustomProperties["Team"])-1).transform.GetChild(x).GetChild(0).childCount == 0)
                            {
                                spawnParent = teamRoot.transform.GetChild(Convert.ToInt32(player.CustomProperties["Team"]) - 1).transform.GetChild(x).GetChild(0);
                                customProperties = player.CustomProperties;
                                customProperties.Remove("NoInTeam");
                                customProperties.Add("NoInTeam", x);
                                player.SetCustomProperties(customProperties);
                                break;
                            }

                        }

                        ////Debug.Log(Convert.ToInt32(player.CustomProperties["Team"]));
                        //if (Convert.ToInt32(player.CustomProperties["Team"]) == 1)
                        //{
                        //    for (int x = 0; x < team1Slots.Count; x++)
                        //    {
                        //        if (team1Slots[x].transform.GetChild(0).childCount == 0)
                        //        {
                        //            spawnParent = team1Slots[x].transform.GetChild(0);
                        //            customProperties = player.CustomProperties;
                        //            customProperties.Remove("NoInTeam");
                        //            customProperties.Add("NoInTeam", x);
                        //            player.SetCustomProperties(customProperties);
                        //            break;
                        //        }

                        //    }
                        //}
                        //else if (Convert.ToInt32(player.CustomProperties["Team"]) == 2)
                        //{
                        //    for (int x = 0; x < team2Slots.Count; x++)
                        //    {
                        //        if (team2Slots[x].transform.GetChild(0).childCount == 0)
                        //        {
                        //            spawnParent = team2Slots[x].transform.GetChild(0);
                        //            customProperties = player.CustomProperties;
                        //            customProperties.Remove("NoInTeam");
                        //            customProperties.Add("NoInTeam", x);
                        //            player.SetCustomProperties(customProperties);
                        //            break;
                        //        }

                        //    }
                        //}

                        foreach (Player playerr in PhotonNetwork.PlayerList.Where(x => Convert.ToInt32(x.CustomProperties["Team"]) == 1))
                        {

                        }

                        //for (int x = 0; i < playerItemSpawns.Count + 1; x++)
                        //{
                        //    if (playerItemSpawns[x].childCount == 0)
                        //    {
                        //        spawnParent = playerItemSpawns[x];
                        //        break;
                        //    }
                        //}
                    }
                }

                //if (spawnParent.transform.childCount > 0)
                //{
                //    if (spawnParent.GetChild(0).gameObject.tag == "Bot")
                //    Destroy(spawnParent.GetChild(0).gameObject, 0f);
                //}

                PlayerItem newPlayerItem = Instantiate(playerItemPrefab, spawnParent);
                Debug.Log(newPlayerItem);
                newPlayerItem.SetPlayerInfo(player);

                //if (PhotonNetwork.IsMasterClient)
                //{
                //    newPlayerItem.isMasterClient(true);
                //    newPlayerItem.SetReadyStateIsReady();
                //}
                //else
                //{
                //    newPlayerItem.isMasterClient(false);
                //}

                playerItemsList.Add(newPlayerItem);
            }
        }

        // if (playerVRItemParent.childCount == 0)
        // {
        //     //OnClickAddBotVR();
        // }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RoundCount"))
            roundText.text = PhotonNetwork.CurrentRoom.CustomProperties["RoundCount"].ToString();

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("isPrivate"))
        {
            if (Convert.ToBoolean(PhotonNetwork.CurrentRoom.CustomProperties["isPrivate"]))
                roomPrivateStateText.text = "PRIVATE";
            else
                roomPrivateStateText.text = "PUBLIC";
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //StartCoroutine(UpdatePlayerList(0.2f));
    
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //HashTable hash = new HashTable();
        //otherPlayer.SetCustomProperties(hash);
        //otherPlayer.CustomProperties.Clear();
        StartCoroutine(UpdatePlayerList(0));
    }

    public IEnumerator OnPlay()
    {
        customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        customProperties.Remove("RoundCount");
        customProperties.Add("RoundCount", roundText.text);
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

        doOnce3 = true;
        gameStartPanel.SetActive(true);
        //view.RPC("Sendmessage", RpcTarget.Others, playerItemsList[PhotonNetwork.LocalPlayer.ActorNumber - 1].robotNameID.text.ToString(), playerItemsList[PhotonNetwork.LocalPlayer.ActorNumber - 1].robotNameText.text.ToString());
        yield return new WaitForSeconds(1);
        countdownText.text = "4";
        yield return new WaitForSeconds(1);
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1"; 
        yield return new WaitForSeconds(1);
        if (lobbySettings.isVR == true)
        {
            SceneManager.LoadScene("GameLobbyVR");
        }
        else
        {
            SceneManager.LoadScene("GameLobby");
        }
        //view.RPC("RPCAsyncLoadScene", RpcTarget.All, "RobotGameScene");
        //StartCoroutine(LoadYourAsyncScene("RobotGameScene"));
    }

    public void OnStartGame()
    {

        view.RPC("StartGamePanel", RpcTarget.All);

        //view.RPC("Sendmessage", RpcTarget.Others, playerItemsList[PhotonNetwork.LocalPlayer.ActorNumber - 1].robotNameID.text.ToString(), playerItemsList[PhotonNetwork.LocalPlayer.ActorNumber - 1].robotNameText.text.ToString());
        
    }

    [PunRPC]
    public void StartGamePanel()
    {
        StartCoroutine(OnPlay());

        //for (int i = 0; i < playerItemsList.Count; i++)
        //{
      
          //  playerItemsList[i].RobotSelectPanel.gameObject.SetActive(true);
        //}
    }

    public void OnClickManagePlayer(Player player)
    {
        //managePanel.SetActive(true);
        playerToManage = player;
        //managePanelPlayerNameText.text = "<sprite=0 color=#e4b54d>" + playerToManage.NickName;
    }

    public void OnClickManageClose()
    {
        managePanel.SetActive(false);
        playerToManage = null;
    }

    public void OnSetMaster()
    {
        // for (int i = 0; i < makeHostButtons.transform.childCount-1; i++)
        // {
        //     if (EventSystem.current.currentSelectedGameObject.name == makeHostButtons.transform.GetChild(i).gameObject.name)
        //     {
        //         EventSystem.current.currentSelectedGameObject.SetActive(false);
        //         playerItemSpawns[i].GetChild(0).gameObject.GetComponent<PlayerItem>().OnClickItem();
        //         playerItemSpawns[i].GetChild(0).gameObject.GetComponent<PlayerItem>().doOnce = false;
        //         PhotonNetwork.SetMasterClient(playerToManage);
        //         playerItemSpawns[i].GetChild(0).gameObject.GetComponent<PlayerItem>().UpdatePlayerItem(playerToManage);
        //         view.RPC("UpdateRPC", RpcTarget.All);
        //         break;
        //     }
        // }
    }

    [PunRPC]
    public void UpdateRPC()
    {
        StartCoroutine(UpdatePlayerList(0));
    }

    // public void OnSetMasterVR()
    // {
    //     EventSystem.current.currentSelectedGameObject.SetActive(false);
    //     playerVRItemParent.transform.GetChild(0).GetComponent<PlayerItem>().OnClickItem();
    //     playerVRItemParent.transform.GetChild(0).gameObject.GetComponent<PlayerItem>().doOnce = false;
    //     PhotonNetwork.SetMasterClient(playerToManage);
    //     playerVRItemParent.transform.GetChild(0).gameObject.GetComponent<PlayerItem>().UpdatePlayerItem(playerToManage);
    //     view.RPC("UpdateRPC", RpcTarget.All);
    // }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //StartCoroutine(UpdatePlayerList(0.3f));
    }

    public void OnClickManageKick()
    {
        if (PhotonNetwork.IsMasterClient && playerToManage != null)
            OnKickRoom(playerToManage);
    }
    
    public void OnKickRoom(Player player)
    {
        PhotonNetwork.CloseConnection(player);
        OnClickManageClose();
    }

    public void SetPopupPanel(string message)
    {
        popupPanelMessageText.text = message;
        popupPanel.SetActive(true);
    }

    public void OnClickPopupClose()
    {
        popupPanel.SetActive(false);
    }

    public void ToggleReadyButtonColor(Player player, bool isReady)
    {
        if (player == PhotonNetwork.LocalPlayer)
        {
            Color textLightColor;
            Color buttonLightColor;
            Color buttonMainColor;

            ColorUtility.TryParseHtmlString("#FDC955", out textLightColor);
            ColorUtility.TryParseHtmlString("#76694B", out buttonLightColor);
            ColorUtility.TryParseHtmlString("#262626", out buttonMainColor);

            if (isReady)
            {
                readyToggle = true;
                readyButtonImage.color = buttonLightColor;
                readyButtonImage.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = textLightColor;
            }
            else
            {
                readyToggle = false;
                readyButtonImage.color = buttonMainColor;
                readyButtonImage.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = Color.white;
            }
        }
    }
}
