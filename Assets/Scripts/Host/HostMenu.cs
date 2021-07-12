using UnityEngine;

namespace BOYAREngine.Game
{
    public class HostMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _menuGameObject;
        [SerializeField] private GameObject _answerPanel;

        public void OnMenuButtonClick()
        {
            _menuGameObject.SetActive(!_menuGameObject.activeSelf);
        }

        public void OnAnswerPanelClick()
        {
            _answerPanel.SetActive(!_answerPanel.activeSelf);
        }
    }
}

