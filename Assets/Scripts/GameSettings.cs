using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class GameSettings : MonoBehaviour
    {
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _infoPanel;

        [SerializeField] private InputField _roundTimerInputField;
        [SerializeField] private InputField _questionTimerInputField;
        [SerializeField] private InputField _answerTimerInputField;

        private void Start()
        {
            // Round timer
            _roundTimerInputField.onValueChanged.AddListener(delegate
            {
                RoundTimerInputField_OnTImeChanged(_roundTimerInputField);
            });

            // Question timer
            _questionTimerInputField.onValueChanged.AddListener(delegate
            {
                QuestionTimerInputField_OnNameChanged(_questionTimerInputField);
            });

            // Answer timer
            _answerTimerInputField.onValueChanged.AddListener(delegate
            {
                AnswerTimerInputField_OnNameChanged(_answerTimerInputField);
            });
        }

        public void OnSettingsClick()
        {
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
        }

        public void OnInfoClick()
        {
            _infoPanel.SetActive(!_infoPanel.activeSelf);
        }

        private void RoundTimerInputField_OnTImeChanged(InputField input)
        {
            if (input.text != null)
            {
                if (int.Parse(input.text) == 0)
                {
                    RoundManager.Instance.IsLimited = false;
                }
                else
                {
                    RoundManager.Instance.RoundTimer = int.Parse(input.text);
                    RoundManager.Instance.IsLimited = true;
                }
            }
        }

        private void QuestionTimerInputField_OnNameChanged(InputField input)
        {
            if (input.text != null)
            {
                QuestionManager.Instance.QuestionTimer = int.Parse(input.text);
            }
        }

        private void AnswerTimerInputField_OnNameChanged(InputField input)
        {
            if (input.text != null)
            {
                QuestionManager.Instance.AnswerTimer = int.Parse(input.text);
            }
        }
    }
}

