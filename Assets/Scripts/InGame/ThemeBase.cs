using BOYAREngine.Game;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Net
{
    public class ThemeBase : MonoBehaviour
    {
        [SerializeField] private GameObject _infoPanel;

        public Text Name;
        public string Info;

        private void Start()
        {
            gameObject.transform.SetParent(GameObject.FindGameObjectWithTag("ThemeParent").transform, false);
        }

        public void TurnOnInfoPanel()
        {
            _infoPanel.SetActive(true);
        }

        public void OnPointerDown()
        {
            QuestionManager.Instance.ShowInfo(true, Info);
        }

        public void OnPointerUp()
        {
            QuestionManager.Instance.ShowInfo(false, null);
        }
    }
}

