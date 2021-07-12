using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace BOYAREngine.Game
{
    public class HostMessages : NetworkBehaviour
    {
        public void SetActivePlayer(ulong id)
        {
            SetActivePlayerServerRpc(id);

            

            Debug.Log($"Id: {id}");
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetActivePlayerServerRpc(ulong id)
        {
            Debug.Log($"Id: {id}");
            DisableAnswerButtonClientRpc();

            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<NetworkObject>().OwnerClientId == id)
                {
                    GameManager.Instance.ChangeActivePlayer(i);

                    Debug.Log($"Active player: {i}");
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

