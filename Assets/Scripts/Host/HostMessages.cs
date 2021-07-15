using MLAPI;
using MLAPI.Messaging;

namespace BOYAREngine.Game
{
    public class HostMessages : NetworkBehaviour
    {
        public void SetActivePlayer(ulong id)
        {
            SetActivePlayerServerRpc(id);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetActivePlayerServerRpc(ulong id)
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
        }
    }
}

