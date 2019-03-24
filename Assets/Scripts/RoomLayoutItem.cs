using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RoomLayoutItem : MonoBehaviour
{
    [SerializeField]
    private Text RoomLayoutItemText;
    [SerializeField]
    private Text RoomLayoutItemPlayerCountText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetRoomText(string txt, int playerCount, byte MaxPlayers)
    {
        if (RoomLayoutItemText == null || RoomLayoutItemPlayerCountText == null)
        {
            Debug.LogError("No Room Layout Item Text Assign to Script");
            return;
        }

        RoomLayoutItemText.text = txt;
        RoomLayoutItemPlayerCountText.text = "" + playerCount + "/" + MaxPlayers;

    }
    public void ConnectToRoom()
    {
        if (RoomLayoutItemText == null)
        {
            Debug.LogError("No Room Layout Item Text Assign to Script");
            return;
        }
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Cannot Join Room, Photon Not Connected");
            return;
        }


        PhotonNetwork.JoinRoom(RoomLayoutItemText.text);
    }

}
