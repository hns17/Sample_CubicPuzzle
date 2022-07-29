using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class ThreeMatchBoardActManager :BoardActManager, ISwipeBlockEvent
    {
        //매치 가능한 블럭 정보를 기록
        private MatchHelpInfo matchHelper;

        public override void Initalize()
        {
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
                        bool isMatch = EvaluatorNUpdate(from);
                        isMatch |= EvaluatorNUpdate(to);

                        //Move Block
                        await (
                            from.MoveBlock(to.Position, 0.25f, !isMatch),
                            to.MoveBlock(from.Position, 0.25f, !isMatch)
                        );

                        //Match되지 않은 경우 Board Container의 Block 정보를 이전 상태로 
                        if(isMatch) {
                            await MatchEvent();
                        }
                        else {
                            board.SwapBlock(from.Idx, to.Idx);
                        }

                        board.SetBoardState(BoardState.READY);
                        break;
                    }
                }
            }
        }

        public override async UniTask MatchEvent()
        {
            board.SetBoardState(BoardState.MATCH_EVENT);
            do {
                cts.Token.ThrowIfCancellationRequested();
                //Start Block Destory
                await DestroyMatchBlocks();

                //Start Drop And Fille
                await eventDropNFill.StartDropAndFill();
            } while(EvaluatorNUpdate(null));

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
                if(!blocks[i].IsEnableBlock()) {
                    continue;
                }

                var blockIndex = blocks[i].Idx;
                //대상 Block을 전체 Neigh Block과 Swipe
                for(BlockNeighType neighType = BlockNeighType.START + 1; neighType < BlockNeighType.NONE; neighType++) {
                    await UniTask.Yield();

                    var neighBlock = board.GetNeighBlock(blocks[i], neighType);
                    if(neighBlock == null || !neighBlock.IsEnableBlock()) {
                        continue;
                    }
                    var neighIndex = neighBlock.Idx;

                    //대상 블럭과 이웃 블럭 위치 Swap
                    board.SwapBlock(blockIndex, neighIndex);

                    matchIndices.Clear();

                    isMatched |= Evaluator(blocks[i]);

                    //Match 정보 Update
                    if(isMatched) {
                        matchHelper.UpdateMatchHelpInfo(blocks[blockIndex], blocks[neighIndex], matchIndices);
                    }

                    //평가 후 원래 위치로
                    board.SwapBlock(blockIndex, neighIndex);
                }
            }
            matchIndices.Clear();
            return isMatched;
        }

        public MatchHelpInfo GetMatchHelpInfo()
        {
            return matchHelper;
        }

        /**
         *  @brief   CustomFactoryClass
         */
        public new class Factory :IFactory<BoardModel, BoardActManager>
        {
            [Inject] private DiContainer diContainer;
            public BoardActManager Create(BoardModel param)
            {
                var newActManager = diContainer.Instantiate<ThreeMatchBoardActManager>(new object[] { param });

                newActManager.matchHelper = new MatchHelpInfo(param);
                newActManager.matchEvaluator = new ThreeMatchHexEvaluator(param);
                newActManager.eventDropNFill = new HexDropDownAndFillEvent(param);

                return newActManager;
            }
        }
    }
}
