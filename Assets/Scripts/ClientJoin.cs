using MLAPI;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Net
{
    public class ClientJoin : MonoBehaviour
    {
        [Header("States")]
        [SerializeField] private GameObject _mainStateGameObject;
        [SerializeField] private GameObject _gameStateGameObject;

        [Header("Input")]
        [SerializeField] private InputField _inputName;

        public void OnClient()
        {
            NetworkManager.Singleton.StartClient();

            GameManager.Instance.PlayersList.Add(NetworkManager.Singleton.LocalClientId);
        }
    }
}

