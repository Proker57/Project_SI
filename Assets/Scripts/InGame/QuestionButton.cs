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
                ShowQuestionClientRpc();
                ShowQuestionHost();
            }

            if (IsOwner && IsClient && !IsHost)
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

        [ClientRpc]
        private void ShowQuestionClientRpc()
        {
            if (!IsHost)
            {
                QuestionManager.Instance.ShowQuestionClient(ThemeIndex, QuestionIndex);
            }

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

