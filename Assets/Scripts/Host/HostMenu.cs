using MLAPI;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class HostMenu : NetworkBehaviour
    {
        [SerializeField] private GameObject _menuGameObject;
        [SerializeField] private GameObject _answerPanel;
        [Space]
        [SerializeField] private GameObject[] _activePlayerButtons;

        public void OnMenuButtonClick()
        {
            _menuGameObject.SetActive(!_menuGameObject.activeSelf);

            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                _activePlayerButtons[i].SetActive(true);
            }
        }

        public void OnAnswerPanelClick()
        {
            _answerPanel.SetActive(!_answerPanel.activeSelf);
        }

        public void OnSkipQuestion()
        {
            if (QuestionManager.Instance.IsShowQuestion)
            {
                var themeIndex = GameManager.Instance.ThemeIndexCurrent;
                var questionIndex = GameManager.Instance.QuestionIndexCurrent;
                QuestionManager.Instance.ShowAnswerHost(themeIndex, questionIndex);
            }
        }

        public void OnSkipRound()
        {
            if (GameManager.Instance.Round < GameManager.Instance.Rounds.Count - 1)
            {
                RoundManager.Instance.StopTimer();

                HostManager.Instance.Messages.NextRoundClientRpc();
            }
        }
    }
}

