using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    public TMP_Text buttonJoinText;
    public TMP_Text buttonCreateText;
    public TMP_Text nameChangeButtonText;
    public TMP_Text nameText;
    public GameObject inputObject;
    public GameObject cam;
    public GameObject changeNameButton;
    public GameObject changeNameApplyButton;
    public AnimationCurve moveCurve;

    public static bool createOrJoin;
    public static string nickname = "";
    public float speed = 30.0F;
    private float result;
    private float t = 0.0F;
    private bool movingRight = true;
    private TouchScreenKeyboard overlayKeyboard;
    private bool start = false;
    public Transform positionToMoveTo;
    public GameObject loginCanvas;
    public GameObject loadingCanvas;
    public bool VR;
    public static bool isVR;

    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
    
    Vector3 firstLocation;
    Vector3 lastLocation;

    void Start()
    {
        isVR = VR;

        if (PhotonNetwork.IsConnected)
        {
            customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            customProperties.Remove("Team");
            customProperties.Add("Team", null);
            customProperties.Remove("PPTCount");
            customProperties.Add("PPTCount", null);
            customProperties.Remove("NoInTeam");
            customProperties.Add("NoInTeam", null);
            customProperties.Remove("isReady");
            customProperties.Add("isReady", 0);
        }

        StartCoroutine(StartFade(0.4F));
        Debug.Log(PhotonNetwork.OfflineMode);
        Vector3 firstLocation = cam.transform.localPosition;
        Debug.Log(PlayerPrefs.GetString("NickName"));
        overlayKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);

        if (PlayerPrefs.GetString("NickName") != "")
        {
            inputObject.SetActive(false);
            changeNameButton.SetActive(false);
            nameText.text = PlayerPrefs.GetString("NickName");
        }
        else
        {
            changeNameButton.GetComponent<Button>().interactable = true;
        }

        if (!inputObject.activeSelf)
        {
            changeNameButton.SetActive(true);
        }    

        //if (nickname != "")
        //{
        //    inputObject.SetActive(false);
        //    nameText.text = nickname;
        //}
      
    }

    public void deneme()
    {
        Debug.Log("click çalışıyo");
    }

    void Update()
    {
        //foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        //{
        //    if (Input.GetKey(kcode))
        //        Debug.Log("KeyCode down: " + kcode);
        //}
    }

    IEnumerator StartFade(float duration)
    {
        float time = 0;
        CanvasGroup canvasGroup = loginCanvas.GetComponent<CanvasGroup>();
        float startAlpha = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1, moveCurve.Evaluate(time / duration));
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator CameraMover(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = cam.transform.position;
        CanvasGroup canvasGroup = loginCanvas.GetComponent<CanvasGroup>();
        float startAlpha = 1;
        
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, moveCurve.Evaluate(time / duration));
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, moveCurve.Evaluate(time / duration));
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        cam.transform.position = targetPosition;

        time = 0;
        CanvasGroup loadingCanvasGroup = loadingCanvas.GetComponent<CanvasGroup>();
        while (time < duration)
        {
            loadingCanvasGroup.alpha = Mathf.Lerp(0, 1, moveCurve.Evaluate(time / duration));
            time += Time.deltaTime;
            yield return null;
        }
        loadingCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(0.1f);
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            if (isVR == true)
            {
                SceneManager.LoadScene("LobbyVR");
            }
            else
            {
                SceneManager.LoadScene("Lobby");
            }
        }
    }

    public void OnClickChangeNameConnect()
    {
        inputObject.SetActive(true);
        changeNameButton.SetActive(false);
        changeNameApplyButton.SetActive(true);
        nameText.text = "";
    }

    public void OnClickApplyNameChange()
    {
        if (usernameInput.text.Length >= 1 || !inputObject.activeSelf)
        {
            nickname = usernameInput.text;
            PlayerPrefs.SetString("NickName", nickname);
            PlayerPrefs.Save();
            nameText.text = PlayerPrefs.GetString("NickName");
            changeNameApplyButton.SetActive(false);
            changeNameButton.SetActive(true);
            inputObject.SetActive(false);
        }
    }

    public void OnClickJoin() //Join butonuna basılınca oyuncu photona bağlanır ve lobbye aktarılır.
    {
        if (usernameInput.text.Length >= 1 || !inputObject.activeSelf)
        {
            if (inputObject.activeSelf)
            {
                OnClickApplyNameChange();
            }
            changeNameButton.SetActive(false);

            if (PlayerPrefs.GetString("NickName") != "")
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
            }
            else
            {
                PhotonNetwork.NickName = usernameInput.text;
            }
            nickname = PhotonNetwork.NickName;

            PlayerPrefs.SetString("NickName", nickname);
            PlayerPrefs.Save();
            buttonJoinText.text = "CONNECTING...";
            PhotonNetwork.AutomaticallySyncScene = true;

            createOrJoin = true;
            StartCoroutine(CameraMover(positionToMoveTo.position, 0.6f));
        }
    }

    public void OnClickCreate() //Create butonuna basınca oyuncu photona bağlanır ve oda yaratılır.
    {

        Debug.Log("deneme");
        if (usernameInput.text.Length >= 1 || !inputObject.activeSelf)
        {
            Debug.Log("deneme1");
            if (inputObject.activeSelf)
            {
                Debug.Log("deneme3");
                OnClickApplyNameChange();
            }
            changeNameButton.SetActive(false);

            if (PlayerPrefs.GetString("NickName") != "")
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
            }
            else
            {
                PhotonNetwork.NickName = usernameInput.text;
            }
            nickname = PhotonNetwork.NickName;

            PlayerPrefs.SetString("NickName", nickname);
            PlayerPrefs.Save();
            buttonCreateText.text = "CONNECTING...";
            PhotonNetwork.AutomaticallySyncScene = true;

            createOrJoin = false;
            StartCoroutine(CameraMover(positionToMoveTo.position, 0.6f));
        }
       
    }

    public override void OnConnectedToMaster()
    {
        if (isVR == true)
        {
            SceneManager.LoadScene("LobbyVR");
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}
