using UnityEngine;

namespace BOYAREngine.Game
{
    public class HostMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _menuGameObject;

        public void OnMenuButtonClick()
        {
            _menuGameObject.SetActive(!_menuGameObject.activeSelf);
        }
    }
}

