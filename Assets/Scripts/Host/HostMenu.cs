using System.Collections.Generic;
using MLAPI;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class HostMenu : MonoBehaviour
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
                GameManager.Instance.QuestionButtonsList = new List<NetworkObject>();

                //GameManager.Instance.NetQuestionRowCount = new NetworkVariable<byte>();
                //GameManager.Instance.NetQuestionColumnCount = new NetworkVariable<byte>();

                GameManager.Instance.NetThemeNames = new NetworkList<string>();
                GameManager.Instance.NetQuestionPrice = new NetworkDictionary<Vector2, string>();


                GameManager.Instance.Round++;
                GameManager.Instance.HostCreate.SetupHostRound();
            }
        }
    }
}

