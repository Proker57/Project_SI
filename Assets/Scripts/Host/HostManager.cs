using System.Collections;
using BOYAREngine.Net;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class HostManager : NetworkBehaviour
    {
        public static HostManager Instance;

        [HideInInspector] public HostMessages Messages;

        [Header("Links")]
        [SerializeField] private GameObject _mainStateGameObject;
        [SerializeField] private GameObject _gameStateGameObject;
        [SerializeField] private Transform _themeParentGameObject;
        [Space]
        [SerializeField] private GameObject _themePrefab;
        [SerializeField] private NetworkObject _questionPrefab;
        [Space]
        [SerializeField] private HostCreate _hostCreate;

        [Header("Player Data")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawnParent;
        [SerializeField] private Transform _hostSpawnParent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Messages = GetComponent<HostMessages>();
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnDisable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        private void OnClientConnected(ulong id)
        {
            if (IsServer)
            {
                SpawnClientPlayer(id);
                SyncDataOnClientConnection(id);
            }

            if (NetworkManager.Singleton.LocalClientId == id)
            {
                StartCoroutine(RenamePlayer(id));
                GameManager.Instance.NetId = id;
                SetGameStarted(true);
            }
        }

        public void SetupHostRound()
        {
            if (GameManager.Instance.IsPacketChosen)
            {
                SetGameStarted(true);

                if (GameManager.Instance.Round > 0)
                {
                    DeleteThemes();
                }
                else
                {
                    GameManager.Instance.ParsePackage();
                }

                CreateThemes(GameManager.Instance.Round);
            }

            Debug.Log("SetupHostRound");
        }

        private void CreateThemes(int roundIndex)
        {
            for (var i = 0; i < GameManager.Instance.Rounds[roundIndex].Themes.Count; i++)
            {
                var theme = Instantiate(_themePrefab, _themeParentGameObject.transform);
                theme.GetComponent<NetworkObject>().Spawn();
                GameManager.Instance.ThemeNames.Add(GameManager.Instance.Rounds[roundIndex].Themes[i].Name);
                theme.GetComponentInChildren<ThemeBase>().Name.text = GameManager.Instance.Rounds[roundIndex].Themes[i].Name;
                if (GameManager.Instance.Rounds[roundIndex].Themes[i].Info != null)
                {
                    theme.GetComponentInChildren<ThemeBase>().Info = GameManager.Instance.Rounds[roundIndex].Themes[i].Info;
                    theme.GetComponentInChildren<ThemeBase>().TurnOnInfoPanel();
                }

                CreateQuestion(roundIndex, i, theme);
            }
        }

        private void CreateQuestion(int roundIndex, int themeIndex, GameObject themeParent)
        {
            for (var questionIndex = 0; questionIndex < GameManager.Instance.Rounds[roundIndex].Themes[themeIndex].Questions.Count; questionIndex++)
            {
                var question = Instantiate(_questionPrefab, themeParent.transform);
                question.GetComponent<NetworkObject>().Spawn();
                GameManager.Instance.QuestionButtonsList.Add(question);
                GameManager.Instance.QuestionsLeft = GameManager.Instance.QuestionButtonsList.Count;
                GameManager.Instance.QuestionPrice.Add(new Vector2(themeIndex, questionIndex), GameManager.Instance.Rounds[roundIndex].Themes[themeIndex].Questions[questionIndex].Price);
                question.GetComponentInChildren<Text>().text = GameManager.Instance.Rounds[roundIndex].Themes[themeIndex].Questions[questionIndex].Price;
                var questionButton = question.GetComponent<QuestionButton>();
                questionButton.ThemeIndex = themeIndex;
                questionButton.QuestionIndex = questionIndex;
            }
        }

        private void DeleteThemes()
        {
            if (IsHost)
            {
                foreach (Transform child in _themeParentGameObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void SetGameStarted(bool isGameStarted)
        {
            _mainStateGameObject.SetActive(!isGameStarted);
            _gameStateGameObject.SetActive(isGameStarted);
        }

        private void SyncDataOnClientConnection(ulong id)
        {
            SendThemeNames(id);
            SendQuestionPrices(id);
            SendRoundName(id, GameManager.Instance.Rounds[GameManager.Instance.Round].Name);
        }

        public void SendThemeNames()
        {
            for (var i = 0; i < GameManager.Instance.ThemeNames.Count; i++)
            {
                SendThemeNamesClientRpc(GameManager.Instance.ThemeNames[i]);
            }

            SetThemeNameClientRpc();
        }

        public void SendThemeNames(ulong id)
        {
            for (var i = 0; i < GameManager.Instance.ThemeNames.Count; i++)
            {
                SendThemeNamesWithIdClientRpc(id, GameManager.Instance.ThemeNames[i]);
            }

            SetThemeNameWithIdClientRpc(id);
        }

        [ClientRpc]
        public void SendThemeNamesClientRpc(string themeName)
        {
            if (!IsHost)
            {
                GameManager.Instance.ThemeNames.Add(themeName);
            }
        }

        [ClientRpc]
        public void SendThemeNamesWithIdClientRpc(ulong id, string themeName)
        {
            if (!IsHost && NetworkManager.Singleton.LocalClientId == id)
            {
                GameManager.Instance.ThemeNames.Add(themeName);
            }
        }

        [ClientRpc]
        public void SetThemeNameClientRpc()
        {
            if (!IsHost)
            {
                SetThemeName();
            }
        }

        [ClientRpc]
        public void SetThemeNameWithIdClientRpc(ulong id)
        {
            if (!IsHost && NetworkManager.Singleton.LocalClientId == id)
            {
                SetThemeName();
            }
        }

        public void SetThemeName()
        {
            if (!IsHost)
            {
                var themes = GameObject.FindGameObjectsWithTag("Theme");

                for (var i = 0; i < themes.Length; i++)
                {
                    themes[i].GetComponent<ThemeBase>().Name.text = GameManager.Instance.ThemeNames[i];
                }
            }
        }

        public void SendQuestionPrices()
        {
            var round = GameManager.Instance.Round;

            for (var themeIndex = 0; themeIndex < GameManager.Instance.Rounds[round].Themes.Count; themeIndex++)
            {
                for (var questionIndex = 0; questionIndex < GameManager.Instance.Rounds[round].Themes[themeIndex].Questions.Count; questionIndex++)
                {
                    SendQuestionPriceClientRpc(themeIndex, questionIndex, GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Price);
                }
            }

            SetQuestionPricesClientRpc();
        }

        public void SendQuestionPrices(ulong id)
        {
            var round = GameManager.Instance.Round;

            for (var themeIndex = 0; themeIndex < GameManager.Instance.Rounds[round].Themes.Count; themeIndex++)
            {
                for (var questionIndex = 0; questionIndex < GameManager.Instance.Rounds[round].Themes[themeIndex].Questions.Count; questionIndex++)
                {
                    SendQuestionPriceWithIdClientRpc(id, themeIndex, questionIndex, GameManager.Instance.Rounds[round].Themes[themeIndex].Questions[questionIndex].Price);
                }
            }

            SetQuestionPricesWithIdClientRpc(id);
        }

        [ClientRpc]
        public void SendQuestionPriceClientRpc(int themeIndex, int questionIndex, string price)
        {
            if (!IsHost)
            {
                GameManager.Instance.QuestionPrice.Add(new Vector2(themeIndex, questionIndex), price);
            }
        }

        [ClientRpc]
        public void SendQuestionPriceWithIdClientRpc(ulong id, int themeIndex, int questionIndex, string price)
        {
            if (!IsHost && NetworkManager.Singleton.LocalClientId == id)
            {
                GameManager.Instance.QuestionPrice.Add(new Vector2(themeIndex, questionIndex), price);
            }
        }

        [ClientRpc]
        public void SetQuestionPricesClientRpc()
        {
            if (!IsHost)
            {
                var questions = GameObject.FindGameObjectsWithTag("Question");
                var index = 0;

                foreach (var price in GameManager.Instance.QuestionPrice)
                {
                    questions[index].GetComponent<QuestionBase>().Price.text = price.Value;
                    index++;
                }
            }
        }

        [ClientRpc]
        public void SetQuestionPricesWithIdClientRpc(ulong id)
        {
            if (!IsHost && NetworkManager.Singleton.LocalClientId == id)
            {
                var questions = GameObject.FindGameObjectsWithTag("Question");
                var index = 0;

                foreach (var price in GameManager.Instance.QuestionPrice)
                {
                    questions[index].GetComponent<QuestionBase>().Price.text = price.Value;
                    index++;
                }
            }
        }

        public void SendRoundName(string roundName)
        {
            SendRoundNameClientRpc(roundName);
        }

        private void SendRoundName(ulong id, string roundName)
        {
            SendRoundNameWithIdClientRpc(id, roundName);
        }

        [ClientRpc]
        private void SendRoundNameClientRpc(string roundName)
        {
            if (!IsHost)
            {
                RoundManager.Instance.ShowIntro(roundName);
            }
        }

        [ClientRpc]
        private void SendRoundNameWithIdClientRpc(ulong id, string roundName)
        {
            if (!IsHost && NetworkManager.Singleton.LocalClientId == id)
            {
                RoundManager.Instance.ShowIntro(roundName);
            }
        }

        private void SpawnClientPlayer(ulong id)
        {
            var go = Instantiate(_playerPrefab, _playerSpawnParent);
            go.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
        }

        private IEnumerator RenamePlayer(ulong id)
        {
            yield return new WaitForSeconds(.2f);

            var goList = GameObject.FindGameObjectsWithTag("Player");
            for (var i = 0; i < goList.Length; i++)
            {
                if (goList[i].GetComponent<NetworkObject>().OwnerClientId == id)
                {
                    goList[i].GetComponent<PlayerData>().Name.Value = GameManager.Instance.Name;
                }
            }
        }
    }
}

