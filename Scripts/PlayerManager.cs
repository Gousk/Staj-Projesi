using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PhotonView photonView;
    void Start()
    {
        StartCoroutine(WaitForPhoton()); 
    }

    IEnumerator WaitForPhoton()
		{
        yield return new WaitUntil(() => PhotonNetwork.InRoom==true);
        photonView = GetComponent<PhotonView>();
        }
 
}
