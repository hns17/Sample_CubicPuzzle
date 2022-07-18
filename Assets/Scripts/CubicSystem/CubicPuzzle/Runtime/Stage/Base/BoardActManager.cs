using Cysharp.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board의 Event 관리자
     *          Swipe, Match Check, Destroy & Drop 이벤트 처리
     */
    public class BoardActManager
    {
        protected BoardModel board;

        //Match Event 처리
        protected IMatchEvaluator matchEvaluator;

        //Drop & Fill Event 처리
        protected IDropAndFillEvent eventDropNFill;


        //Injection Parameter
        public BoardActManager(BoardModel board, 
                            IMatchEvaluator matchEvaluator, 
                            IDropAndFillEvent eventDropNFill)
        {
            this.board = board;
            
            this.matchEvaluator = matchEvaluator;
            this.eventDropNFill = eventDropNFill;
        }


        /**
         *  @brief 블럭 매칭 이벤트
         */
        public async UniTask MatchEvent()
        {
            board.SetBoardState(BoardState.MATCH_EVENT);
            do {
                //Start Block Destory
                await DestroyMatchBlocks();

                //Start Drop And Fille
                await eventDropNFill.StartDropAndFill();
            } while(matchEvaluator.EvalNUpdateMatchBoard());

            //Check board clear quest
            if(!board.CheckClearQuest()) {
                //Board에 Match 가능한 Block이 있는지 확인
                if(!await matchEvaluator.EvalPossibleBoard()) {
                    Debug.Log("Impossible");
                    await board.NoMoreMatchEvent();
                }
            }
        }

        /**
         *  @brief  Match된 블럭 파괴
         */
        private async UniTask DestroyMatchBlocks()
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

        public bool EvalNUpdateMatchBlock(BlockModel block)
        {
            return matchEvaluator.EvalNUpdateMatchBlock(block);
        }
    }
}