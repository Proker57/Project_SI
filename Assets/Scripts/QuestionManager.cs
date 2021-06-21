using System.Collections;
using BOYAREngine.Audio;
using MLAPI;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class QuestionManager : NetworkBehaviour
    {
        public static QuestionManager Instance;

        [Header("Panels")]
        [SerializeField] private GameObject _themePanel;
        [SerializeField] private GameObject _questionPanel;
        [SerializeField] private GameObject _answerPanel;

        [Header("Content")]
        public GameObject Audio;
        [SerializeField] private Image _image;

        [Header("Scenario")]
        [SerializeField] private Text _scenario;

        [Header("Answer")]
        [SerializeField] private Text _answer;
        [SerializeField] private float _answerTimer;
        [SerializeField] private float _backToThemeTimer;

        private SiGameMobile _si;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            _si = SiGameMobile.Instance;
        }

        public void ShowQuestion(int themeIndex, int questionIndex)
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(true);
            _answerPanel.SetActive(false);

            _scenario.text = null;
            Audio.GetComponent<AudioSource>().clip = null;

            var round = _si.Round;
            if (_si.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario != null )
            {
                _scenario.text = _si.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario;
            }

            if (_si.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsMusic)
            {
                Audio.SetActive(true);

                //Audio.GetComponent<AudioSource>().clip = _si.Rounds[round].Themes[themeIndex].Questions[questionIndex].Clip;
                Audio.GetComponent<AudioSource>().Play();
            }
            else
            {
                Audio.GetComponent<AudioSource>().clip = null;
                Audio.SetActive(false);
            }

            if (_si.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsImage)
            {
                _image.gameObject.SetActive(true);

                //var img = _si.Rounds[round].Themes[themeIndex].Questions[questionIndex].Image;
                //var tex = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(.5f, .5f));
                //_image.sprite = tex;
            }
            else
            {
                _image.sprite = null;
                _image.gameObject.SetActive(false);
            }

            StartCoroutine(AnswerCountdown());
        }

        public void ShowAnswer(int themeIndex, int questionIndex)
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(true);

            var round = _si.Round;
            _answer.text = _si.Rounds[round].Themes[themeIndex].Questions[questionIndex].Answers[0];

            StartCoroutine(ThemeCountdown());
        }

        private IEnumerator AnswerCountdown()
        {
            yield return new WaitForSeconds(_answerTimer);

            ShowAnswer(_si.ThemeIndexCurrent, _si.QuestionIndexCurrent);
        }

        private IEnumerator ThemeCountdown()
        {
            yield return new WaitForSeconds(_backToThemeTimer);

            _themePanel.SetActive(true);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(false);
        }
    }
}

