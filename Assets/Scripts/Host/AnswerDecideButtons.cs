using UnityEngine;

namespace BOYAREngine.Game
{
    public class AnswerDecideButtons : MonoBehaviour
    {
        public void OnRight()
        {
            gameObject.SetActive(false);

            GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value += GameManager.Instance.QuestionPriceCurrent;

            QuestionManager.Instance.IsRightAnswer = true;
            QuestionManager.Instance.ShowAnswerHost(GameManager.Instance.ThemeIndexCurrent, GameManager.Instance.QuestionIndexCurrent);
        }

        public void OnWrong()
        {
            gameObject.SetActive(false);

            GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value -= GameManager.Instance.QuestionPriceCurrent;

            QuestionManager.Instance.IsRightAnswer = false;
            QuestionManager.Instance.ShowAnswerHost(GameManager.Instance.ThemeIndexCurrent, GameManager.Instance.QuestionIndexCurrent);
        }
    }
}

