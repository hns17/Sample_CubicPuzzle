using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  3-Match Board 이벤트를 수행하는 클래스
     *  @detail swipe, match and destroy, drop and fill, extra pattern event
     */
    public class ThreeMatchBoardActManager :BoardActManager, ISwipeBlockEvent
    {
        //매치 가능한 블럭 정보를 기록
        private ThreeMatchHelpInfo matchHelper;
        
        //current swipe blocks
        private BlockModel[] swipeBlock = new BlockModel[2];

        //extra pattern event
        private ThreeMatchExtraPatternEvent extraEvent;

        [Inject]
        private void InjectDependices(CubicPuzzlePatternData extraPattern
            , MatchEvaluatorFactory matchEvalFactory)
        {
            matchEvaluator = matchEvalFactory.Create(board);
            //create extra pattern event
            extraEvent = new ThreeMatchExtraPatternEvent(board, extraPattern.patternData[board.BoardStyle].container);
        }

        public override void Initalize()
        {
            matchHelper ??= new ThreeMatchHelpInfo(board);
            matchHelper.Clear();
        }

        /**
         *  @brief  Block Swipe Event
         *  @param  from : 선택된 Block, to : 교환될 Block
         */
        public async UniTask DoSwipeAction(BlockModel from, BlockModel to)
        {
            if(!to.IsEnableBlock() || !from.IsEnableBlock()) {
                return;
            }

            if(board.State == BoardState.READY) {

                swipeBlock[0] = from;
                swipeBlock[1] = to;

                //두 Block이 이웃 관계인지 확인
                for(BlockNeighType neighType = BlockNeighType.START;
                        neighType < BlockNeighType.NONE; neighType++) {

                    int fromIdx = from.Idx;
                    int toIdx = to.Idx;

                    //이웃 관계인 경우 SwipeEvent
                    if(board.GetNeighIndex(fromIdx, neighType) == toIdx) {
                        board.SetBoardState(BoardState.MATCH_EVENT);
                        board.SwapBlock(from.Idx, to.Idx);

                        //Match Check
                        bool isMatch = Evaluator(from, true);
                        isMatch |= Evaluator(to, true);

                        //Move Block
                        await (
                            from.MoveBlock(to.Position, 0.25f, !isMatch),
                            to.MoveBlock(from.Position, 0.25f, !isMatch)
                        );

                        
                        if(isMatch) {
                            await MatchEvent();
                        }
                        //Match되지 않은 경우 Block 정보를 이전 상태로 
                        else {
                            board.SwapBlock(from.Idx, to.Idx);
                        }

                        board.SetBoardState(BoardState.READY);
                        break;
                    }
                }
                swipeBlock[0] = swipeBlock[1] = null;
            }
        }


        public override async UniTask MatchEvent()
        {
            board.SetBoardState(BoardState.MATCH_EVENT);

            do {
                cts.Token.ThrowIfCancellationRequested();

                //Start Block Destory
                await DestroyMatchBlocks(board.GetMatchBlocks());

                //Start Drop And Fille
                await eventDropNFill.StartDropAndFill();
            } while(Evaluator(null, true));


            //Check board clear quest
            if(!board.CheckClearQuest()) {
                //Board에 Match 가능한 Block이 있는지 확인
                if(!await IsPossibleBoard()) {
                    Debug.Log("Impossible");
                    await NoMoreMatchEvent();
                }
            }
            board.SetBoardState(BoardState.READY);
        }


        protected override UniTask DestroyMatchBlocks(List<BlockModel> matchBlocks)
        {
            //eval extra pattern
            var extraMatchs = extraEvent.Evaluator(matchBlocks);

            //run extra patten event & destroy match blocks
            return UniTask.WhenAll(base.DestroyMatchBlocks(matchBlocks),
                extraEvent.RunExtraPatternEvent(extraMatchs, swipeBlock));
        }


        /**
        *  @brief  게임 진행이 가능한 보드인지 확인한다.
        *  @return true(가능한 경우), false(불가능한 경우)
        */
        protected override async UniTask<bool> IsPossibleBoard()
        {
            var blocks = board.Blocks;
            bool isMatched = false;
            matchHelper.Clear();

            //Board의 전체 Block을 대상으로 Block 단위 평가
            for(int i = 0; i < blocks.Count; i++) {
                await UniTask.Yield();
                if(!blocks[i].IsEnableBlock()) {
                    continue;
                }

                var blockIndex = blocks[i].Idx;
                //대상 Block을 전체 Neigh Block과 Swipe
                for(BlockNeighType neighType = BlockNeighType.START; neighType < BlockNeighType.NONE; neighType++) {
                    BlockModel neighBlock = board.GetNeighBlock(blocks[i], neighType);
                    if(neighBlock == null || !neighBlock.IsEnableBlock()) {
                        continue;
                    }
                    int neighIndex = neighBlock.Idx;

                    //대상 블럭과 이웃 블럭 위치 Swap
                    board.SwapBlock(blockIndex, neighIndex);

                    HashSet<int> matchIndices = UnityEngine.Pool.HashSetPool<int>.Get();
                    isMatched |= Evaluator(blocks[i], false, matchIndices);

                    //Match 정보 Update
                    if(isMatched) {
                        matchHelper.UpdateMatchHelpInfo(blocks[blockIndex], blocks[neighIndex], matchIndices);
                    }

                    //평가 후 원래 위치로
                    board.SwapBlock(blockIndex, neighIndex);
                }
            }
            return isMatched;
        }

        public ThreeMatchHelpInfo GetMatchHelpInfo()
        {
            return matchHelper;
        }

        /**
         *  @brief   CustomFactoryClass
         */
        //public new class Factory :IFactory<BoardModel, BoardActManager>
        //{
        //    [Inject] private DiContainer diContainer;
        //    public BoardActManager Create(BoardModel param)
        //    {
        //        var newActManager = diContainer.Instantiate<ThreeMatchBoardActManager>(new object[] { param });

        //        newActManager.matchHelper = new MatchHelpInfo(param);
        //        newActManager.matchEvaluator = new ThreeMatchHexEvaluator(param);
        //        newActManager.eventDropNFill = new HexDropDownAndFillEvent(param);

        //        return newActManager;
        //    }
        //}
    }
}
