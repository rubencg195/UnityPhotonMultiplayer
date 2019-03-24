using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

namespace com.MyCompany.MyGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        #region Public Variables
        public static GameManager Instance;
        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;
        #region Monobehaviour Callbacks
        private void Start()
        {
            Instance = this;
            /*
             Notice we've decorated the Instance variable with the [static] keyword,
             meaning that this variable is available without having to hold a pointer 
             to an instance of GameManager, so you can simply do GameManager.Instance.xxx() 
             from anywhere in your code. It's very practical indeed! Let's see how that 
             fits for our game for logic management
             */
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
        #endregion
        #endregion
        #region Photon Callbacks
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName);
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName);
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }
        }
        #endregion
        #region Public Methods
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
        public void QuitApplication()
        {
            Application.Quit();
        }
        #endregion
        #region LoadArena
        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork: Trying to load a level but we are not in the master client");
            }
            Debug.LogFormat("PhotonNetwork: Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for "+PhotonNetwork.CurrentRoom.PlayerCount);
            /*
             PhotonNetwork.LoadLevel() should only be called if we are the MasterClient. 
             So we check first that we are the MasterClient using PhotonNetwork.IsMasterClient.
             It will be the responsibility of the caller to also check for this, we'll cover that 
             in the next part of this section.
            We use PhotonNetwork.LoadLevel() to load the level we want, we don't use Unity directly, 
            because we want to rely on Photon to load this level on all connected clients in the room,
            since we've enabled PhotonNetwork.AutomaticallySyncScene for this Game.
             */
        }
        #endregion
        
    }
}
