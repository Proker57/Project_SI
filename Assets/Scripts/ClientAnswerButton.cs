using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class ClientAnswerButton : MonoBehaviour
    {
        public void OnClick()
        {
            ChangeActivePlayerServerRpc();
        }

        [ServerRpc]
        private void ChangeActivePlayerServerRpc()
        {
            var clientIndex = 0;
            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<PlayerData>().Id == NetworkManager.Singleton.LocalClientId)
                {
                    clientIndex = i;
                }
            }

            GameManager.Instance.ChangeActivePlayer(clientIndex);

            DisableAnswerButton();
        }

        [ClientRpc]
        private void DisableAnswerButton()
        {
            QuestionManager.Instance.AnswerButtonGameObject.SetActive(false);
            Debug.Log("Disable Answer Button for All Clients");
        }

    }
}

