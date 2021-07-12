using UnityEngine;

namespace BOYAREngine.Game
{
    public class ClientAnswerButton : MonoBehaviour
    {
        public void OnClick()
        {
            HostManager.Instance.Messages.SetActivePlayer(GameManager.Instance.NetId);

            QuestionManager.Instance.StopAllCoroutinesForClients();
        }
    }
}

