using Cysharp.Threading.Tasks;
using System.Collections.Generic;

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
         *  @brief �� ��Ī �̺�Ʈ
         */
        public abstract UniTask MatchEvent();


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
         *  @brief  target Block�� �ֻ�ܿ� ��ġ�� Block Index ��ȯ
         *  @param  targetIndex : Target Block Index
         *  @return int : �ֻ�� Block Index
         */
        private int GetTopBlockIndex(int targetIndex)
        {
            return targetIndex % board.Col;
        }

        /**
         *  @brief  target Block�� ���ϴܿ� ��ġ�� Block Index ��ȯ
         *  @param  targetIndex : Target Block Index
         *  @return int : ���ϴ� Block Index
         */
        private int GetFirstBottomBlockIndex(int targetIndex)
        {
            return board.Col * board.Row - GetTopBlockIndex(targetIndex);
        }

        /**
         *  @brief  Base ���� Block�� ��ġ�� ��ȯ�Ѵ�.
         *  @param  baseIdx : ������ �� BlockIdx, findIdx : ã�� BlockIdx
         *  @return 0 : ������ ����, 1 : ��, -1 : �Ʒ�
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
    }
}