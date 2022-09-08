using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board�� Block �̵� �� ä��� �̺�Ʈ ó��
     */
    public class SquareDropDownAndFillEvent :IDropAndFillEvent
    {
        //Block �̵� �ӵ�
        private const float BlockMoveSpeed = .15f;

        private BoardModel board;

        //�ܰ躰 �̵� ��� Ƚ��
        private int stepCount;

        //Block�� �̵� ��� ����
        private Dictionary<BlockModel, BlockPathData> pathTable;
        private List<UniTask> uniTasks = new List<UniTask>();

        private const int pathDataPoolCapacity = 300;
        private IObjectPool<BlockPathData> pathDataPool;

        public SquareDropDownAndFillEvent(BoardModel board)
        {
            stepCount = 0;
            this.board = board;
            pathTable = new Dictionary<BlockModel, BlockPathData>();
            pathDataPool = new ObjectPool<BlockPathData>(
                ()=> {
                    return new BlockPathData(Vector2.zero); 
                }, 
                null, 
                (BlockPathData pathData)=> {
                    pathData.Clear(); 
                }, 
                null, true, pathDataPoolCapacity);
        }

        /**
         *  @brief  Block Drop and Fill Event
         */
        public async UniTask StartDropAndFill()
        {
            bool isMoveBlock;
            var blocks = board.Blocks;
            //Board�� ��ü Block�� �������
            //�̵� ������ Block�� ������ ���� �̵� ���� ����
            int loopCount = blocks.Count - 1;
            do {
                isMoveBlock = false;

                for(int i = loopCount; i >= 0; i--) {
                    var block = blocks[i];

                    //�� ���� ����
                    if(!block.IsEnableBlock()
                        || block.IsLocking) {
                        continue;
                    }

                    //Block Drop Down Event
                    isMoveBlock |= BlockDropDownEvent(block);
                }

                //Block Fill Event
                isMoveBlock |= BlockFillEvent();

                //�̵��� ���Ƶ� �� ����
                blocks.ForEach(x => x.SetLocking(false));
            }
            while(isMoveBlock);

            foreach(var item in pathTable) {
                uniTasks.Add(item.Key.MoveBlock(item.Value, BlockMoveSpeed));
            }

            //Wait Tasks
            await UniTask.WhenAll(uniTasks);

            //End Task -> Clear
            uniTasks.Clear();
            ResetPathData();
        }


        /**
         *  @brief  �̵� ��� ���� �߰��ϱ�
         *  @param  moveBlock(�̵� ��), movePosition(�̵��� ��ġ)
         */
        private void InsertPathData(BlockModel moveBlock, Vector2 movePosition, BlockNeighType moveDirection = BlockNeighType.NONE)
        {
            //��� ������ ������ ���� ��� �ʱ� ������ ����(struct BlockPathData)
            BlockPathData blockPathData;

            if(!pathTable.TryGetValue(moveBlock, out blockPathData)) {
                blockPathData = pathDataPool.Get();
                pathTable[moveBlock] = blockPathData;
                blockPathData.FromPosition = moveBlock.Position;
            }

            //�ִ� �̵� �Ÿ� ����
            if(blockPathData.Count + 1 > stepCount) {
                stepCount = blockPathData.Count + 1;
            }

            //��� ���� �߰��ϱ�
            blockPathData.InsertData(movePosition, stepCount - 1, moveDirection);

            //�ش� �� �̵� ��ױ�
            moveBlock.SetLocking(true);
        }


        /**
         *  @brief  �� �Ʒ��� ����߸���
         *  @return �������� ���� �ִ� ���(true) / ���� ���(false)
         */
        private bool BlockDropDownEvent(BlockModel targetBlock)
        {

            //�Ʒ��� �������� �ִ��� Ȯ��
            var downNeigh = board.GetNeighBlock(targetBlock, BlockNeighType.DOWN);
            if(downNeigh == null || !downNeigh.IsEmptyBlock()) {
                return false;
            }
            
            //�̵� ��� ������ �߰�
            var toPos = board.GetCellPosition(downNeigh.Idx);
            InsertPathData(targetBlock, toPos);
            
            //������ �� ��ġ ���� ����
            board.SwapBlock(targetBlock.Idx, downNeigh.Idx);

            return true;
        }

        /**
         *  @brief  ���忡 ���ο� �� �߰��ϱ�
         *  @return �߰� ���� �ִ� ���(true) / ���� ���(false)
         */
        private bool BlockFillEvent()
        {
            var blocks = board.Blocks;
            bool isFillBlocks = false;
            float offsetY = 0.5f;

            int blockCount = board.BlockCount;

            for(int i = 0; i < blockCount; i++) {
                var block = blocks[i];
                var targetCell = board.Cells[block.Idx];

                //������ ���� ����
                //if(!targetCell.IsEnableCell()) {
                //    continue;
                //}

                //���� ä������ ���� �ƴ� ���...
                if(!targetCell.IsFillBlock) {
                    continue;
                }

                //���� ������� ���� ���...
                if(!block.IsEmptyBlock()) {
                    continue;
                }

                isFillBlocks = true;

                //���� �ʱ� ��ġ ����
                float cellSizeY = board.CellSize.y + offsetY;
                var cellPos = board.GetCellPosition(block.Idx);

                //�� ���� �籸�� �� ���� ����
                block.Initialize(BlockType.NORMAL, targetCell.BlockFillRate, cellPos + new Vector2(0f, cellSizeY));
                block.SetBlockState(BlockState.FILL_WAIT);

                //���� �̵� ��� ������ �߰�
                InsertPathData(block, cellPos);
            }

            return isFillBlocks;
        }

        
        /**
         *  @brief  ������ ������ �̿� ���� Ư�� ���¸� ���� ���� �ִ��� �Ǵ�
         *          ���� ã�ų� �� �̻� Ž�� �Ұ����Ҷ� ���� �ݺ� Ž��
         *  @param  target : Ž�� ���� ��, neighType : Ž�� ����, blockState : Ž���� ���� ����
         */
        private bool IsDetectBlockState_InNeighDirection(BlockModel target, BlockNeighType neighType, BlockState blockState)
        {
            var targetBlock = target;

            while(true) {
                var neighBlock = board.GetNeighBlock(targetBlock, neighType);

                //Ž�� �������� �� �̻� ������ �Ұ��� �� ���
                if(neighBlock == null || !board.Cells[neighBlock.Idx].IsEnableCell()) {
                    return false;
                }
                
                //���ǿ� �´� ���� ã�� ���
                if(neighBlock.IsCompareState(blockState)) {
                    return true;
                }

                targetBlock = neighBlock;
            }
        }


        /**
         *  @brief  PathTable ���� �ʱ�ȭ
         */
        private void ResetPathData()
        {
            stepCount = 0;

            foreach(var item in pathTable) {
                pathDataPool.Release(item.Value);
            }
            pathTable.Clear();
        }
    }
}

