using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Zenject;
using Pool = UnityEngine.Pool;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board�� Event ������
     *          Swipe, Match Check, Destroy & Drop �̺�Ʈ ó��
     */
    public abstract class BoardActManager
    {
        protected BoardModel board;

        //Match Event ó��
        protected IMatchEvaluator matchEvaluator;

        //Drop & Fill Event ó��
        protected IDropAndFillEvent eventDropNFill;
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
         *  @brief �� ��Ī �̺�Ʈ
         */
        public abstract UniTask MatchEvent();

        /**
         *  @brief  ���� ������ Board���� üũ
         *  @return bool
         */
        protected abstract UniTask<bool> IsPossibleBoard();

        /**
         *  @brief  Match�� �� �ı�
         *  @param  matchBlocks : Match ó���� Block List
         */
        protected async virtual UniTask DestroyMatchBlocks(List<BlockModel> matchBlocks)
        {
            List<UniTask> destroyTasks = UnityEngine.Pool.ListPool<UniTask>.Get();

            //Match ������ Block -> Destroy�� ����
            for(int idx = 0; idx < matchBlocks.Count; idx++) { 
                if(matchBlocks[idx].IsCompareState(BlockState.MATCH)) {
                    destroyTasks.Add(matchBlocks[idx].DestroyBlock());
                }
            }

            await UniTask.WhenAll(destroyTasks);
        }

        /**
         *  @brief  ��ϵ� Match Block�� ���¸� ����
         */
        public void UpdateMatchBlockState(HashSet<int> matchIndices)
        {
            var blocks = board.Blocks;
            foreach(int index in matchIndices) {
                blocks[index].SetBlockState(BlockState.MATCH);
            }
        }


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

        public async UniTask NoMoreMatchEvent()
        {
            var blocks = board.Blocks;
            //��� Block�� ������ �⺻ �������� ����
            foreach(var block in blocks) {
                if(block.IsEnableBlock()) {
                    block.SetMatchColor(MatchColorType.NONE);
                    block.SetBlockState(BlockState.MATCH);
                    await UniTask.Delay(100);
                }
            }

            await UniTask.Delay(500);
            await DestroyMatchBlocks(board.Blocks);

            //Board�� Block�� ä��� Event�� �ִ��� �˻�
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
         *  @brief  Block ä��� �̺�Ʈ�� �ִ°�?
         *  @return bool : true(�ִ� ���) / false(���� ���)
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