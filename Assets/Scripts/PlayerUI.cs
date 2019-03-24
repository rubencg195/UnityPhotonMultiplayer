using UnityEngine;
using UnityEngine.UI;


using System.Collections;


namespace com.MyCompany.MyGame
{
    public class PlayerUI : MonoBehaviour
    {
        #region Private Fields
        private PlayerManager target;

        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private Text playerNameText;


        [Tooltip("UI Slider to display Player's Health")]
        [SerializeField]
        private Slider playerHealthSlider;

        [Tooltip("Pixel offset from the player target")]
        [SerializeField]
        private Vector3 screenOffset = new Vector3(0f, 30f, 0f);


        float characterControllerHeight = 0f;
        Transform targetTransform;
        Renderer targetRenderer;
        Vector3 targetPosition;

        #endregion


        #region MonoBehaviour Callbacks
        void Awake()
        {
            //One very important constraint with the Unity UI system is that any UI element must be placed within a Canvas GameObject, and so we need to handle this when this PlayerUI Prefab will be instantiated, we'll do this during the initialization of the PlayerUI script.
            // Why going brute force and find the Canvas this way? Because when scenes are going to be loaded and unloaded, so is our Prefab, and the Canvas will be everytime different. To avoid more complex code structure, we'll go for the quickest way. However it's really not recommended to use "Find", because this is a slow operation. This is out of scope for this tutorial to implement a more complex handling of such case, but a good exercise when you'll feel comfortable with Unity and scripting to find ways into coding a better management of the reference of the Canvas element that takes loading and unloading into account.
            this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        }
        void Update()
        {
            // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
            //However this is far from being sufficient, we need to deal with the deletion of the player, we certainly don't want to have orphan UI instances all over the scene, so we need to destroy the UI instance when it finds out that the target it's been assigned is gone.
            //Save PlayerUI script This code, while easy, is actually quite handy. Because of the way Photon deletes Instances that are networked, it's easier for the UI instance to simply destroy itself if the target reference is null. This avoids a lot of potential problems, and is very secure, no matter the reason why a target is missing, the related UI will automatically destroy itself too, very handy and quick. But wait... when a new level is loaded, the UI is being destroyed yet our player remains... so we need to instantiate it as well when we know a level was loaded, let's do this:
            if (target == null)
            {
                Destroy(this.gameObject);
                return;
            }
            // Reflect the Player Health
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = target.Health;
            }
        }
        void LateUpdate()
        {
            /*
             So, the trick to match a 2D position with a 3D position is to use the WorldToScreenPoint function of a camera and since we only have one in our game, we can rely on accessing the Main Camera which is the default setup for a Unity Scene.
````````````Notice how we setup the offset in several steps: first we get the actual position of the target, then we add the characterControllerHeight, and finally, after we've deduced the screen position of the top of the Player, we add the screen offset.
             */
            // Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
            if (targetRenderer != null)
            {
                this.gameObject.SetActive(targetRenderer.isVisible);
            }


            // #Critical
            // Follow the Target GameObject on screen.
            if (targetTransform != null)
            {
                targetPosition = targetTransform.position;
                targetPosition.y += characterControllerHeight;
                this.transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
            }
        }
        #endregion


        #region Public Methods
        public void SetTarget(PlayerManager _target)
        {
            Debug.Log("SetTarget: "+_target+" photonView Owner: "+ _target.photonView.Owner);
            if (_target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            // Cache references for efficiency
            target = _target;
            targetTransform = this.target.GetComponent<Transform>();
            targetRenderer = this.target.GetComponent<Renderer>();
            CharacterController characterController = _target.GetComponent<CharacterController>();

            // Get data from the Player that won't change during the lifetime of this Component
            if (characterController != null)
            {
                characterControllerHeight = characterController.height;
            }

            if (playerNameText != null && target.photonView.Owner != null)
            {
                playerNameText.text = target.photonView.Owner.NickName;
            }
            else
            {
                Debug.LogError("Owner NickName Null, are you playing Offline? No Nickname found.");
            }
        }

        #endregion


    }
}