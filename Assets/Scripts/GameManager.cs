using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public InputField playerNickname;
    private string setName = "";
    public GameObject connecting;
    // Start is called before the first frame update
    void Start()
    {
        connecting.SetActive(false);
    }

    // Update is called once per frame
    public void UpdateText()
    {
        setName = playerNickname.text;
        PhotonNetwork.LocalPlayer.NickName = setName;
    }

    public void EnterButton()
    {
        if (setName != "")
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            connecting.SetActive(true);
        }
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("I'm connected to the server!");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Floor layout");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom("Arena1");
    }
}
