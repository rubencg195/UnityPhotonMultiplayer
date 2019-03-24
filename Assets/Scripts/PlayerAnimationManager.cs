using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.MyCompany.MyGame
{
    public class PlayerAnimationManager : MonoBehaviourPun
    {
        #region Private Fields
        [SerializeField]
        private float directionDampTime = 0.25f;
        private Animator animator;
        #endregion

        #region Monobehaviour Callbacks
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            if (!animator)
            {
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            /*
             A critical aspect of user control over the network is that the same prefab will be instantiated for all players, but only one of them represents the user actually playing in front of the computer, all other instances represents other users, playing on other computers. So the first hurdle with this in mind is "Input Management". How can we enable input on one instance and not on others and how to know which one is the right one? Enter the IsMine concept.
             Ok, photonView.IsMine will be true if the instance is controlled by the 'client' application, meaning this instance represents the physical person playing on this computer within this application. So if it is false, we don't want to do anything and solely rely on the PhotonView component to synchronize the transform and animator components we've setup earlier. But, why having then to enforce PhotonNetwork.IsConnected == true in our if statement? eh eh :) because during development, we may want to test this prefab without being connected. In a dummy scene for example, just to create and validate code that is not related to networking features per se. And so with this additional expression, we will allow input to be used if we are not connected. It's a very simple trick and will greatly improve your workflow during development.
             */
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            /*
                Since our game does not allow going backward, we make sure that v is less than 0. If the user is pressing the 'down arrow' or 's' key (default setting for the Verticalaxis), we do not allow this and force the value to 0.

                You'll also notice that we've squared both inputs. Why? So that it's always a positive absolute value as well as adding some easing. Nice subtle trick right here. You could use Mathf.Abs() too, that would work fine.

                We also add both inputs to control Speed, so that when just pressing left or right input, we still gain some speed as we turn.

                All of this is very specific to our character design of course, depending on your game logic, you may want the character to turn in place, or have the ability to go backward. The control of Animation Parameters is always very specific to the game.
             */
            if (!animator)
            {
                return;
            }
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (v < 0)
            {
                v = 0;
            }
            animator.SetFloat("Speed", h * h + v * v);
            animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);

            /*
             So we notice straight away that animator.SetFloat() has different signatures.
             The one we use for controlling the Speed is straightforward, but this one takes 
             two more parameters, one is the damping time, and one the deltaTime. Damping
             time makes sense: it's how long it will take to reach the desired value, but
             deltaTime?. It essentially lets you write code that is frame rate independent 
             since Update() is dependant on the frame rate, we need to counter this by using 
             the deltaTime. Read as much as you can on the topic and what you'll find when
             searching the web for this. After you understood the concept, you'll be able to
             make the most out of many Unity features when it comes to animation and
             consistent control of values over time.
             */

            // deal with Jumping
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // only allow jumping if we are running.
            if (stateInfo.IsName("Base Layer.Run"))
            {
                // When using trigger parameter
                if (Input.GetKeyDown("space"))
                {
                    animator.SetTrigger("Jump");
                }
            }
        }
        #endregion
    }
}
