using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board의 Event 관리자
     *          Swipe, Match Check, Destroy & Drop 이벤트 처리
     */
    public abstract class BoardActManager
    {
        protected BoardModel board;

        //Match Event 처리
        protected IMatchEvaluator matchEvaluator;

        //Drop & Fill Event 처리
        protected IDropAndFillEvent eventDropNFill;

        protected HashSet<int> matchIndices;


        //Injection Parameter
        public BoardActManager(BoardModel board, 
                            IMatchEvaluator matchEvaluator, 
                            IDropAndFillEvent eventDropNFill)
        {
            this.board = board;
            this.matchEvaluator = matchEvaluator;
            this.eventDropNFill = eventDropNFill;

            this.matchIndices = new HashSet<int>();
        }

        public virtual void Initalize()
        {

        }

        /**
         *  @brief 블럭 매칭 이벤트
         */
        public abstract UniTask MatchEvent();


        protected abstract UniTask<bool> IsPossibleBoard();

        /**
         *  @brief  Match된 블럭 파괴
         */
        protected async UniTask DestroyMatchBlocks()
        {
            //Match 상태인 Block -> Destroy로 변경
            var blocks = board.Blocks;
            for(int i=0; i<blocks.Count; i++) {
                if(blocks[i] == null) {
                    continue;
                }
                else if(blocks[i].IsCompareState(BlockState.MATCH)) {
                    blocks[i].SetBlockState(BlockState.DESTROYED);
                }
            }

            //Destroy로 변경된 블럭이 모두 파괴될때까지 대기
            bool isDestroyed = false;
            while(!isDestroyed) {
                await UniTask.Yield();
                isDestroyed = true;

                for(int i = 0; i < blocks.Count; i++) {
                    if(blocks[i].IsCompareState(BlockState.DESTROYED)) {
                        isDestroyed = false;
                        break;
                    }
                }
            }            
        }


        /**
         *  @brief  target Block의 최상단에 위치한 Block Index 반환
         *  @param  targetIndex : Target Block Index
         *  @return int : 최상단 Block Index
         */
        private int GetTopBlockIndex(int targetIndex)
        {
            return targetIndex % board.Col;
        }

        /**
         *  @brief  target Block의 최하단에 위치한 Block Index 반환
         *  @param  targetIndex : Target Block Index
         *  @return int : 최하단 Block Index
         */
        private int GetFirstBottomBlockIndex(int targetIndex)
        {
            return board.Col * board.Row - GetTopBlockIndex(targetIndex);
        }

        /**
         *  @brief  Base 기준 Block의 위치를 반환한다.
         *  @param  baseIdx : 기준이 될 BlockIdx, findIdx : 찾을 BlockIdx
         *  @return 0 : 동일한 라인, 1 : 위, -1 : 아래
         */
        public int GetBasedOnLocation(int baseIdx, int findIdx)
        {
            var baseRowIdx = baseIdx / board.Col;
            var findRowIdx = findIdx / board.Col;

            if(baseRowIdx < findRowIdx) {
                return -1;
            }
            else if(baseRowIdx > findRowIdx) {
                return 1;
            }

            return 0;
        }

        /**
         *  @brief  기록된 Match Block의 상태를 변경
         */
        public void UpdateMatchBlockState()
        {
            var blocks = board.Blocks;
            foreach(int index in matchIndices) {
                blocks[index].SetBlockState(BlockState.MATCH);
            }
        }


        public bool Evaluator(BlockModel block)
        {
            return matchEvaluator.Evaluator(block, matchIndices);
        }

        public bool EvaluatorNUpdate(BlockModel block)
        {
            if(matchEvaluator.Evaluator(block, matchIndices)) {
                UpdateMatchBlockState();
                return true;
            }
            return false;
        }


        public async UniTask NoMoreMatchEvent()
        {
            var blocks = board.Blocks;
            //모든 Block의 색상을 기본 색상으로 변경
            foreach(var block in blocks) {
                if(block.IsEnableBlock()) {
                    block.SetMatchColor(MatchColorType.NONE);
                    block.SetBlockState(BlockState.MATCH);
                    await UniTask.Delay(100);
                }
            }

            await UniTask.Delay(500);
            await DestroyMatchBlocks();

            //Board에 Block을 채우는 Event가 있는지 검사
            if(!IsThereFillInfo()) {
                //GameOver
                board.SetBoardState(BoardState.GAME_OVER);
            }
            //Drop N Fill
            else {
                await MatchEvent();
            }
        }

        /**
         *  @brief  Block 채우기 이벤트가 있는가?
         *  @return bool : true(있는 경우) / false(없는 경우)
         */
        public bool IsThereFillInfo()
        {
            var cells = board.Cells;
            bool checkFillInfo = false;
            foreach(var cell in cells) {
                if(cell.IsFillBlock) {
                    checkFillInfo = true;
                    break;
                }
            }

            return checkFillInfo;
        }
    }
}