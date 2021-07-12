using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Net
{
    public class ThemeBase : MonoBehaviour
    {
        public Text Name;

        private void Start()
        {
            gameObject.transform.SetParent(GameObject.FindGameObjectWithTag("ThemeParent").transform, false);
        }
    }
}

