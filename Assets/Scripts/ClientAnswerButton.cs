using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class ClientAnswerButton : NetworkBehaviour
    {
        public void OnClick()
        {
            ChangeActivePlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeActivePlayerServerRpc(ulong id)
        {
            DisableAnswerButtonClientRpc();

            //var clientIndex = 0;
            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<NetworkObject>().OwnerClientId == id)
                {
                    //clientIndex = i;
                    GameManager.Instance.ChangeActivePlayer(i);
                }
            }

            //GameManager.Instance.ChangeActivePlayer(clientIndex);
        }

        [ClientRpc]
        private void DisableAnswerButtonClientRpc()
        {
            QuestionManager.Instance.AnswerButtonGameObject.SetActive(false);
            Debug.Log("Disable Answer Button for All Clients");
        }

    }
}

