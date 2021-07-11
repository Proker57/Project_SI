using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class ClientAnswerButton : NetworkBehaviour
    {
        public void OnClick()
        {
            ChangeActivePlayerServerRpc(OwnerClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeActivePlayerServerRpc(ulong id)
        {
            DisableAnswerButtonClientRpc();

            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<NetworkObject>().OwnerClientId == id)
                {
                    GameManager.Instance.ChangeActivePlayer(i);
                }
            }
        }

        [ClientRpc]
        private void DisableAnswerButtonClientRpc()
        {
            QuestionManager.Instance.AnswerButtonGameObject.SetActive(false);
            Debug.Log("Disable Answer Button for All Clients");
        }

    }
}

