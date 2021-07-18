using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOYAREngine.Net;
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
        [SerializeField] private Image _questionImage;

        [Header("Scenario")]
        [SerializeField] private Text _scenario;

        [Header("Answer")]
        [SerializeField] private Text _answer;
        [SerializeField] private Text _answerHost;
        [SerializeField] private Image _answerImage;
        [SerializeField] private float _answerTimer;
        [SerializeField] private float _backToThemeTimer;

        // Base
        private NetworkVariable<string> _netScenario = new NetworkVariable<string>(new NetworkVariableSettings {ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly});
        private NetworkVariable<string> _netAnswer = new NetworkVariable<string>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });

        private NetworkVariable<bool> _netIsAudio = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private NetworkVariable<bool> _netIsQuestionImage = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private NetworkVariable<bool> _netIsAnswerImage = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private NetworkVariable<bool> _netIsMarker = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private List<byte[]> _imageQuestionChunksList = new List<byte[]>();
        private List<byte[]> _imageAnswerChunksList = new List<byte[]>(256);

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
            _netIsQuestionImage.Value = false;
            _netIsAnswerImage.Value = false;
            _netIsMarker.Value = false;

            AudioSource.gameObject.SetActive(false);
            _questionImage.gameObject.SetActive(false);
            _answerImage.gameObject.SetActive(false);

            _scenario.text = null;
            AudioSource.clip = null;
            _questionImage.sprite = null;

            _answerHost.text = GameManager.Instance.Rounds[GameManager.Instance.Round].Themes[themeIndex].Questions[questionIndex].Answers[0];

            GameManager.Instance.ResetColorsClientRpc();

            var round = GameManager.Instance.Round;
            GameManager.Instance.QuestionPrice = int.Parse(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Price);

            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario != null )
            {
                _scenario.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario;
                _netScenario.Value = _scenario.text;
            }

            // Marker
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsMarker)
            {
                _netIsMarker.Value = true;
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

                        AudioSource.clip = AudioClip.Create("Mp3_Name", samples.Length, mpeg.Channels, mpeg.SampleRate, false);
                        AudioSource.clip.SetData(samples, 0);
                    }
                }

                AudioSource.Play();
            }

            // Question Image
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsQuestionImage)
            {
                _questionImage.gameObject.SetActive(true);

                _netIsQuestionImage.Value = true;

                /*var compressedData = NetDataUtils.CompressGZip(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData);

                // TODO: Delete
                Debug.Log($"Raw data: {GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData.Length} || Compressed Data: {compressedData.Length}");

                var chunks = NetDataUtils.SplitArrayToChunks(compressedData, 1200).ToList();
                var isLastChunk = false;
                for (var i = 0; i < chunks.Count; i++)
                {
                    if (i == chunks.Count - 1)
                    {
                        isLastChunk = true;

                        Debug.Log("Host is done sending question image");
                    }

                    ReceiveQuestionImageChunkClientRpc(chunks[i], isLastChunk);
                }*/

                StartCoroutine(SendQuestionChunks(themeIndex, questionIndex, round));

                var tex = new Texture2D(2, 2);
                tex.LoadImage(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData);
                _questionImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                _questionImage.rectTransform.sizeDelta = new Vector2(_questionImage.sprite.texture.width, _questionImage.sprite.texture.height);
            }

            StartCoroutine(AnswerCountdown());
        }

        private IEnumerator SendQuestionChunks(int themeIndex, int questionIndex, int round)
        {
            var compressedData = NetDataUtils.CompressGZip(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData);

            // TODO: Delete
            Debug.Log($"Raw data: {GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData.Length} || Compressed Data: {compressedData.Length}");

            var chunks = NetDataUtils.SplitArrayToChunks(compressedData, 1200).ToList();
            var isLastChunk = false;
            for (var i = 0; i < chunks.Count; i++)
            {
                if (i == chunks.Count - 1)
                {
                    isLastChunk = true;
                    Debug.Log("Sending question image is done");
                    yield return null;
                }

                ReceiveQuestionImageChunkClientRpc(chunks[i], isLastChunk);
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator AnswerCountdown()
        {
            yield return new WaitForSeconds(_answerTimer);

            ShowAnswerHost(GameManager.Instance.ThemeIndexCurrent, GameManager.Instance.QuestionIndexCurrent);
        }

        public void ShowQuestionClient(int themeIndex, int questionIndex)
        {
            if (!IsHost)
            {
                _themePanel.SetActive(false);
                _questionPanel.SetActive(true);
                _answerPanel.SetActive(false);

                _answerImage.gameObject.SetActive(false);
                _questionImage.gameObject.SetActive(false);

                AnswerButtonGameObject.SetActive(true);

                _scenario.text = null;

                Invoke(nameof(WaitForQuestionClient), 0.3f);
            }
        }

        private void WaitForQuestionClient()
        {
            _scenario.text = _netScenario.Value;

            AudioSource.gameObject.SetActive(false);

            // Audio
//            if (_netIsAudio.Value)
//            {
//                AudioSource.gameObject.SetActive(true);
//            }
        }

        [ClientRpc]
        private void ReceiveQuestionImageChunkClientRpc(byte[] chunk, bool isLast)
        {
            if (IsHost == false)
            {
                _imageQuestionChunksList.Add(chunk);

                if (isLast)
                {
                    var result = _imageQuestionChunksList.SelectMany(x => x).ToArray();

                    var decompressedData = NetDataUtils.DecompressGZip(result);

                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(decompressedData);

                    _questionImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                    Debug.Log("Client received Question image");
                    _questionImage.gameObject.SetActive(true);
                }
            }
        }

        // ANSWER HOST
        public void ShowAnswerHost(int themeIndex, int questionIndex)
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(true);
            _answerDecideHostPanel.SetActive(false);
            _answerImage.gameObject.SetActive(_netIsAnswerImage.Value);

            var round = GameManager.Instance.Round;
            _answer.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Answers[0];
            _netAnswer.Value = _answer.text;

            // Answer Image
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsAnswerImage)
            {
                _answerImage.gameObject.SetActive(true);

                _netIsAnswerImage.Value = true;

                StartCoroutine(SendAnswerChunks(themeIndex, questionIndex));

                var tex = new Texture2D(2, 2);
                tex.LoadImage(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageAnswerData);
                _answerImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                _answerImage.rectTransform.sizeDelta = new Vector2(_answerImage.sprite.texture.width / 6f, _answerImage.sprite.texture.height / 6f);
            }

            // Client
            ShowAnswerClientRpc();

            Invoke(nameof(BackToThemeClientRpc), 5f);
        }

        private IEnumerator SendAnswerChunks(int themeIndex, int questionIndex)
        {
            var round = GameManager.Instance.Round;
            var compressedData = NetDataUtils.CompressGZip(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageAnswerData);

            // TODO: Delete
            Debug.Log($"Raw data: {GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageAnswerData.Length} || Compressed Data: {compressedData.Length}");

            var chunks = NetDataUtils.SplitArrayToChunks(compressedData, 1200).ToList();
            var isLastChunk = false;
            for (var i = 0; i < chunks.Count; i++)
            {
                if (i == chunks.Count - 1)
                {
                    isLastChunk = true;
                    Debug.Log("Sending answer image is done");
                    yield return null;
                }

                ReceiveAnswerImageChunkClientRpc(chunks[i], isLastChunk);
                yield return new WaitForEndOfFrame();
            }
        }

        [ClientRpc]
        private void ReceiveAnswerImageChunkClientRpc(byte[] chunk, bool isLast)
        {
            if (IsHost == false)
            {
                _imageAnswerChunksList.Add(chunk);

                Debug.Log(chunk.Length);

                if (isLast)
                {
                    var result = _imageAnswerChunksList.SelectMany(x => x).ToArray();

                    var decompressedData = NetDataUtils.DecompressGZip(result);

                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(decompressedData);
                    _answerImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                    //
                    _answerImage.gameObject.SetActive(true);

                    Debug.Log("Client received Answer image");
                }
            }
        }

        [ClientRpc]
        private void ShowAnswerClientRpc()
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(true);
            //_answerImage.gameObject.SetActive(_netIsAnswerImage.Value);

            AnswerButtonGameObject.SetActive(false);

            _answer.text = null;

            //_imageChunksList = new List<byte[]>();

            //_questionImage.sprite = null;

            Invoke(nameof(WaitForAnswerClient), 0.2f);
        }

        private void WaitForAnswerClient()
        {
            _answer.text = _netAnswer.Value;
        }

        [ClientRpc]
        public void BackToThemeClientRpc()
        {
            _themePanel.SetActive(true);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(false);

            _imageQuestionChunksList = new List<byte[]>();
            _imageAnswerChunksList = new List<byte[]>();

            _answerImage.gameObject.SetActive(false);
            _questionImage.gameObject.SetActive(false);

            StopAllCoroutinesForClients();
        }

        public void StopAllCoroutinesForClients()
        {
            StopAllCoroutinesForClientsServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopAllCoroutinesForClientsServerRpc()
        {
            StopAllCoroutinesForClientsClientRpc();
        }

        [ClientRpc]
        private void StopAllCoroutinesForClientsClientRpc()
        {
            StopAllCoroutines();
        }
    }
}

