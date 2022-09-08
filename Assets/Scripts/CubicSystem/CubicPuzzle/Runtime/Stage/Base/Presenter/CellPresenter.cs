using System.Collections;
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
        [SerializeField] private SpriteRenderer spriteView;
        [SerializeField] private ParticleSystem fxCellExplosion;

        [Header("[Sprite]")]
        [SerializeField] private Sprite hexSprite;
        [SerializeField] private Sprite squareSprite;

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

            this.Cell.CellStyleObservable.Subscribe(x =>
            {
                spriteView.sprite = null;
                if(x == CellStyle.HEX) {
                    spriteView.sprite = hexSprite;
                }
                else if(x == CellStyle.SQUARE) {
                    spriteView.sprite = squareSprite;
                }
            }).AddTo(this);

            //Cell 상태 변경시 상태에 맞게 GameObject 상태 변경
            this.Cell.CellStateObservable.Subscribe(x =>
            {
                if(x== CellState.EMPTY) {
                    spriteView.gameObject.SetActive(false);
                    fxCellExplosion.gameObject.SetActive(false);
                }
                else if(x== CellState.NORMAL) {
                    spriteView.gameObject.SetActive(true);
                }
                else if(x == CellState.DESTROYED) {
                    spriteView.gameObject.SetActive(false);
                    fxCellExplosion.gameObject.SetActive(true);
                }
            }).AddTo(this);
        }

        public class Factory :PlaceholderFactory<CellModel, Transform, CellPresenter>
        {

        }
    }
}

