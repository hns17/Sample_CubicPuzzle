using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class StageUIPresenter :MonoBehaviour
    {
        [SerializeField] private Button btnExit;
        [SerializeField] private Button btnPause;

        [SerializeField] private GameObject infos;
        [SerializeField] private GameObject pfInfoItem;

        [Inject] private CTSManager ctsManager;

        private int timeScale = 1;

        private void Start()
        {
            btnExit.onClick.AddListener(() =>
            {
                ctsManager?.CancellationAll();
                SceneManager.LoadSceneAsync(0);
            });

            btnPause.onClick.AddListener(() =>
            {
                timeScale = timeScale == 0 ? 1 : 0;
                Time.timeScale = timeScale;
            });
        }
    }
}