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
        public float QuestionTimer;
        public float AnswerTimer;

        [HideInInspector] public bool IsRightAnswer;
        public bool IsShowQuestion;

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

        public void ShowQuestionHost(int themeIndex, int questionIndex)
        {
            ResetQuestionData();

            var round = GameManager.Instance.Round;
            _answerHost.text = GameManager.Instance.Rounds[GameManager.Instance.Round].Themes[themeIndex].Questions[questionIndex].Answers[0];

            HostManager.Instance.Messages.ResetColorsClientRpc();
            HostManager.Instance.Messages.SetQuestionPriceClientRpc(int.Parse(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Price));

            NetQuestionType.Value = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Type;

            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario != null )
            {
                _scenario.text = GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Scenario;
                _netScenario.Value = _scenario.text;
            }

            ReadMusicInQuestion(round, themeIndex, questionIndex);
            ReadImageInQuestion(round, themeIndex, questionIndex);

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
                        GameManager.Instance.QuestionPriceCurrent = int.Parse(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].CatPrice);
                    }
                }

                // Auction
                if (NetQuestionType.Value.Equals(Auction))
                {
                    HostManager.Instance.Messages.TurnOnAuctionPanelsClientRpc();
                    GameManager.Instance.GetComponent<Auction>().ResetValues();
                    _auctionPanel.SetActive(true);
                    //_auctionButtons.SetActive(true);
                    _auctionPointsPanel.SetActive(true);
                }
            }
            else
            {
                StartCoroutine(AnswerCountdown());
            }
        }

        private void ReadMusicInQuestion(int round, int themeIndex, int questionIndex)
        {
            if (GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].IsMusic)
            {
                AudioSource.gameObject.SetActive(true);

                using (var memoryStream = new MemoryStream(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].AudioData))
                {
                    using (var mpeg = new MpegFile(memoryStream))
                    {
                        var samples = new float[mpeg.Length];
                        mpeg.ReadSamples(samples, 0, samples.Length);

                        AudioSource.clip = AudioClip.Create("Mp3_Name", samples.Length, mpeg.Channels, mpeg.SampleRate, false);
                        AudioSource.clip.SetData(samples, 0);
                    }
                }

                if (NetQuestionType.Value == null)
                {
                    AudioSource.Play();
                }
            }
        }

        private void ReadImageInQuestion(int round, int themeIndex, int questionIndex)
        {
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
        }

        private IEnumerator SendQuestionChunks(int themeIndex, int questionIndex, int round)
        {
            var chunks = NetDataUtils.SplitArrayToChunks(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageQuestionData, 1300).ToList();
            var isLastChunk = false;
            for (var i = 0; i < chunks.Count; i++)
            {
                if (i == chunks.Count - 1)
                {
                    isLastChunk = true;
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
            if (!IsHost)
            {
                _imageQuestionChunksList.Add(chunk);

                if (isLast)
                {
                    var result = _imageQuestionChunksList.SelectMany(x => x).ToArray();
                    var tex = new Texture2D(2, 2);
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

            // Image
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
                // Cat
                if (NetQuestionType.Value.Equals(Cat) && !IsRightAnswer)
                {
                    GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value -= GameManager.Instance.QuestionPriceCurrent;
                }

                // Auction
                if (NetQuestionType.Value.Equals(Auction) && !IsRightAnswer)
                {
                    GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().Points.Value -= GameManager.Instance.Players[GameManager.Instance.ActivePlayer].GetComponent<PlayerData>().AuctionBet.Value;
                }
            }

            IsRightAnswer = false;

            ShowAnswerClientRpc();

            Invoke(nameof(BackToThemeClientRpc), AnswerTimer);
        }

        private IEnumerator SendAnswerChunks(int themeIndex, int questionIndex)
        {
            var round = GameManager.Instance.Round;
            var chunks = NetDataUtils.SplitArrayToChunks(GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].ImageAnswerData, 1300).ToList();
            var isLastChunk = false;
            for (var i = 0; i < chunks.Count; i++)
            {
                Debug.Log(chunks[i].Length);

                if (i == chunks.Count - 1)
                {
                    isLastChunk = true;
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
                    var tex = new Texture2D(2, 2);
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
            _auctionPanel.SetActive(false);
            _catPanel.SetActive(false);

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

            if (IsHost)
            {
                GameManager.Instance.QuestionsLeft--;

                if (GameManager.Instance.QuestionsLeft <= 0)
                {
                    HostManager.Instance.Messages.NextRoundClientRpc();
                }
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

            if (GameManager.Instance.Rounds[GameManager.Instance.Round].Themes[GameManager.Instance.ThemeIndexCurrent].Questions[GameManager.Instance.QuestionIndexCurrent].IsMusic)
            {
                AudioSource.Play();
            }

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

        public void AuctionQuestionContinue()
        {
            _auctionPanel.SetActive(false);
            TurnOffAuctionPanelClientRpc();

            if (GameManager.Instance.Rounds[GameManager.Instance.Round].Themes[GameManager.Instance.ThemeIndexCurrent].Questions[GameManager.Instance.QuestionIndexCurrent].IsMusic)
            {
                AudioSource.Play();
            }

            StartCoroutine(AnswerCountdown());
        }

        [ClientRpc]
        private void TurnOffAuctionPanelClientRpc()
        {
            if (!IsHost)
            {
                _auctionPanel.SetActive(false);
            }
            else
            {
                GameManager.Instance.GetComponent<Auction>().ResetValues();
            }
        }

        public void ShowInfo(bool isActive, string text)
        {
            _infoPanel.SetActive(isActive);

            _infoText.text = text;
        }

        private void ResetQuestionData()
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
        }
    }
}

