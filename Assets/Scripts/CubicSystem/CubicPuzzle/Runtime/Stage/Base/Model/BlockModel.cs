
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Block 상태 및 정보 관리
     */
    public class BlockModel
    {
        //Block Index
        public int Idx { get; private set; }

        //이동(Drop) 가능한 블럭인가?
        public bool IsLocking { get; private set; }

        //블럭의 위치, IObservable, Value Getter 공개
        private ReactiveProperty<Vector2> position = new ReactiveProperty<Vector2>(Vector2.zero);
        public Vector2 Position => position.Value;
        public IObservable<Vector2> PositionObservable => position;


        //블럭의 색상, IObservable, Value Getter 공개
        private ReactiveProperty<MatchColorType> color = new ReactiveProperty<MatchColorType>(MatchColorType.RED);
        public MatchColorType Color => color.Value;
        public IObservable<MatchColorType> ColorObservable => color;


        //블럭의 현재 상태, IObservable, Value Getter 공개
        private ReactiveProperty<BlockState> state = new ReactiveProperty<BlockState>(BlockState.NORMAL);
        public BlockState State => state.Value;
        public IObservable<BlockState> StateObservable => state;

        //Block 위치 흔들기, IObservable 공개
        private ReactiveProperty<bool> isShakePosition = new ReactiveProperty<bool>(false);
        public IObservable<bool> ShakePositionObservable => isShakePosition;

        //Block 크기 흔들기, IObservable 공개
        private ReactiveProperty<bool> isShakeScale = new ReactiveProperty<bool>(false);
        public IObservable<bool> ShakeScaleObservable => isShakeScale;

        //Shake Duration
        public const float ShakeScaleDuration = 1.2f;
        public const float ShakePositionDuration = 0.2f;


        [Inject]
        private BlockModel(BoardItemData itemData, Transform parent, Vector2 position, BlockPresenter.Factory factory) {
            
            //Make Block Gameobject
            factory.Create(this, parent);

            Initialize(itemData, position);
        }

        public void Initialize(BoardItemData itemData, Vector2 position)
        {
            Initialize(itemData.color, position);
        }

        /**
         *  @brief  Block 정보 초기화
         *  @param  colorType : MatchColor, position : 위치
         */
        public void Initialize(MatchColorType colorType, Vector2 position)
        {
            this.IsLocking = false;
            this.position.Value = position;
            this.state.Value = BlockState.NORMAL;
            this.color.Value = colorType;
        }

        /**
         *  @brief  Block 정보 초기화, FillRate를 이용해 확률적으로 블럭을 생성
         *  @param  blockFillRate : 블럭의 색상별 생성 확률, position : 위치
         */
        public void Initialize(Dictionary<MatchColorType, float> blockFillRate, Vector2 position)
        {
            MatchColorType colorType = MatchColorType.NONE;

            //확률의 최대치 구하기
            float maxValue =0f;
            foreach(var rateValue in blockFillRate.Values) {
                maxValue += rateValue;
            }


            //Rnd 값 생성
            var rndRate = UnityEngine.Random.Range(0, maxValue);

            //색상 선택하기
            float calcRate = 0f;
            foreach(var rateItem in blockFillRate) {
                calcRate += rateItem.Value;
                if(calcRate >= rndRate) {
                    colorType = rateItem.Key;
                    break;
                }
            }

            //초기화
            Initialize(colorType, position);
        }

        /**
         *  @brief  Block의 Board Container Index 지정
         *          Match, Drop 이벤트 등에 사용
         *  @param  idx : Block index
         */
        public void SetIndex(int idx)
        {
            this.Idx = idx;
        }

        /**
         *  @brief  Block 상태 변경
         *  @param  blockState : 변경하려는 상태
         */
        public void SetBlockState(BlockState blockState)
        {
            if(blockState == state.Value) {
                return;
            }

            state.Value = blockState;

            //Block이 Destroy로 변경될 경우 Size Shake 후 파괴
            if(blockState == BlockState.DESTROYED) {
                isShakeScale.Value = true;

                var actDestroy = UniTask.Action(async () => {
                    await UniTask.Delay(TimeSpan.FromSeconds(ShakeScaleDuration));
                    isShakeScale.Value = false;
                    state.Value = BlockState.EMPTY;
                });

                actDestroy();
            }
        }

        /**
         *  @brief  Block 색상 변경
         *  @param  matchColorType : 변경하려는 색상 정보
         */
        public void SetMatchColor(MatchColorType matchColorType)
        {
            color.Value = matchColorType;
        }


        /**
         *  @brief  Block을 이동 불가능한 상태로 변경
         *  @detail Drop Event 계산 중 이미 이동 처리가 된 Block은 Locking하여 Drop 대상에서 제외시킨다.
         *  @param  isLock : true(이동 불가능) / false(이동 가능)
         */
        public void SetLocking(bool isLock)
        {
            IsLocking = isLock;
        }

        /**
         *  @brief  활성화된 Block인지 확인
         *  @return true : 활성화,  false : 비활성화
         */
        public bool IsEnableBlock()
        {
            var blockState = this.State;
            if(blockState == BlockState.EMPTY || blockState == BlockState.NONE) {
                return false;
            }

            return true;
        }

        /**
         *  @brief  Block의 현재 상태 확인
         *  @param  state : 확인하려는 Block 상태
         *  @return true : 동일한 상태,  false : 동일하지 않은 상태
         */
        public bool IsCompareState(BlockState state)
        {
            if(this.state.Value == state) {
                return true;
            }
            return false;
        }

        /**
         *  @brief  Block의 Match Color 확인
         *  @param  type : 확인하려는 Match Color
         *  @return true : 동일한 Match Color,  false : 동일하지 않은 Match Color
         */
        public bool IsCompareColor(MatchColorType type)
        {
            if(color.Value == type) {
                return true;
            }
            return false;
        }

        /**
         *  @brief  Block 움직이기
         *  @param  to : 이동할 위치, speed : 이동 속도, isRewind : 이동 후 위치 되돌리기
         */
        public async UniTask MoveBlock(Vector2 to, float speed, bool isRewind)
        {
            Vector2 from = Position;

            //Move Block
            for(float t = 0f; t < 1f; t += Time.deltaTime / speed) {
                position.Value = Vector2.Lerp(from, to, t);
                await UniTask.Yield();
            }
            position.Value = to;

            //위치 되돌리기
            if(isRewind) {
                await MoveBlock(from, speed, false);
            }
        }

        /**
         *  @brief   경로 정보를 참고하여 Block 이동(Path)
         *  @param  blockPathData : Block의 이동 경로, speed : 이동 속도
         */
        public async UniTask MoveBlock(BlockPathData blockPathData, float speed)
        {
            var pathData = blockPathData.PathData;

            for(int i = 0; i < pathData.Count; i++) {
                if(pathData.Count <= i) {
                    continue;
                }

                Vector2 from = Position;
                Vector2 to = pathData[i];

                //이동 하려는 Block 활성화
                if(!IsCompareState(BlockState.NORMAL) && from != to) {
                    state.Value = BlockState.NORMAL;
                }

                //Move Block
                for(float t = 0f; t < 1f; t += Time.deltaTime / speed) {
                    position.Value = Vector2.Lerp(from, to, t);
                    await UniTask.Yield();
                }
                position.Value = to;
            }

            //이동 완료 후 Position Shake
            isShakePosition.Value = true;
            await UniTask.Delay(TimeSpan.FromSeconds(ShakePositionDuration));
            isShakePosition.Value = false;
        }


        public class Factory :PlaceholderFactory<BoardItemData, Transform, Vector2, BlockModel>
        {

        }

    }
}

