using UnityEngine;

namespace BOYAREngine.Game
{
    public class AnswerDecideButtons : MonoBehaviour
    {
        public void OnRight()
        {
            GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value += GameManager.Instance.QuestionPriceCurrent;
            QuestionManager.Instance.IsRightAnswer = true;
            QuestionManager.Instance.ShowAnswerHost(GameManager.Instance.ThemeIndexCurrent, GameManager.Instance.QuestionIndexCurrent);
            gameObject.SetActive(false);
        }

        public void OnWrong()
        {
            GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value -= GameManager.Instance.QuestionPriceCurrent;
            HostManager.Instance.Messages.ResetColorsClientRpc();
            HostManager.Instance.Messages.EnableAnswerButtonClientRpc();
            QuestionManager.Instance.IsRightAnswer = false;
        }
    }
}

