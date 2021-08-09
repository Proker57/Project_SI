using BOYAREngine.Net;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.UI;

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

        [ServerRpc(RequireOwnership = false)]
        public void ChangeColorServerRpc(int index)
        {
            ActivePlayerChangeColorClientRpc(index);
        }

        [ClientRpc]
        public void NextRoundClientRpc()
        {
            GameManager.Instance.ClearData();
            GameManager.Instance.Round++;

            if (IsHost)
            {
                HostManager.Instance.SetupHostRound();
                RoundManager.Instance.ShowIntro(GameManager.Instance.Rounds[GameManager.Instance.Round].Name);
                HostManager.Instance.SendRoundName(GameManager.Instance.Rounds[GameManager.Instance.Round].Name);
                HostManager.Instance.SendThemeNames();
                HostManager.Instance.SendQuestionPrices();

                RoundManager.Instance.StartRoundTimer();
            }
        }

        [ClientRpc]
        private void DisableAnswerButtonClientRpc()
        {
            QuestionManager.Instance.AnswerButtonGameObject.SetActive(false);
        }

        [ClientRpc]
        public void EnableAnswerButtonClientRpc()
        {
            if (!IsHost)
            {
                QuestionManager.Instance.AnswerButtonGameObject.SetActive(true);
            }
        }

        [ClientRpc]
        public void SetQuestionPriceClientRpc(int price)
        {
            GameManager.Instance.QuestionPriceCurrent = price;
        }

        [ClientRpc]
        public void TurnOnAuctionPanelsClientRpc()
        {
            GameManager.Instance.GetComponent<Auction>().ResetValues();
            GameManager.Instance.GetComponent<Auction>().TurnOnPanels();
        }

        [ClientRpc]
        private void ActivePlayerChangeColorClientRpc(int index)
        {
            ResetColorsClientRpc();

            // 191 121 164 Pink
            GameManager.Instance.Players[index].GetComponent<Image>().color = new Color32(191, 121, 164, 255);
        }

        [ClientRpc]
        public void ResetColorsClientRpc()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                // 69 121 164  Blue
                player.GetComponent<Image>().color = new Color32(64, 121, 164, 255);
            }
        }
    }
}

