using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace com.MyCompany.MyGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields
        #endregion

        #region Private Fields
        //Users are separated by gameversion
        [Tooltip("The maximum number of players per room. When room is full, it can'y be joined by new players, and so new room will be created.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;
        [Tooltip("The UI Panel to let the user enter the name, connect and play")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;
        [SerializeField]
        private GameObject RoomGroupLayout;
        [SerializeField]
        private GameObject RoomLayoutItem;
        [SerializeField]
        private InputField RoomNameInputField;


        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        private bool isConnecting;

        string gameVersion = "1";
        #endregion

        #region MonoBehaviour Callbacks
        private void Start()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            RoomNameInputField.text = "RubenRoomDemo";
        }
        private void Awake()
        {
            //Method called on Gameobject by Unity during initialization phase
            //This makes sure we can use PhotonNetwork.LoadLevel() on the master client and
            //all the clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.OfflineMode = false;
            /*
             Our game will have a resizable arena based on the number of players, 
             and to make sure that the loaded scene is the same for every connected
             player, we'll make use of the very convenient feature provided by 
             Photon: PhotonNetwork.AutomaticallySyncScene When this is true, 
             the MasterClient can call PhotonNetwork.LoadLevel() and all 
             connected players will automatically load that same level.
             */

        }
        public override void OnConnectedToMaster()
        {
            //implementation of base class not needed. Only for OnEnable and OnDisable override.
            Debug.Log("Connected To Master. Trying to Join a Random Room.");
            PhotonNetwork.JoinLobby(TypedLobby.Default);
            // #Critical: The first we try to do is to join a potential existing room. 
            //If there is, good, else, we'll be called back with OnJoinRandomFailed()

            // we don't want to do anything if we are not attempting to join a room.
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            
        }
        public override void OnJoinedLobby()
        {
            print("Joined Lobby");
            if (isConnecting)
            {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                //PhotonNetwork.JoinRoom("RubenRoomDemo");
                
            }
        }
        public void OnCreateRoom()
        {
            isConnecting = true;
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            PhotonNetwork.CreateRoom(RoomNameInputField.text, new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            }, TypedLobby.Default);
        }
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (Transform child in RoomGroupLayout.transform)
            {
                Destroy(child.gameObject);
            }
            print("RoomListUpdated");
            foreach(RoomInfo room in roomList)
            {
                print(room);
                GameObject _item = Instantiate(RoomLayoutItem);
                _item.transform.SetParent(RoomGroupLayout.transform, false);
                RoomLayoutItem layoutItem = _item.GetComponent<RoomLayoutItem>();
                layoutItem.SetRoomText(room.Name, room.PlayerCount, room.MaxPlayers);


            }
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Joining Room Failed, Maybe not exist or full. Trying to Create a Room");

            // #Critical: we failed to join a random room,
            // maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(RoomNameInputField.text, new RoomOptions {
                MaxPlayers = maxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            }, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room successfully. Now this client is in a room.");
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1'");
                PhotonNetwork.LoadLevel("Room for 1");
            }
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("Disconected, Cause: {0}", cause);
        }
        #endregion

        #region Public Methods
        public void Connect()
        {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = true;
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            //Check if we are connected or not. We Join if we are
            //Else we initiate the connection to the server
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRoom(RoomNameInputField.text);
            }
            else
            {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        #endregion
    }
}