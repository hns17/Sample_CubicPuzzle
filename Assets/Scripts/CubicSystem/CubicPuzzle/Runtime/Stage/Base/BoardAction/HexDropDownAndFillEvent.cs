using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board의 Block 이동 및 채우기 이벤트 처리
     */
    public class HexDropDownAndFillEvent :IDropAndFillEvent
    {
        //Block 이동 속도
        private const float BlockMoveSpeed = .15f;

        private BoardModel board;

        //단계별 이동 계산 횟수
        private int stepCount;

        //Block의 이동 경로 정보
        private Dictionary<BlockModel, BlockPathData> pathTable;
        private List<UniTask> uniTasks = new List<UniTask>();

        //PathData ObjectPool field
        private const int pathDataPoolCapacity = 300;
        private IObjectPool<BlockPathData> pathDataPool;

        public HexDropDownAndFillEvent(BoardModel board)
        {
            stepCount = 0;
            this.board = board;

            //Setup PathTable Object
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
            List<BlockModel> blocks = board.Blocks;
            //Board의 전체 Block을 대상으로
            //이동 가능한 Block이 없을때 까지 이동 정보 생성


            int loopCount = blocks.Count - 1;
            do {
                isMoveBlock = false;

                for(int i = loopCount; i >= 0; i--) {
                    BlockModel block = blocks[i];

                    //빈 블럭은 제외
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

                //이동을 막아둔 블럭 해제
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
         *  @brief  이동 경로 정보 추가하기
         *  @param  moveBlock(이동 블럭), movePosition(이동할 위치)
         *  @param  moveDirection : 이동한 방향 정보
         */
        private void InsertPathData(BlockModel moveBlock, Vector2 movePosition, BlockNeighType moveDirection = BlockNeighType.NONE)
        {
            //경로 정보에 정보가 없는 경우 초기 데이터 생성(struct BlockPathData)
            BlockPathData blockPathData;

            if(!pathTable.TryGetValue(moveBlock, out blockPathData)) {
                blockPathData = pathDataPool.Get();
                pathTable[moveBlock] = blockPathData;
                blockPathData.FromPosition = moveBlock.Position;
            }

            //최대 이동 거리 갱신
            if(blockPathData.Count + 1 > stepCount) {
                stepCount = blockPathData.Count + 1;
            }

            //경로 정보 추가하기
            blockPathData.InsertData(movePosition, stepCount - 1, moveDirection);

            //해당 블럭 이동 잠그기
            moveBlock.SetLocking(true);
        }


        /**
         *  @brief  블럭 아래로 떨어뜨리기
         *  @param  targetBlock : DropDown 시킬 블럭
         *  @return 떨어지는 블럭이 있는 경우(true) / 없는 경우(false)
         */
        private bool BlockDropDownEvent(BlockModel targetBlock)
        {

            //아래로 떨어질수 있는지 확인
            BlockModel downNeigh = board.GetNeighBlock(targetBlock, BlockNeighType.DOWN);
            if(downNeigh == null || !downNeigh.IsEmptyBlock()) {
                return false;
            }
            
            //이동 경로 정보에 추가
            Vector2 toPos = board.GetCellPosition(downNeigh.Idx);
            InsertPathData(targetBlock, toPos);
            
            //보드의 블럭 위치 정보 변경
            board.SwapBlock(targetBlock.Idx, downNeigh.Idx);

            return true;
        }


        /**
         *  @brief  블럭 대각선으로 떨어뜨리기
         *  @param  targetBlock : 슬라이드 이동 할 블럭
         *  @return 대각선으로 떨어지는 블럭이 있는 경우(true) / 없는 경우(false)
         */
        private bool BlockDropSlideEvent(BlockModel targetBlock)
        {
            //좌/우 하단 블럭 중 이동 가능한 블럭있는지 확인
            var firstSlideDireciton = BlockNeighType.RIGHT_DOWN;
            var secondSlideDireciton = BlockNeighType.LEFT_DOWN;

            //이동할 방향 우선 순위
            BlockPathData blockPathData;

            if(pathTable.TryGetValue(targetBlock, out blockPathData)) { 
                if(blockPathData.PrevMoveDirection == secondSlideDireciton) {
                    firstSlideDireciton = BlockNeighType.LEFT_DOWN;
                    secondSlideDireciton = BlockNeighType.RIGHT_DOWN;
                }
            }

            //이동할 위치 정보 가져오기
            BlockNeighType slideDirection = firstSlideDireciton;
            BlockModel sideNeigh = board.GetNeighBlock(targetBlock, slideDirection);
            if(sideNeigh == null || !sideNeigh.IsEmptyBlock()) {
                slideDirection = secondSlideDireciton;
                sideNeigh = board.GetNeighBlock(targetBlock, slideDirection);
            }

            //이동이 불가능한 경우
            if(sideNeigh == null || !sideNeigh.IsEmptyBlock()) {
                return false;
            }

            //이동 하려는 Side Block의 윗 쪽에 드랍 가능한 블럭이 있으면 제외
            if(IsDetectBlockState_InNeighDirection(sideNeigh, BlockNeighType.UP, BlockState.NORMAL)) {
                return false;
            }


            //이동 하려는 Side Block의 윗 쪽에 블럭을 채우는 Cell이 있으면 제외
            bool isFillCell = false;
            int upIdx = sideNeigh.Idx;
            while(upIdx >= 0 && !isFillCell) {
                if(board.Blocks[upIdx].IsCompareType(BlockType.NONE)) {
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

            //아래로 드랍될 가능성이 있는 경우 제외
            if(IsDetectBlockState_InNeighDirection(targetBlock, BlockNeighType.DOWN, BlockState.EMPTY)) {
                return false;
            }

            //이동 경로 정보에 추가
            Vector2 toPos = board.GetCellPosition(sideNeigh.Idx);
            InsertPathData(targetBlock, toPos, slideDirection);

            //보드의 블럭 위치 정보 변경
            board.SwapBlock(targetBlock.Idx, sideNeigh.Idx);

            return true;
        }


        /**
         *  @brief  보드에 새로운 블럭 추가하기
         *  @return 추가 블럭이 있는 경우(true) / 없는 경우(false)
         */
        private bool BlockFillEvent()
        {
            List<BlockModel> blocks = board.Blocks;
            bool isFillBlocks = false;
            float offsetY = 0.5f;

            int blockCount = board.BlockCount;

            for(int i = 0; i < blockCount; i++) {
                BlockModel block = blocks[i];
                CellModel targetCell = board.Cells[block.Idx];

                //숨겨진 셀은 제외
                //if(!targetCell.IsEnableCell()) {
                //    continue;
                //}

                //블럭이 채워지는 셀이 아닌 경우...
                if(!targetCell.IsFillBlock) {
                    continue;
                }

                //블럭이 비어있지 않은 경우...
                if(!block.IsEmptyBlock()) {
                    continue;
                }

                isFillBlocks = true;

                //블럭의 초기 위치 정보
                float cellSizeY = board.CellSize.y + offsetY;
                Vector2 cellPos = board.GetCellPosition(block.Idx);

                //블럭 정보 재구성 및 상태 변경
                block.Initialize(BlockType.NORMAL, targetCell.BlockFillRate, cellPos + new Vector2(0f, cellSizeY));
                block.SetBlockState(BlockState.FILL_WAIT);

                //블럭의 이동 경로 정보에 추가
                InsertPathData(block, cellPos);
            }

            return isFillBlocks;
        }

        
        /**
         *  @brief  지정된 방향의 이웃 블럭에 특정 상태를 가진 블럭이 있는지 판단
         *          블럭을 찾거나 더 이상 탐색 불가능할때 까지 반복 탐색
         *  @param  target : 탐색 기준 블럭, neighType : 탐색 방향, blockState : 탐색할 블럭의 상태
         *  @return bool : true(조건에 맞는 블럭이 있는 경우) / false(없는 경우)
         */
        private bool IsDetectBlockState_InNeighDirection(BlockModel target, BlockNeighType neighType, BlockState blockState)
        {
            BlockModel targetBlock = target;

            while(true) {
                BlockModel neighBlock = board.GetNeighBlock(targetBlock, neighType);

                //탐색 방향으로 더 이상 진행이 불가능 한 경우
                if(neighBlock == null || !board.Cells[neighBlock.Idx].IsEnableCell()) {
                    return false;
                }
                
                //조건에 맞는 블럭을 찾은 경우
                if(neighBlock.IsCompareState(blockState)) {
                    return true;
                }

                targetBlock = neighBlock;
            }
        }


        /**
         *  @brief  PathTable 정보 초기화
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

