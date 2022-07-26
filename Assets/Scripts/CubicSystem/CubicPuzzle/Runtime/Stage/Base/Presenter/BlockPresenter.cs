using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Block GameObject(Block ���� �� �ı� ���� ǥ��)
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

            Initialize(true);
        }

        /**
         *  @brief  Block GameObject �ʱ�ȭ
         *  @param  isEnable : true(Ȱ��ȭ) / false(��Ȱ��ȭ)
         */
        private void Initialize(bool isEnable)
        {
            //if(this.objView.activeSelf != isEnable) {
            //    this.objView.SetActive(isEnable);
            //    this.colBlock.enabled = isEnable;
            //}

            //SetActive���� ȭ�� ������ �̵���Ű�°� ������...
            //Animator, collider, renderer ���� Ȱ��ȭ, ��Ȱ��ȭ�� �������� ����� ŭ
            //colBlock.enabled = isEnable;
            if(!isEnable) {
                objView.transform.localPosition = new Vector3(-2000f, -2000f, 0f);
            }
            else {
                objView.transform.localPosition = Vector3.zero;
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
            //Block �ı� ȿ��, �Ϸ�� Block GameObject ��Ȱ��ȭ
            shakeScale = objView.transform.DOShakeScale(BlockModel.ShakeScaleDuration, 0.8f, 5, 45)
                            .SetAutoKill(false)
                            .Pause()
                            .OnPause(() => {
                                DestoryBlock();
                            });
            
            //Caching Position Shake Dotween 
            //Block �̵� ȿ��
            shakePosition = objView.transform.DOShakePosition(BlockModel.ShakePositionDuration, 0.1f)
                                .SetAutoKill(false)
                                .SetLoops(-1)
                                .Pause()
                                .OnPause(()=> {
                                    objView.transform.localPosition = Vector3.zero;
                                });

            //Block ���� ���濡 ���� View ���� ����
            this.Block.ColorObservable.Subscribe(x =>
            {
                spriteRenderer.color = PuzzleBlockUtility.GetMatchColor(x);
            }).AddTo(this);
            
            //Block Position ����� Object Position ����
            this.Block.PositionObservable
                .Subscribe(x => this.transform.localPosition = x)
                .AddTo(this);

            //Block ���� ���濡 ���� View ���� ����
            this.Block.StateObservable.Subscribe(x => {
                Initialize(Block.IsEnableBlock() && x != BlockState.FILL_WAIT);
            }).AddTo(this);

            //��ġ ���� ȿ�� Ȱ��ȭ
            this.Block.ShakePositionObservable.Subscribe(x => {
                if(x) {
                    shakePosition.Restart();
                }
                else {
                    shakePosition.Pause();
                }
            }).AddTo(this); ;

            //ũ�� ���� ȿ�� Ȱ��ȭ
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
         *  @brief  Block �ı� ȿ��
         */
        public void DestoryBlock()
        {
            //���� View ��Ȱ��ȭ
            Initialize(false);

            //�� �ı� ��ƼŬ ��ġ, ���� ���� �� Ȱ��ȭ
            ParticleSystem.MainModule fxParticle = fxBlockExplosion.main;
            fxParticle.startColor = PuzzleBlockUtility.GetMatchColor(Block.Color);

            fxBlockExplosion.gameObject.SetActive(true);
        }

        public class Factory :PlaceholderFactory<BlockModel, Transform, BlockPresenter>
        {

        }
    }
}

