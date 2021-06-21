using MLAPI;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class QuestionButton : MonoBehaviour
    {
        public int ThemeIndex;
        public int QuestionIndex;

        private SiGameMobile _si;

        private void Start()
        {
            _si = SiGameMobile.Instance;
        }

        public void ShowQuestion()
        {
            QuestionManager.Instance.ShowQuestion(ThemeIndex, QuestionIndex);

            _si.ThemeIndexCurrent = ThemeIndex;
            _si.QuestionIndexCurrent = QuestionIndex;

            GetComponent<Button>().interactable = false;
            GetComponentInChildren<Text>().text = null;
        }
    }

}

