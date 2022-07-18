using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CubicSystem.CubicPuzzle
{
    public class StageUIPresenter :MonoBehaviour
    {
        [SerializeField] private Button btnExit;
        [SerializeField] private GameObject infos;
        [SerializeField] private GameObject pfInfoItem;


        private void Start()
        {
            btnExit.onClick.AddListener(() =>
            {
                SceneManager.LoadSceneAsync(0);
            });
        }
    }
}