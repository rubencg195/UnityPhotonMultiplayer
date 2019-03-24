using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Collections;

namespace com.MyCompany.MyGame
{
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields
        [Tooltip("The current Health of our player")]
        public float Health = 1f;
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion
        #region Private Fields
        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject playerUiPrefab;
        //True, when the user is firing
        [SerializeField]
        [Tooltip("Laser With the Collider object")]
        public GameObject LaserWithCollider;
        bool IsFiring;
        #endregion

        #region MonoBehaviour CallBacks
        private void Start()
        {
            CameraWork _cameraWork = gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
            }

            // Create the UI
            if (this.playerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(this.playerUiPrefab);
                PlayerUI _playerUI = _uiGo.GetComponent<PlayerUI>();
                Debug.Log("CallingSetTarge prefab : "+ _uiGo + " PlayerUI: "+ _playerUI );
                _playerUI.SetTarget(this);
                //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            #if UNITY_5_4_OR_NEWER
                        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
                        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
        }
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            if (this.beams == null)
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> Beams Reference.", this);
            }
            else
            {
                this.beams.SetActive(false);
            }

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            // we only process Inputs and check health if we are the local player
            if (photonView.IsMine)
            {
                this.ProcessInputs();

                if (this.Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
                }
            }

            if (this.beams != null && this.IsFiring != this.beams.activeInHierarchy)
            {
                this.beams.SetActive(this.IsFiring);
            }
        }


        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();

            #if UNITY_5_4_OR_NEWER
                        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            #endif
        }
        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>
        public void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (other.GetInstanceID() == LaserWithCollider.GetInstanceID())
            {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f;
        }

        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other.</param>
        public void OnTriggerStay(Collider other)
        {
            // we dont' do anything if we are not the local player.
            if (!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
            this.Health -= 0.1f * Time.deltaTime;
        }


        #if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        #endif


        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            GameObject _uiGo = Instantiate(this.playerUiPrefab);
            PlayerUI _playerUI = _uiGo.GetComponent<PlayerUI>();
            Debug.Log("CallingSetTarge prefab : " + _uiGo + " PlayerUI: " + _playerUI);
            _playerUI.SetTarget(this);
            //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            //Note that there are more complex/powerful ways to deal with this and the UI could be made out with a singleton, but it would quickly become complex, because other players joining and leaving the room would need to deal with their UI as well. In our implementation, this is straight forward, at the cost of a duplication of where we instantiate our UI prefab. As a simple exercise, you can create a private method that would instantiate and send the "SetTarget" message, and from the various places, call that method instead of duplicating the code.
        }
        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1") && !Input.GetKeyDown("space"))
            {
                // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
                // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //	return;
                }

                if (!IsFiring)
                {
                    IsFiring = true;
                }
            }
            if (Input.GetButtonUp("Fire1") || Input.GetKeyDown("space"))
            {
                if (IsFiring)
                {
                    IsFiring = false;
                }
            }
        }

        #endregion

        #region IPunObservable implementation

        /*
        However, when testing this, we only see the local player firing. We need to see when another instance fires, too. We need a mechanism for synchronizing the firing across the network. To do this, we are going to manually synchronize the IsFiring boolean value, until now, we got away with PhotonTransformView and PhotonAnimatorView to do all the internal synchronization of variables for us, we only had to tweak what was conveniently exposed to us via the Unity Inspector, but here what we need is very specific to your game, and so we'll need to do this manually. 
        */
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }


        #endregion


        /*
        What this new code does is watching for a level being loaded, and raycast downwards the current player's position to see if we hit anything. If we don't, this is means we are not above the arena's ground and we need to be repositioned back to the center, exactly like when we are entering the room for the first time.
        If you are on a Unity version lower than Unity 5.4, we'll use Unity's callback OnLevelWasLoaded. If you are on Unity 5.4 or up, OnLevelWasLoaded is not available anymore, instead you have to use the new SceneManagement system. Finally, to avoid duplicating code, we simply have a CalledOnLevelWasLoaded method that will be called either from OnLevelWasLoaded or from the SceneManager.sceneLoaded callback. 
        */
        #region Private Methods
        #if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
                {
                    this.CalledOnLevelWasLoaded(scene.buildIndex);
                }
        #endif
        #endregion
    }
}