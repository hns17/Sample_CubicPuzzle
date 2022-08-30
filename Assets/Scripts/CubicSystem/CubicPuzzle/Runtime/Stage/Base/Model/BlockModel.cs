
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Block ���� �� ���� ����
     */
    public class BlockModel
    {
        //Block Index
        public int Idx { get; private set; }

        //�̵�(Drop) ������ ���ΰ�?
        public bool IsLocking { get; private set; }

        //���� ��ġ, IObservable, Value Getter ����
        private ReactiveProperty<Vector2> position = new ReactiveProperty<Vector2>(Vector2.zero);
        public Vector2 Position => position.Value;
        public IObservable<Vector2> PositionObservable => position;


        //���� ����, IObservable, Value Getter ����
        private ReactiveProperty<MatchColorType> color = new ReactiveProperty<MatchColorType>(MatchColorType.RED);
        public MatchColorType Color => color.Value;
        public IObservable<MatchColorType> ColorObservable => color;

        private ReactiveProperty<BlockType> blockType = new ReactiveProperty<BlockType>();
        public IObservable<BlockType> BlockTypeObservable => blockType;
        public BlockType BlockType
        {
            get { return blockType.Value; }
            private set
            {
                //CellType None�� ��� ������� �ʴ� Cell�� ó��
                blockType.Value = value;
                if(value == BlockType.NONE) {
                    state.Value = BlockState.EMPTY;
                }
            }
        }
        

        //���� ���� ����, IObservable, Value Getter ����
        private ReactiveProperty<BlockState> state = new ReactiveProperty<BlockState>(BlockState.NORMAL);
        public BlockState State => state.Value;
        public IObservable<BlockState> StateObservable => state;

        //Block ��ġ ����, IObservable ����
        private ReactiveProperty<bool> isShakePosition = new ReactiveProperty<bool>(false);
        public IObservable<bool> ShakePositionObservable => isShakePosition;

        //Block ũ�� ����, IObservable ����
        private ReactiveProperty<bool> isShakeScale = new ReactiveProperty<bool>(false);
        public IObservable<bool> ShakeScaleObservable => isShakeScale;

        //Shake Duration
        public const float ShakeScaleDuration = 1.2f;
        public const float ShakePositionDuration = 0.2f;
        

        [Inject]
        private BlockModel(BoardItemData itemData, Transform parent, Vector2 position, BlockPresenter.Factory factory) {
            
            //Make Block Gameobject
            Initialize(itemData, position);

            factory.Create(this, parent);
        }

        public void Initialize(BoardItemData itemData, Vector2 position)
        {
            Initialize(itemData.blockType, itemData.color, position);
        }

        /**
         *  @brief  Block ���� �ʱ�ȭ
         *  @param  colorType : MatchColor, position : ��ġ
         */
        public void Initialize(BlockType blockType, MatchColorType colorType, Vector2 position)
        {
            this.IsLocking = false;
            this.position.Value = position;
            this.state.Value = BlockState.NORMAL;
            this.color.Value = colorType;
            
            this.BlockType = blockType;
        }

        /**
         *  @brief  Block ���� �ʱ�ȭ, FillRate�� �̿��� Ȯ�������� ���� ����
         *  @param  blockFillRate : ���� ���� ���� Ȯ��, position : ��ġ
         */
        public void Initialize(BlockType blockType, Dictionary<MatchColorType, float> blockFillRate, Vector2 position)
        {
            MatchColorType colorType = MatchColorType.NONE;

            //Ȯ���� �ִ�ġ ���ϱ�
            float maxValue =0f;
            foreach(var rateValue in blockFillRate.Values) {
                maxValue += rateValue;
            }


            //Rnd �� ����
            var rndRate = UnityEngine.Random.Range(0, maxValue);

            //���� �����ϱ�
            float calcRate = 0f;
            foreach(var rateItem in blockFillRate) {
                calcRate += rateItem.Value;
                if(calcRate >= rndRate) {
                    colorType = rateItem.Key;
                    break;
                }
            }

            //�ʱ�ȭ
            Initialize(blockType, colorType, position);
        }

        /**
         *  @brief  Block�� Board Container Index ����
         *          Match, Drop �̺�Ʈ � ���
         *  @param  idx : Block index
         */
        public void SetIndex(int idx)
        {
            this.Idx = idx;
        }

        /**
         *  @brief  Block ���� ����
         *  @param  blockState : �����Ϸ��� ����
         */
        public void SetBlockState(BlockState blockState)
        {
            if(blockState == state.Value || this.BlockType == BlockType.NONE) {
                return;
            }

            state.Value = blockState;
        }

        /**
         *  @brief  Block Type ����
         *  @param  blockType : �����Ϸ��� Type
         */
        public void SetBlockType(BlockType blockType)
        {
            if(blockType == this.blockType.Value) {
                return;
            }

            BlockType = blockType;
        }

        /**
         *  @brief  Block ���� ����
         *  @param  matchColorType : �����Ϸ��� ���� ����
         */
        public void SetMatchColor(MatchColorType matchColorType)
        {
            color.Value = matchColorType;
        }


        /**
         *  @brief  Block�� �̵� �Ұ����� ���·� ����
         *  @detail Drop Event ��� �� �̹� �̵� ó���� �� Block�� Locking�Ͽ� Drop ��󿡼� ���ܽ�Ų��.
         *  @param  isLock : true(�̵� �Ұ���) / false(�̵� ����)
         */
        public void SetLocking(bool isLock)
        {
            IsLocking = isLock;
        }

        /**
         *  @brief  Ȱ��ȭ�� Block���� Ȯ��
         *  @return true : Ȱ��ȭ,  false : ��Ȱ��ȭ
         */
        public bool IsEnableBlock()
        {
            if(this.State == BlockState.EMPTY || this.BlockType == BlockType.NONE) {
                return false;
            }

            return true;
        }

        /**
         *  @brief  �� Block���� Ȯ��
         *  @return true : �� Block,  false : ������� �ʰų� Ȱ��ȭ�Ǿ� �ִ� Block
         */
        public bool IsEmptyBlock()
        {
            if(this.State != BlockState.EMPTY || this.BlockType == BlockType.NONE) {
                return false;
            }

            return true;
        }

        /**
         *  @brief  Block�� ���� ���� Ȯ��
         *  @param  state : Ȯ���Ϸ��� Block ����
         *  @return true : ������ ����,  false : �������� ���� ����
         */
        public bool IsCompareState(BlockState state)
        {
            if(this.state.Value == state) {
                return true;
            }
            return false;
        }

        /**
         *  @brief  Block�� Type ��
         *  @param  state : Ȯ���Ϸ��� Block Type
         *  @return true : ������ ����,  false : �������� ���� ����
         */
        public bool IsCompareType(BlockType type)
        {
            if(this.blockType.Value == type) {
                return true;
            }
            return false;
        }

        /**
         *  @brief  Block�� Match Color Ȯ��
         *  @param  type : Ȯ���Ϸ��� Match Color
         *  @return true : ������ Match Color,  false : �������� ���� Match Color
         */
        public bool IsCompareColor(MatchColorType type)
        {
            if(color.Value == type) {
                return true;
            }
            return false;
        }

        /**
         *  @brief  Block �����̱�
         *  @param  to : �̵��� ��ġ, speed : �̵� �ӵ�, isRewind : �̵� �� ��ġ �ǵ�����
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

            //��ġ �ǵ�����
            if(isRewind) {
                await MoveBlock(from, speed, false);
            }
        }

        /**
         *  @brief   ��� ������ �����Ͽ� Block �̵�(Path)
         *  @param  blockPathData : Block�� �̵� ���, speed : �̵� �ӵ�
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

                //�̵� �Ϸ��� Block Ȱ��ȭ
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

            //�̵� �Ϸ� �� Position Shake
            isShakePosition.Value = true;
            await UniTask.Delay((int)(ShakePositionDuration * 1000));
            isShakePosition.Value = false;
        }

        public async UniTask DestroyBlock()
        {
            await ShakeScale();
            //delay one frame, play destroy particle effect
            state.Value = BlockState.DESTROYED;
            await UniTask.Yield();
            state.Value = BlockState.EMPTY;
        }

        public async UniTask ShakeScale()
        {
            isShakeScale.Value = true;
            await UniTask.Delay((int)(ShakeScaleDuration * 1000));
            isShakeScale.Value = false;
        }

        public class Factory :PlaceholderFactory<BoardItemData, Transform, Vector2, BlockModel>
        {

        }

    }
}

