using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class GameSettings : MonoBehaviour
    {
        [SerializeField] private GameObject _settingsPanel;

        [SerializeField] private InputField _questionTimerInputField;
        [SerializeField] private InputField _answerTimerInputField;

        private void Start()
        {
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

        public void OnClick()
        {
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
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

