using MLAPI;
using MLAPI.Messaging;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class QuestionButton : NetworkBehaviour
    {
        public int ThemeIndex;
        public int QuestionIndex;

        public void OnQuestion()
        {
            // TODO: Check Id
            if (IsOwner && IsHost)
            {
                ShowQuestionHost();

                ShowQuestionClientRpc();
            }

            if (IsOwner && IsClient)
            {
                ShowQuestionHostServerRpc();
            }
        }

        private void ShowQuestionHost()
        {
            QuestionManager.Instance.ShowQuestionHost(ThemeIndex, QuestionIndex);

            GameManager.Instance.ThemeIndexCurrent = ThemeIndex;
            GameManager.Instance.QuestionIndexCurrent = QuestionIndex;

            GetComponent<Button>().interactable = false;
            GetComponentInChildren<Text>().text = null;
        }

        [ClientRpc(Delivery = RpcDelivery.Reliable)]
        private void ShowQuestionClientRpc()
        {
            QuestionManager.Instance.ShowQuestionClient(ThemeIndex, QuestionIndex);

            GetComponent<Button>().interactable = false;
            GetComponentInChildren<Text>().text = null;
        }

        [ServerRpc]
        private void ShowQuestionHostServerRpc()
        {
            QuestionManager.Instance.ShowQuestionHost(ThemeIndex, QuestionIndex);

            GameManager.Instance.ThemeIndexCurrent = ThemeIndex;
            GameManager.Instance.QuestionIndexCurrent = QuestionIndex;

            GetComponent<Button>().interactable = false;
            GetComponentInChildren<Text>().text = null;

            ShowQuestionClientRpc();
        }
    }

}
