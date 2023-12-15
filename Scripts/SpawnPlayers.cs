using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<Transform> spawnPositions3v3 = new List<Transform>();
    public List<Transform> spawnPositions2v2 = new List<Transform>();
    public List<Transform> spawnPositions1v1 = new List<Transform>();
    public List<Transform> selectedMode = new List<Transform>();
    private int actorNumber;
    public bool oneTime = false;
    int spawnNumber;
    int teamNo;
    int noInTeam;
    int teamSize;
    public PhotonView view;
    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();

    private void Start()
    {
        if(!PhotonNetwork.IsConnected) { SceneManager.LoadScene("Main"); }
        actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"] != null);
        teamSize = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]);

        switch (teamSize)
        {
            case 1:
                selectedMode = spawnPositions1v1;
                break;
            case 2:
                selectedMode = spawnPositions2v2;
                break;
            case 3:
                selectedMode = spawnPositions3v3;
                break;
        }
        StartCoroutine(SpawnPlayer(selectedMode, true, 0.6f));
    }

    public IEnumerator SpawnPlayer(List<Transform> spawnPositions, bool isStart, float delay)
    {
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties["NoInTeam"] != null);
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties["Team"] != null);
        
        yield return new WaitForSeconds(delay);
        noInTeam = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["NoInTeam"]);
        teamNo = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["Team"]);
        teamSize = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]);

        //spawnNumber = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["NoInTeam"]) + (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["PPTCount"]) * (Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["Team"]) - 1));
        spawnNumber = noInTeam + (teamSize * (teamNo - 1));

        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("spawnNumber");
        customProperties.Add("spawnNumber", spawnNumber);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        view.RPC("NewMethod", RpcTarget.All);

        if (oneTime == false)
        {
            oneTime = true;
            GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPositions[spawnNumber].position, spawnPositions[spawnNumber].rotation);
            player.transform.GetChild(5).gameObject.SetActive(false);
            yield return new WaitForSeconds(1f);
            player.transform.GetChild(5).gameObject.SetActive(true);
            player.name = PhotonNetwork.LocalPlayer.NickName;
        }

        if (isStart)
        {
            oneTime = false;
        }
    }

    [PunRPC]
    public void NewMethod()
    {
        //Debug.Log(spawnNumber);
    }
}
