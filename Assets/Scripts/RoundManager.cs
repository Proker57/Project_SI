using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance;

        [SerializeField] private Text _hostTimerDisplayText;
        [SerializeField] private GameObject _introGameObject;
        [SerializeField] private Text _roundNameText;

        public float RoundTimer;
        public bool IsLimited;

        private int _minutes;
        private int _seconds;

        private Coroutine _timerDisplayCoroutine;
        private Coroutine _timerCoroutine;

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

        public void StartRoundTimer()
        {
            if (IsLimited)
            {
                _minutes = (int) RoundTimer;

                if (_timerCoroutine == null)
                {
                    _timerCoroutine = StartCoroutine(TimerCoroutine());
                }

                if (_timerDisplayCoroutine == null)
                {
                    _timerDisplayCoroutine = StartCoroutine(TimerHostDisplayCoroutine());
                }
            }
        }

        private IEnumerator TimerCoroutine()
        {
            yield return new WaitForSeconds(RoundTimer * 60);

            if (GameManager.Instance.Round < GameManager.Instance.Rounds.Count - 1)
            {
                HostManager.Instance.Messages.NextRoundClientRpc();
            }
            else
            {
                // TODO: GameOver statistic
            }
            
            _timerCoroutine = null;
        }

        private IEnumerator TimerHostDisplayCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                _seconds--;

                if (_seconds < 0)
                {
                    _minutes--;

                    _seconds = 59;
                }

                _hostTimerDisplayText.text = $"{_minutes:00} : {_seconds:00}";
            }
        }

        public void StopTimer()
        {
            StopAllCoroutines();
            _timerCoroutine = null;
            _timerDisplayCoroutine = null;
        }

        public void ShowIntro(string roundName)
        {
            _roundNameText.text = roundName;
            _introGameObject.SetActive(true);

            Invoke(nameof(CloseIntro), 3f);
        }

        private void CloseIntro()
        {
            _introGameObject.SetActive(false);
            _roundNameText.text = "";
        }
    }
}

