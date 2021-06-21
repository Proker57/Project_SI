using System.Collections;
using System.IO;
using BOYAREngine.Net;
using NLayer;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

namespace BOYAREngine.Game
{
    public class HostCreate : NetworkBehaviour
    {
        [Header("Parents")]
        [SerializeField] private GameObject _mainStateGameObject;
        [SerializeField] private GameObject _gameStateGameObject;
        [SerializeField] private Transform _themeParentGameObject;

        [Header("Game Prefabs")]
        [SerializeField] private GameObject _themePrefab;
        [SerializeField] private GameObject _questionPrefab;

        [Header("Player setup")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawnParent;

        // Sas
        [Header("Lists of round data")]
        //private NetworkVariable<byte> _netQuestionRowCount = new NetworkVariable<byte>();
        //private NetworkVariable<byte> _netQuestionColumnCount = new NetworkVariable<byte>();

        private AudioClip _audioClip;

        public void OnHost()
        {
            NetworkManager.Singleton.StartHost();

            SetupHostRound();

            SetGameStarted(true);
        }

        private void SetupHostRound()
        {
            // Check if Package is chosen
            if (GameManager.Instance.IsReadyToStart)
            {
                // Parse Package from folder
                SiGameMobile.Instance.ParsePackage();
                var round = SiGameMobile.Instance.Round;

                // TODO: Fix this
                GameManager.Instance.NetQuestionRowCount.Value = (byte)SiGameMobile.Instance.Rounds[round].Themes[0].Questions.Count;
                GameManager.Instance.NetQuestionColumnCount.Value = (byte) SiGameMobile.Instance.Rounds[round].Themes.Count;

                for (var i = 0; i < SiGameMobile.Instance.Rounds[round].Themes.Count; i++)
                {
                    // Spawn Theme Object on server
                    var theme = Instantiate(_themePrefab, _themeParentGameObject.transform);
                    theme.GetComponent<NetworkObject>().Spawn();
                    // Add Theme names to the game data list
                    GameManager.Instance.NetThemeNames.Add(SiGameMobile.Instance.Rounds[round].Themes[i].Name);
                    // Set name to Theme object
                    theme.GetComponentInChildren<Text>().text = SiGameMobile.Instance.Rounds[round].Themes[i].Name;

                    for (var j = 0; j < SiGameMobile.Instance.Rounds[round].Themes[i].Questions.Count; j++)
                    {
                        // Spawn Question Object on server
                        var question = Instantiate(_questionPrefab, theme.transform);
                        question.GetComponent<NetworkObject>().Spawn();
                        // Add Quesntion Price the the game data list
                        GameManager.Instance.NetQuestionPrice.Add(new Vector2(i, j), SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].Price);
                        // Set price to the object
                        question.GetComponentInChildren<Text>().text = SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].Price;
                        // Set indexes to the question
                        var questionButton = question.GetComponent<QuestionButton>();
                        questionButton.ThemeIndex = i;
                        questionButton.QuestionIndex = j;

                        // Audio
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
                                    //SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].Clip = _audioClip;
                                }

                            }
                        }

                        // Image
                        if (SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].ImageData != null)
                        {
                            var tex = new Texture2D(2, 2);
                            tex.LoadImage(SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].ImageData);
                            //SiGameMobile.Instance.Rounds[round].Themes[i].Questions[j].Image = tex;
                        }
                    }
                }

                SetGameStarted(true);
            }
        }

        private void OnClientJoins(ulong id)
        {
            if (IsHost)
            {
                SpawnPlayer(id);
            }

            if (NetworkManager.Singleton.LocalClientId == id)
            {
                FindThemes();
                StartCoroutine(FindQuestions());
                StartCoroutine(FindPlayers());

                SetGameStarted(true);
            }
        }

        private void FindThemes()
        {
            var themes = GameObject.FindGameObjectsWithTag("Theme");
            for (var i = 0; i < themes.Length; i++)
            {
                // Set parent
                themes[i].transform.SetParent(_themeParentGameObject, false);
                // Set name of theme
                themes[i].GetComponent<ThemeBase>().Name.text = GameManager.Instance.NetThemeNames[i];
            }
        }

        private IEnumerator FindQuestions()
        {
            yield return new WaitForSeconds(.5f);

            var questions = GameObject.FindGameObjectsWithTag("Question");
            Debug.Log(questions.Length);
            var index = 0;
            for (var i = 0; i < GameManager.Instance.NetQuestionColumnCount.Value; i++)
            {
                for (var j = 0; j < GameManager.Instance.NetQuestionRowCount.Value; j++)
                {
                    questions[index].GetComponent<QuestionBase>().Price.text = GameManager.Instance.NetQuestionPrice[new Vector2(i, j)];
                    index++;
                }
            }
        }

        private IEnumerator FindPlayers()
        {
            yield return new WaitForSeconds(.5f);
            // TODO: Don't work but why?
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                player.transform.SetParent(_playerSpawnParent);
            }
        }

        private void SetGameStarted(bool isGameStarted)
        {
            _mainStateGameObject.SetActive(!isGameStarted);
            _gameStateGameObject.SetActive(isGameStarted);
        }

        private void SpawnPlayer(ulong id)
        {
            var go = Instantiate(_playerPrefab, _playerSpawnParent);
            go.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoins;
        }

        private void OnDisable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientJoins;
        }
    }
}

