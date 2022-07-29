using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Cell GameObject
     */
    public class CellPresenter :MonoBehaviour
    {
        public CellModel Cell { get; private set; }
        [SerializeField] private GameObject objView;
        [SerializeField] private ParticleSystem fxCellExplosion;

        [Inject]
        private void InjectDependencies(CellModel cellModel, Transform parent)
        {
            this.Cell = cellModel;
            this.transform.SetParent(parent);
        }

        private void OnEnable()
        {
            fxCellExplosion.gameObject.SetActive(false);
        }

        private void Start()
        {
            //Cell 위치정보 변경시 Gameobject 위치 변경
            this.Cell.PositionObservable.Subscribe(x => this.transform.localPosition = x).AddTo(this);
            this.Cell.CellTypeObservable.Subscribe(x =>
            {
                if(x == CellType.NONE) {
                    gameObject.SetActive(false);
                }
                else {
                    gameObject.SetActive(true);
                }
            }).AddTo(this);

            //Cell 상태 변경시 상태에 맞게 GameObject 상태 변경
            this.Cell.CellStateObservable.Subscribe(x =>
            {
                if(x== CellState.EMPTY) {
                    objView.SetActive(false);
                }
                else if(x== CellState.NORMAL) {
                    objView.SetActive(true);
                }
                else if(x == CellState.DESTROYED) {
                    StartCoroutine(CellExplosionEffect());
                }
            }).AddTo(this);
        }


        /**
         *  @brief  Cell 파괴 연출로 Board Clear시 사용 됨.
         */
        private IEnumerator CellExplosionEffect()
        {
            objView.SetActive(false);
            fxCellExplosion.gameObject.SetActive(true);

            while(fxCellExplosion.isPlaying) {
                yield return null;
            }

            Cell.SetCellState(CellState.EMPTY);
        }

        public class Factory :PlaceholderFactory<CellModel, Transform, CellPresenter>
        {

        }
    }
}

