using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board�� Block �̵� �� ä��� �̺�Ʈ ó��
     */
    public class HexDropAndFillEvent :IDropAndFillEvent
    {
        //Block �̵� �ӵ�
        private const float BlockMoveSpeed = .15f;

        private BoardModel board;

        //�ܰ躰 �̵� ��� Ƚ��
        private int stepCount;

        //Block�� �̵� ��� ����
        private Dictionary<BlockModel, BlockPathData> pathTable;
        private List<UniTask> uniTasks = new List<UniTask>();

        public HexDropAndFillEvent(BoardModel board)
        {
            stepCount = 0;
            this.board = board;
            pathTable = new Dictionary<BlockModel, BlockPathData>();
        }

        /**
         *  @brief  Block Drop and Fill Event
         */
        public UniTask StartDropAndFill()
        {
            return CreateDropAndFillTask();
        }


        /**
         *  @brief  Create Block Drop and Fill Task
         */
        private async UniTask CreateDropAndFillTask()
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

                    //Slide Drop Down Event
                    if(!block.IsLocking) {
                        isMoveBlock |= BlockDropSlideEvent(block);
                    }
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
                blockPathData = new BlockPathData(board.BlockCount, moveBlock.Position);
            }

            //�ִ� �̵� �Ÿ� ����
            if(blockPathData.Count + 1 > stepCount) {
                stepCount = blockPathData.Count + 1;
            }

            //��� ���� �߰��ϱ�
            blockPathData.InsertData(movePosition, stepCount - 1, moveDirection);
            pathTable[moveBlock] = new BlockPathData(blockPathData);
            blockPathData.Clear();

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
            if(downNeigh == null || !downNeigh.IsCompareState(BlockState.EMPTY)) {
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
         *  @brief  �� �밢������ ����߸���
         *  @return �밢������ �������� ���� �ִ� ���(true) / ���� ���(false)
         */
        private bool BlockDropSlideEvent(BlockModel targetBlock)
        {
            //��/�� �ϴ� �� �� �̵� ������ ���ִ��� Ȯ��
            var firstSlideDireciton = BlockNeighType.RIGHT_DOWN;
            var secondSlideDireciton = BlockNeighType.LEFT_DOWN;

            //�̵��� ���� �켱 ����
            BlockPathData blockPathData;

            if(pathTable.TryGetValue(targetBlock, out blockPathData)) { 
                if(blockPathData.PrevMoveDirection == secondSlideDireciton) {
                    firstSlideDireciton = BlockNeighType.LEFT_DOWN;
                    secondSlideDireciton = BlockNeighType.RIGHT_DOWN;
                }
            }

            //�̵��� ��ġ ���� ��������
            var slideDirection = firstSlideDireciton;
            var sideNeigh = board.GetNeighBlock(targetBlock, slideDirection);
            if(sideNeigh == null || !sideNeigh.IsCompareState(BlockState.EMPTY)) {
                slideDirection = secondSlideDireciton;
                sideNeigh = board.GetNeighBlock(targetBlock, slideDirection);
            }

            //�̵��� �Ұ����� ���
            if(sideNeigh == null || !sideNeigh.IsCompareState(BlockState.EMPTY)) {
                return false;
            }

            //�̵� �Ϸ��� Side Block�� �� �ʿ� ��� ������ ���� ������ ����
            if(IsDetectBlockState_InNeighDirection(sideNeigh, BlockNeighType.UP, BlockState.NORMAL)) {
                return false;
            }


            //�̵� �Ϸ��� Side Block�� �� �ʿ� ���� ä��� Cell�� ������ ����
            //�̵� �Ϸ��� Side Block�� �� �ʿ� ���� ä��� Cell�� ������ ����
            bool isFillCell = false;
            var upIdx = sideNeigh.Idx;
            while(upIdx >= 0 && !isFillCell) {
                if(board.Blocks[upIdx].IsCompareState(BlockState.NONE)) {
                    break;
                }
                else if(board.Cells[upIdx].IsFillBlock) {
                    isFillCell = true;
                    break;
                }
                upIdx = board.GetNeighIndex(upIdx, BlockNeighType.UP);
            }

            if(isFillCell) {
                return false;
            }

            //�Ʒ��� ����� ���ɼ��� �ִ� ��� ����
            if(IsDetectBlockState_InNeighDirection(targetBlock, BlockNeighType.DOWN, BlockState.EMPTY)) {
                return false;
            }

            //�̵� ��� ������ �߰�
            var toPos = board.GetCellPosition(sideNeigh.Idx);
            InsertPathData(targetBlock, toPos, slideDirection);

            //������ �� ��ġ ���� ����
            board.SwapBlock(targetBlock.Idx, sideNeigh.Idx);

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
                if(!targetCell.IsEnableCell()) {
                    continue;
                }

                //���� ä������ ���� �ƴ� ���...
                if(!targetCell.IsFillBlock) {
                    continue;
                }

                //���� ������� ���� ���...
                if(!block.IsCompareState(BlockState.EMPTY)) {
                    continue;
                }

                isFillBlocks = true;

                //���� �ʱ� ��ġ ����
                float cellSizeY = board.CellSize.y + offsetY;
                var cellPos = board.GetCellPosition(block.Idx);

                //�� ���� �籸�� �� ���� ����
                block.Initialize(targetCell.BlockFillRate, cellPos + new Vector2(0f, cellSizeY));
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
                item.Value.Clear();
            }
            pathTable.Clear();
        }
    }
}

