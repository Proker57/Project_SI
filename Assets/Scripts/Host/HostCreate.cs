using System.Collections;
using BOYAREngine.Net;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.NetworkVariable.Collections;

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
        [SerializeField] private NetworkObject _questionPrefab;

        [Header("Host menu")]
        [SerializeField] private GameObject _hostMenuGameObject;

        [Header("Player setup")]
        [SerializeField] private GameObject _hostPrefab;
        [SerializeField] private Transform _hostSpawnParent;

        private AudioClip _audioClip;

        public void OnHost()
        {
            NetworkManager.Singleton.StartHost();

            _hostMenuGameObject.SetActive(true);
            SetupHostRound();
            SpawnHost(NetworkManager.Singleton.ServerClientId);
            SetGameStarted(true);
        }

        public void SetupHostRound()
        {
            // Check if Package is chosen
            if (GameManager.Instance.IsReadyToStart)
            {
                // Change State
                SetGameStarted(true);

                if (GameManager.Instance.Round > 0)
                {
                    DeleteThemes();
                }

                // Parse Package from folder
                GameManager.Instance.ParsePackage();
                var round = GameManager.Instance.Round;

                // TODO: Fix this
                GameManager.Instance.NetQuestionRowCount.Value = (byte)GameManager.Instance.Rounds[round].Themes[0].Questions.Count;
                GameManager.Instance.NetQuestionColumnCount.Value = (byte)GameManager.Instance.Rounds[round].Themes.Count;

                for (var i = 0; i < GameManager.Instance.Rounds[round].Themes.Count; i++)
                {
                    // Spawn Theme Object on server
                    var theme = Instantiate(_themePrefab, _themeParentGameObject.transform);
                    theme.GetComponent<NetworkObject>().Spawn();
                    // Add Theme names to the game data list
                    GameManager.Instance.NetThemeNames.Add(GameManager.Instance.Rounds[round].Themes[i].Name);
                    // Set name to Theme object
                    theme.GetComponentInChildren<ThemeBase>().Name.text = GameManager.Instance.Rounds[round].Themes[i].Name;
                    if (GameManager.Instance.Rounds[round].Themes[i].Info != null)
                    {
                        theme.GetComponentInChildren<ThemeBase>().Info = GameManager.Instance.Rounds[round].Themes[i].Info;
                        theme.GetComponentInChildren<ThemeBase>().TurnOnInfoPanel();
                    }

                    for (var j = 0; j < GameManager.Instance.Rounds[round].Themes[i].Questions.Count; j++)
                    {
                        // Spawn Question Object on server
                        var question = Instantiate(_questionPrefab, theme.transform);
                        question.GetComponent<NetworkObject>().Spawn();
                        GameManager.Instance.QuestionButtonsList.Add(question);
                        GameManager.Instance.QuestionsLeft = GameManager.Instance.QuestionButtonsList.Count;
                        // Add Question Price the the game data list
                        GameManager.Instance.NetQuestionPrice.Add(new Vector2(i, j), GameManager.Instance.Rounds[round].Themes[i].Questions[j].Price);
                        // Set price to the object
                        question.GetComponentInChildren<Text>().text = GameManager.Instance.Rounds[round].Themes[i].Questions[j].Price;
                        // Set indexes to the question
                        var questionButton = question.GetComponent<QuestionButton>();
                        questionButton.ThemeIndex = i;
                        questionButton.QuestionIndex = j;
                    }
                }
            }
        }

        public void SetThemeName()
        {
            var themes = GameObject.FindGameObjectsWithTag("Theme");

            Debug.Log(themes.Length);

            for (var i = 0; i < themes.Length; i++)
            {
                // Set name of theme
                themes[i].GetComponent<ThemeBase>().Name.text = GameManager.Instance.NetThemeNames[i];
            }
        }

        public IEnumerator SetQuestionPrice()
        {
            yield return new WaitForSeconds(.1f);

            var questions = GameObject.FindGameObjectsWithTag("Question");
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

        public void SetGameStarted(bool isGameStarted)
        {
            _mainStateGameObject.SetActive(!isGameStarted);
            _gameStateGameObject.SetActive(isGameStarted);
        }

        private void SpawnHost(ulong id)
        {
            var go = Instantiate(_hostPrefab, _hostSpawnParent);
            go.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
            go.gameObject.GetComponent<HostData>().Name.Value = GameManager.Instance.Name;
        }
    }
}

