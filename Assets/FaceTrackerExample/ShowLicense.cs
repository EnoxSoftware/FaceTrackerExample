using UnityEngine;
using UnityEngine.SceneManagement;

namespace FaceTrackerExample
{
    /// <summary>
    /// Show License
    /// </summary>
    public class ShowLicense : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBackButton()
        {
            SceneManager.LoadScene("FaceTrackerExample");
        }
    }
}
