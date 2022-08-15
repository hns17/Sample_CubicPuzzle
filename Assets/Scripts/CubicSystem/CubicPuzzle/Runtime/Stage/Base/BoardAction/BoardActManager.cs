using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Zenject;

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

        protected HashSet<int> matchIndices = new HashSet<int>();
        protected CancellationTokenSource cts;

        [Inject]
        public void InjectDependices(BoardModel boardModel, CTSManager ctsManager)
        {
            this.board = boardModel;
            this.cts = ctsManager.GetDefaultCancellationTokenSource();
        }

        public virtual void Initalize()
        {

        }

        /**
         *  @brief 블럭 매칭 이벤트
         */
        public abstract UniTask MatchEvent();

        /**
         *  @brief  진행 가능한 Board인지 체크
         *  @return bool
         */
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

        public class Factory :PlaceholderFactory<BoardModel, BoardActManager> { 
        }
    }
}