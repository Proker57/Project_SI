using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using NLayer;
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
        [SerializeField] private GameObject _answerDecideHostPanel;

        [Header("Local Objects")]
        public GameObject AnswerButtonGameObject;

        [Header("Content")]
        public AudioSource AudioSource;
        [SerializeField] private Image _image;

        [Header("Scenario")]
        [SerializeField] private Text _scenario;

        [Header("Answer")]
        [SerializeField] private Text _answer;
        [SerializeField] private Image _answerImage;
        [SerializeField] private float _answerTimer;
        [SerializeField] private float _backToThemeTimer;

        // Base
        private NetworkVariable<string> _netScenario = new NetworkVariable<string>(new NetworkVariableSettings {ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly});
        private NetworkVariable<string> _netAnswer = new NetworkVariable<string>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });

        // Audio
        //private byte[] _audioData;
        //private byte[] _newData;

        private NetworkVariable<bool> _netIsAudio = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private NetworkVariable<bool> _netIsImage = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private List<byte[]> _imageChunksList = new List<byte[]>();

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

        // QUESTION HOST
        public void ShowQuestionHost(int themeIndex, int questionIndex)
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(true);
            _answerPanel.SetActive(false);
            _answerDecideHostPanel.SetActive(true);
            _netIsAudio.Value = false;
            _netIsImage.Value = false;

            AudioSource.gameObject.SetActive(false);
            _image.gameObject.SetActive(false);

            _scenario.text = null;
            AudioSource.clip = null;
            _image.sprite = null;

            GameManager.Instance.ResetColorsClientRpc();

            var round = GameManager.Instance.Round;
            GameManager.Instance.QuestionPrice = int.Parse(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Price);

            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario != null )
            {
                _scenario.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario;
                _netScenario.Value = _scenario.text;
            }

            // Music
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsMusic)
            {
                AudioSource.gameObject.SetActive(true);
                _netIsAudio.Value = true;

                using (var ms = new MemoryStream(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].AudioData))
                {
                    using (var mpeg = new MpegFile(ms))
                    {
                        var samples = new float[mpeg.Length];
                        mpeg.ReadSamples(samples, 0, samples.Length);

                        AudioSource.clip = AudioClip.Create("Name", samples.Length, mpeg.Channels, mpeg.SampleRate, false);
                        AudioSource.clip.SetData(samples, 0);
                    }
                }

                AudioSource.Play();
            }

            // Image
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsImage)
            {
                _image.gameObject.SetActive(true);

                _netIsImage.Value = true;

                var chunks = SplitArrayToChunks(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageData, 1200).ToList();
                var isLastChunk = false;
                for (var i = 0; i < chunks.Count; i++)
                {
                    if (i == chunks.Count - 1)
                        isLastChunk = true;

                    ReceiveImageChunkClientRpc(chunks[i], isLastChunk);

                    Debug.Log(chunks[i].Length);
                }

                var tex = new Texture2D(2, 2);
                tex.LoadImage(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageData);
                //_image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                _answerImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            }

            StartCoroutine("AnswerCountdown");
            Invoke(nameof(AnswerCountdown), _answerTimer);
        }

        public void ShowQuestionClient(int themeIndex, int questionIndex)
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(true);
            _answerPanel.SetActive(false);

            if (!IsHost)
                AnswerButtonGameObject.SetActive(true);

            _scenario.text = null;

            Invoke(nameof(WaitForQuestionClient), 0.1f);
        }

        private void WaitForQuestionClient()
        {
            _scenario.text = _netScenario.Value;

            AudioSource.gameObject.SetActive(false);
            AudioSource.clip = null;
            _image.gameObject.SetActive(false);

            // Audio
            if (_netIsAudio.Value)
            {
                AudioSource.gameObject.SetActive(true);
            }

            // Image
            if (_netIsImage.Value)
            {
                _image.gameObject.SetActive(true);
            }
        }

        [ClientRpc]
        private void ReceiveImageChunkClientRpc(byte[] chunk, bool isLast)
        {
            if (!IsHost)
            {
                _imageChunksList.Add(chunk);

                if (isLast)
                {
                    // Do smth
                    Debug.Log(_imageChunksList.Count);
                    var result = _imageChunksList.SelectMany(x => x).ToArray();

                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(result);
                    _image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                }
            }
        }

        // ANSWER HOST
        public void ShowAnswerHost(int themeIndex, int questionIndex)
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(true);
            _answerImage.gameObject.SetActive(_netIsImage.Value);

            var round = GameManager.Instance.Round;
            _answer.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Answers[0];
            _netAnswer.Value = _answer.text;

            // Client
            ShowAnswerClientRpc();
        }

        [ClientRpc]
        private void ShowAnswerClientRpc()
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(true);

            AnswerButtonGameObject.SetActive(false);

            _answer.text = null;

            _imageChunksList = new List<byte[]>();
            _image.sprite = null;

            Invoke(nameof(WaitForAnswerClient), 0.1f);
        }

        private void WaitForAnswerClient()
        {
            _answer.text = _netAnswer.Value;
        }

        private IEnumerable AnswerCountdown()
        {
            yield return new WaitForSeconds(_answerTimer);

            ShowAnswerHost(GameManager.Instance.ThemeIndexCurrent, GameManager.Instance.QuestionIndexCurrent);
        }

        [ClientRpc]
        public void BackToThemeClientRpc()
        {
            _themePanel.SetActive(true);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(false);

            _answerImage.gameObject.SetActive(_netIsImage.Value);

            StopAllCoroutines();
        }






        public static byte[] Compress(byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static IEnumerable<byte[]> SplitArrayToChunks(byte[] value, int bufferLength)
        {
            var countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
                countOfArray++;
            for (var i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();

            }
        }
    }
}

