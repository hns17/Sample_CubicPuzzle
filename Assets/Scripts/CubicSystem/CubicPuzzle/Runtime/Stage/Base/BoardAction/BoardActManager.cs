using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Zenject;
using Pool = UnityEngine.Pool;

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
        protected CancellationTokenSource cts;

        [Inject]
        public void InjectDependices(BoardModel boardModel, 
                                        DropDownAndFillEventFactory dropEventFactory, 
                                        CTSManager ctsManager) {
            this.board = boardModel;
            this.cts = ctsManager.GetDefaultCancellationTokenSource();
            eventDropNFill = dropEventFactory.Create(boardModel);
        }


        /**
         *  @brief  Board 초기화 함수
         */
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
         *  @param  matchBlocks : Match 처리된 Block List
         */
        protected async virtual UniTask DestroyMatchBlocks(List<BlockModel> matchBlocks)
        {
            List<UniTask> destroyTasks = UnityEngine.Pool.ListPool<UniTask>.Get();

            //Match 상태인 Block -> Destroy로 변경
            for(int idx = 0; idx < matchBlocks.Count; idx++) { 
                if(matchBlocks[idx].IsCompareState(BlockState.MATCH)) {
                    destroyTasks.Add(matchBlocks[idx].DestroyBlock());
                }
            }

            await UniTask.WhenAll(destroyTasks);
        }

        /**
         *  @brief  기록된 Match Block의 상태를 변경
         *  @param  matchIndices : Match Block Indices
         */
        public void UpdateMatchBlockState(HashSet<int> matchIndices)
        {
            List<BlockModel> blocks = board.Blocks;
            foreach(int index in matchIndices) {
                blocks[index].SetBlockState(BlockState.MATCH);
            }
        }

        /**
         *  @brief  Match 평가 함수
         *  @param  block : 평가 대상이 되는 블럭, null 인 경우 전체 보드 평가
         *  @param  isUpdate : true인 경우 match된 블럭 상태 변경
         *  @param  matchIndices : Match Block Indices
         *  @return bool : 매치된 블럭이 있는 경우(true)/없는 경우(false)
         */
        public bool Evaluator(BlockModel block, bool isUpdate, HashSet<int> matchIndices = null)
        {
            matchIndices ??= Pool.HashSetPool<int>.Get();

            if(matchEvaluator.Evaluator(block, matchIndices)) {
                if(isUpdate) {
                    UpdateMatchBlockState(matchIndices);
                }
                return true;
            }
            return false;
        }


        /**
         *  @brief  Match 가능한 블럭이 없을 때 수행되는 보드 이벤트
         */
        public async UniTask NoMoreMatchEvent()
        {
            List<BlockModel> blocks = board.Blocks;
            //모든 Block의 색상을 기본 색상으로 변경
            foreach(BlockModel block in blocks) {
                if(block.IsEnableBlock()) {
                    block.SetMatchColor(MatchColorType.NONE);
                    block.SetBlockState(BlockState.MATCH);
                    await UniTask.Delay(100);
                }
            }

            await UniTask.Delay(500);
            await DestroyMatchBlocks(board.Blocks);

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
            List<CellModel> cells = board.Cells;
            bool checkFillInfo = false;
            foreach(CellModel cell in cells) {
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