using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class HostMessages : NetworkBehaviour
    {
        public void SetActivePlayer(ulong id)
        {
            SetActivePlayerClientRpc(id);
        }

        [ClientRpc]
        private void SetActivePlayerClientRpc(ulong id)
        {
            if (IsHost)
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
        }

        [ClientRpc]
        private void DisableAnswerButtonClientRpc()
        {
            QuestionManager.Instance.AnswerButtonGameObject.SetActive(false);
            Debug.Log("Disable Answer Button for All Clients");
        }
    }
}

