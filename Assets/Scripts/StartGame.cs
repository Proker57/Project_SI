using System.IO;
using NLayer;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class StartGame : MonoBehaviour
    {
        [SerializeField] private GameObject _loginGameObject;
        [SerializeField] private GameObject _gameGameObject;

        [SerializeField] private GameObject _parentGameObject;

        [SerializeField] private GameObject _themePrefab;
        [SerializeField] private GameObject _questionPrefab;

        private AudioClip _audioClip;

        public void Init()
        {
            if (GameManager.Instance.IsReadyToStart)
            {
                SiGameMobile.Instance.StartGame();
                var round = SiGameMobile.Instance.Round;

                for (var i = 0; i < SiGameMobile.Instance.Rounds[round].Themes.Count; i++)
                {
                    var theme = Instantiate(_themePrefab, _parentGameObject.transform);
                    theme.GetComponentInChildren<Text>().text = SiGameMobile.Instance.Rounds[round].Themes[i].Name;

                    for (var j = 0; j < SiGameMobile.Instance.Rounds[round].Themes[i].Questions.Count; j++)
                    {
                        var question = Instantiate(_questionPrefab, theme.transform);
                        question.GetComponentInChildren<Text>().text = SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].Price;
                        var questionButton = question.GetComponent<QuestionButton>();
                        questionButton.ThemeIndex = i;
                        questionButton.QuestionIndex = j;

                        if (SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].AudioData != null)
                        {
                            using (var ms = new MemoryStream(SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].AudioData))
                            {
                                //var mpeg = new MpegFile(ms);
                                using (var mpeg = new MpegFile(ms))
                                {
                                    var samples = new float[mpeg.Length];
                                    mpeg.ReadSamples(samples, 0, samples.Length);

                                    _audioClip = AudioClip.Create("Name", samples.Length, mpeg.Channels, mpeg.SampleRate, false);
                                    _audioClip.SetData(samples, 0);
                                    SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].Clip = _audioClip;
                                }
                                
                            }
                        }
                    }
                }

                SetGameStarted(true);
            }

            SetGameStarted(true);
        }

        private void SetGameStarted(bool isGameStarted)
        {
            _loginGameObject.SetActive(!isGameStarted);
            _gameGameObject.SetActive(isGameStarted);
        }


    }
}

