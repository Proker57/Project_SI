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
        [Header("Cat")]
        [SerializeField] private GameObject _catPanel;
        [SerializeField] private GameObject _catHostDataPanel;
        [SerializeField] private Text _catThemeText;
        [SerializeField] private Text _catPriceText;
        [Header("Auction")]
        [SerializeField] private GameObject _auctionPanel;
        [SerializeField] private GameObject _auctionButtons;
        [SerializeField] private GameObject _auctionPointsPanel;

        [Header("Local Objects")]
        public GameObject AnswerButtonGameObject;

        [Header("Content")]
        public AudioSource AudioSource;
        [SerializeField] private Image _questionImage;

        [Header("Scenario")]
        [SerializeField] private Text _scenario;

        [Header("Info")]
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private Text _infoText;

        [Header("Answer")]
        [SerializeField] private Text _answer;
        [SerializeField] private Text _answerHost;
        [SerializeField] private Image _answerImage;
        [Space]
        // Timer
        public float QuestionTimer;
        public float AnswerTimer;

        [HideInInspector] public bool IsRightAnswer;
        public bool IsShowQuestion;

        // Base
        public NetworkVariable<string> NetQuestionType = new NetworkVariable<string>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private const string Cat = "cat";
        private const string Auction = "auction";
        private List<byte[]> _imageQuestionChunksList = new List<byte[]>(64);
        private List<byte[]> _imageAnswerChunksList = new List<byte[]>(64);
        private NetworkVariable<string> _netScenario = new NetworkVariable<string>(new NetworkVariableSettings {ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly});
        private NetworkVariable<string> _netAnswer = new NetworkVariable<string>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private NetworkVariable<bool> _netIsQuestionImage = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });
        private NetworkVariable<bool> _netIsAnswerImage = new NetworkVariable<bool>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });

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
            QuestionTimer = 10f;
            AnswerTimer = 5f;
        }

        // QUESTION HOST
        public void ShowQuestionHost(int themeIndex, int questionIndex)
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(true);
            _answerPanel.SetActive(false);
            _answerDecideHostPanel.SetActive(true);
            _netIsQuestionImage.Value = false;
            _netIsAnswerImage.Value = false;
            AudioSource.gameObject.SetActive(false);
            _questionImage.gameObject.SetActive(false);
            _answerImage.gameObject.SetActive(false);
            _catPanel.SetActive(false);
            IsShowQuestion = true;
            _scenario.text = null;
            AudioSource.clip = null;
            _questionImage.sprite = null;

            _answerHost.text = GameManager.Instance.Rounds[GameManager.Instance.Round].Themes[themeIndex].Questions[questionIndex].Answers[0];

            GameManager.Instance.ResetColorsClientRpc();
            GameManager.Instance.QuestionsLeft--;

            var round = GameManager.Instance.Round;
            GameManager.Instance.QuestionPrice = int.Parse(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Price);

            NetQuestionType.Value = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Type;

            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario != null )
            {
                _scenario.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario;
                _netScenario.Value = _scenario.text;
            }

            // Music
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsMusic)
            {
                AudioSource.gameObject.SetActive(true);

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

                if (GameManager.Instance.Players.Count > 0)
                {
                    StartCoroutine(SendQuestionChunks(themeIndex, questionIndex, round));
                }

                var tex = new Texture2D(2, 2);
                tex.LoadImage(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData);
                _questionImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                _questionImage.rectTransform.sizeDelta = new Vector2(_questionImage.sprite.texture.width, _questionImage.sprite.texture.height);
            }

            if (NetQuestionType.Value != null)
            {
                // Cat
                if (NetQuestionType.Value.Equals(Cat))
                {
                    _catPanel.SetActive(true);
                    _catHostDataPanel.SetActive(true);
                    _catThemeText.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].CatTheme;
                    _catPriceText.text = $"Цена: {GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].CatPrice}";

                    if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].CatPrice != null)
                    {
                        GameManager.Instance.QuestionPrice = int.Parse(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].CatPrice);
                    }
                }

                // Auction
                if (NetQuestionType.Value.Equals(Auction))
                {
                    _auctionPanel.SetActive(true);
                    _auctionButtons.SetActive(true);
                    _auctionPointsPanel.SetActive(true);
                }
            }
            else
            {
//                if (!GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsQuestionImage)
//                {
//                    StartCoroutine(AnswerCountdown());
//                }

                StartCoroutine(AnswerCountdown());
            }

            //StartCoroutine(AnswerCountdown());
        }

        private IEnumerator SendQuestionChunks(int themeIndex, int questionIndex, int round)
        {
            //var compressedData = NetDataUtils.CompressGZip(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData);
            //var chunks = NetDataUtils.SplitArrayToChunks(compressedData, 1200).ToList();
            var chunks = NetDataUtils.SplitArrayToChunks(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData, 1300).ToList();
            var isLastChunk = false;
            for (var i = 0; i < chunks.Count; i++)
            {
                Debug.Log(chunks[i].Length);
                if (i == chunks.Count - 1)
                {
                    isLastChunk = true;
                    // StartCoroutine(AnswerCountdown());
                    yield return null;
                }

                ReceiveQuestionImageChunkClientRpc(chunks[i], isLastChunk);
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator AnswerCountdown()
        {
            yield return new WaitForSeconds(QuestionTimer);

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

                //_imageQuestionChunksList = new List<byte[]>();
                //_imageAnswerChunksList = new List<byte[]>();

                _scenario.text = null;

                Invoke(nameof(WaitForQuestionClient), 0.1f);
            }
        }

        private void WaitForQuestionClient()
        {
            if (NetQuestionType.Value == null)
            {
                AnswerButtonGameObject.SetActive(true);
            }
            else
            {
                // Cat
                if (NetQuestionType.Value.Equals(Cat))
                {
                    _catPanel.SetActive(true);
                }

                // Auction
                if (NetQuestionType.Value.Equals(Auction))
                {
                    _auctionPanel.SetActive(true);
                }
            }

            _scenario.text = _netScenario.Value;

            AudioSource.gameObject.SetActive(false);
        }

        [ClientRpc]
        private void ReceiveQuestionImageChunkClientRpc(byte[] chunk, bool isLast)
        {
            if (IsHost == false)
            {
                _imageQuestionChunksList.Add(chunk);

                Debug.Log(chunk.Length);

                if (isLast)
                {
                    var result = _imageQuestionChunksList.SelectMany(x => x).ToArray();
                    //var decompressedData = NetDataUtils.DecompressGZip(result);
                    var tex = new Texture2D(2, 2);
                    //tex.LoadImage(decompressedData);
                    tex.LoadImage(result);
                    _questionImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
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
            IsShowQuestion = false;

            var round = GameManager.Instance.Round;
            _answer.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Answers[0];
            _netAnswer.Value = _answer.text;

            // Answer Image
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsAnswerImage)
            {
                _answerImage.gameObject.SetActive(true);
                _netIsAnswerImage.Value = true;

                if (GameManager.Instance.Players.Count > 0)
                {
                    StartCoroutine(SendAnswerChunks(themeIndex, questionIndex));
                }

                var tex = new Texture2D(2, 2);
                tex.LoadImage(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageAnswerData);
                _answerImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                _answerImage.rectTransform.sizeDelta = new Vector2(_answerImage.sprite.texture.width, _answerImage.sprite.texture.height);
            }

            if (NetQuestionType.Value != null)
            {
                if (NetQuestionType.Value.Equals(Cat) && !IsRightAnswer)
                {
                    GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value -= GameManager.Instance.QuestionPrice;
                }
            }

            IsRightAnswer = false;

            // Client
            ShowAnswerClientRpc();

            //            if (!GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsAnswerImage)
            //            {
            //                Invoke(nameof(BackToThemeClientRpc), AnswerTimer);
            //            }

            Invoke(nameof(BackToThemeClientRpc), AnswerTimer);
        }

        private IEnumerator SendAnswerChunks(int themeIndex, int questionIndex)
        {
            var round = GameManager.Instance.Round;
            //var compressedData = NetDataUtils.CompressGZip(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageAnswerData);
            //var chunks = NetDataUtils.SplitArrayToChunks(compressedData, 1300).ToList();
            var chunks = NetDataUtils.SplitArrayToChunks(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageAnswerData, 1300).ToList();
            var isLastChunk = false;
            for (var i = 0; i < chunks.Count; i++)
            {
                Debug.Log(chunks[i].Length);

                if (i == chunks.Count - 1)
                {
                    isLastChunk = true;
                    //Invoke(nameof(BackToThemeClientRpc), AnswerTimer);
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
                    //var decompressedData = NetDataUtils.DecompressGZip(result);
                    var tex = new Texture2D(2, 2);
                    //tex.LoadImage(decompressedData);
                    tex.LoadImage(result);
                    _answerImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    _answerImage.gameObject.SetActive(true);
                }
            }
        }

        [ClientRpc]
        private void ShowAnswerClientRpc()
        {
            _themePanel.SetActive(false);
            _questionPanel.SetActive(false);
            _answerPanel.SetActive(true);
            AnswerButtonGameObject.SetActive(false);
            _answer.text = null;

            Invoke(nameof(WaitForAnswerClient), 0.1f);
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
            _scenario.text = null;

            if (IsHost)
            {
                _netScenario.Value = null;
            }

            _answerImage.gameObject.SetActive(false);
            _questionImage.gameObject.SetActive(false);

            StopAllCoroutinesForClients();

            ChangeRound();
        }

        public void ChangeRound()
        {
            if (GameManager.Instance.QuestionsLeft == 0)
            {
                GameManager.Instance.Round++;
                GameManager.Instance.HostCreate.SetupHostRound();
            }
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

        public void CatQuestionContinue()
        {
            _catPanel.SetActive(false);
            TurnOffCatPanelClientRpc();

            StartCoroutine(AnswerCountdown());
        }

        [ClientRpc]
        private void TurnOffCatPanelClientRpc()
        {
            if (!IsHost)
            {
                _catPanel.SetActive(false);
            }
        }

        public void ShowInfo(bool isActive, string text)
        {
            _infoPanel.SetActive(isActive);

            _infoText.text = text;
        }
    }
}

