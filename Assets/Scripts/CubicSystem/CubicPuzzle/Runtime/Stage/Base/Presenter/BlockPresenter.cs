using UnityEngine;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Block GameObject(Block 색상 및 파괴 등을 표현)
     */
    public class BlockPresenter :MonoBehaviour
    {   
        public BlockModel Block { get; private set; }

        //Block View Object
        [SerializeField] private GameObject objView;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private PolygonCollider2D colBlock;

        //Block Destroy Particle
        [SerializeField] private ParticleSystem fxBlockExplosion;



        //Block Shaek Effect
        private Tween shakeScale;
        private Tween shakePosition;


        [Inject]
        private void InjectDependencies(BlockModel blockModel, Transform parent)
        {
            this.Block = blockModel;
            this.transform.SetParent(parent);
        }

        /**
         *  @brief  Block GameObject 초기화
         *  @param  isEnable : true(활성화) / false(비활성화)
         */
        private void Initialize(bool isEnable)
        {
            if(!isEnable) {
                transform.localPosition = new Vector3(-2000f, -2000f, 0f);
            }
            else {
                transform.localPosition = Block.Position;
            }

            objView.transform.localScale = Vector3.one;
            if(!fxBlockExplosion.isPlaying) {
                fxBlockExplosion.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            shakeScale.Kill();
            shakePosition.Kill();
        }


        private void Start()
        {
            //Caching Scale Shake Dotween
            //Block 파괴 효과, 완료시 Block GameObject 비활성화
            shakeScale = objView.transform.DOShakeScale(BlockModel.ShakeScaleDuration, 0.8f, 5, 45)
                            .SetAutoKill(false)
                            .Pause();
            
            //Caching Position Shake Dotween 
            //Block 이동 효과
            shakePosition = objView.transform.DOShakePosition(BlockModel.ShakePositionDuration, 0.2f)
                                .SetAutoKill(false)
                                .SetLoops(-1)
                                .Pause()
                                .OnPause(()=> {
                                    objView.transform.localPosition = Vector3.zero;
                                });

            //Block 색상 변경에 따른 View 색상 변경
            this.Block.ColorObservable.Subscribe(x =>
            {
                spriteRenderer.color = CubicPuzzleUtility.GetMatchColor(x);
            }).AddTo(this);
            
            //Block Position 변경시 Object Position 변경
            this.Block.PositionObservable
                .Subscribe(x => this.transform.localPosition = x)
                .AddTo(this);

            //Block Type 변경에 맞춰 Object 상태 변경
            this.Block.BlockTypeObservable.Subscribe(x => {
                if(x == BlockType.NONE) {
                    gameObject.SetActive(false);
                }
                else {
                    gameObject.SetActive(true);
                    Initialize(!Block.IsCompareState(BlockState.EMPTY));
                }
            }).AddTo(this);

            //Block 상태 변경에 맞춰 View 상태 변경
            this.Block.StateObservable.Subscribe(x => {
                if(x == BlockState.DESTROYED) {
                    DestoryBlock();
                }
                else if(!Block.IsCompareType(BlockType.NONE)) {
                    Initialize(!Block.IsCompareState(BlockState.EMPTY) && x != BlockState.FILL_WAIT);
                }
            }).AddTo(this);

            //위치 흔들기 효과 활성화
            this.Block.ShakePositionObservable.Subscribe(x => {
                if(x) {
                    shakePosition.Restart();
                }
                else {
                    shakePosition.Pause();
                }
            }).AddTo(this); ;

            //크기 흔들기 효과 활성화
            this.Block.ShakeScaleObservable.Subscribe(x => {
                if(x) {
                    shakeScale.Restart();
                }
                else {
                    shakeScale.Pause();
                }
            }).AddTo(this); ;
        }

        /**
         *  @brief  Block 파괴 효과
         */
        public void DestoryBlock()
        {
            //블럭의 View 비활성화
            Initialize(false);

            //블럭 파괴 파티클 위치, 색상 지정 후 활성화
            ParticleSystem.MainModule fxParticle = fxBlockExplosion.main;
            fxParticle.startColor = CubicPuzzleUtility.GetMatchColor(Block.Color);

            fxBlockExplosion.gameObject.SetActive(true);
        }

        public class Factory :PlaceholderFactory<BlockModel, Transform, BlockPresenter>
        {

        }
    }
}

