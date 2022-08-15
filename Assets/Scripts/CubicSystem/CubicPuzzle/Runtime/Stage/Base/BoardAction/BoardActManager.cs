using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Zenject;

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
         */
        protected async UniTask DestroyMatchBlocks()
        {

            //Match ������ Block -> Destroy�� ����
            var blocks = board.Blocks;
            for(int i=0; i<blocks.Count; i++) {
                if(blocks[i] == null) {
                    continue;
                }
                else if(blocks[i].IsCompareState(BlockState.MATCH)) {
                    blocks[i].SetBlockState(BlockState.DESTROYED);
                }
            }

            //Destroy�� ����� ���� ��� �ı��ɶ����� ���
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
         *  @brief  ��ϵ� Match Block�� ���¸� ����
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
            //��� Block�� ������ �⺻ �������� ����
            foreach(var block in blocks) {
                if(block.IsEnableBlock()) {
                    block.SetMatchColor(MatchColorType.NONE);
                    block.SetBlockState(BlockState.MATCH);
                    await UniTask.Delay(100);
                }
            }

            await UniTask.Delay(500);
            await DestroyMatchBlocks();

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