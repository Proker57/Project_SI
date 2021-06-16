using UnityEngine;

namespace BOYAREngine.Game
{
    public class ShowAnswer : MonoBehaviour
    {
        private SiGameMobile _si;

        private void Start()
        {
            _si = SiGameMobile.Instance;
        }

        public void Show()
        {
            QuestionManager.Instance.ShowAnswer(_si.ThemeIndexCurrent, _si.QuestionIndexCurrent);
        }
    }

}

