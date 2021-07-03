using UnityEngine;

namespace BOYAREngine.Game
{
    public class AnswerDecideButtons : MonoBehaviour
    {


        public void OnRight()
        {
            QuestionManager.Instance.BackToThemeClientRpc();
            gameObject.SetActive(false);

            GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value += GameManager.Instance.QuestionPrice;
        }

        public void OnWrong()
        {
            QuestionManager.Instance.BackToThemeClientRpc();
            gameObject.SetActive(false);

            GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value -= GameManager.Instance.QuestionPrice;
        }
    }
}

