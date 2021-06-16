using BOYAREngine.Game;
using UnityEngine;

namespace BOYAREngine.Audio
{
    public class StopMusic : MonoBehaviour
    {
        public void Stop()
        {
            QuestionManager.Instance.Audio.GetComponent<AudioSource>().Stop();
        }
    }
}

