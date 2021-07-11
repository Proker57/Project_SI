using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class ClientAnswerButton : MonoBehaviour
    {
        public void OnClick()
        {
            HostManager.Instance.Messages.SetActivePlayer(GameManager.Instance.NetId);
        }

        /*[ServerRpc]
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
        }*/

    }
}

