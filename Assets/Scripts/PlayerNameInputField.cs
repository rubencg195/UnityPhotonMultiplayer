using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

namespace com.MyCompany.MyGame
{
    //Input Field Let The User input his name and will appear above the player in the game.
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {

        #region Private Constants
        const string playerNamePrefKey = "PlayerName";
        #endregion

        #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start()
        {
            string defaultName = string.Empty;
            InputField _inputField = this.GetComponent<InputField>();
            if(_inputField != null)
            {
                //PlayerPrefs stores players preferences between game sessions
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _inputField.text = defaultName;
                }
            }
            PhotonNetwork.NickName = defaultName;
        }
        #endregion

        #region Public Methods
        public void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player Name is Null or Empty"+ value);
                return;
            }
            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(playerNamePrefKey, value);
        }
        #endregion
    }
}
