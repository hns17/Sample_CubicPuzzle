using Cysharp.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board�� Event ������
     *          Swipe, Match Check, Destroy & Drop �̺�Ʈ ó��
     */
    public class BoardActManager
    {
        protected BoardModel board;

        //Match Event ó��
        protected IMatchEvaluator matchEvaluator;

        //Drop & Fill Event ó��
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
         *  @brief �� ��Ī �̺�Ʈ
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
                //Board�� Match ������ Block�� �ִ��� Ȯ��
                if(!await matchEvaluator.EvalPossibleBoard()) {
                    Debug.Log("Impossible");
                    await board.NoMoreMatchEvent();
                }
            }
        }

        /**
         *  @brief  Match�� �� �ı�
         */
        private async UniTask DestroyMatchBlocks()
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

        public bool EvalNUpdateMatchBlock(BlockModel block)
        {
            return matchEvaluator.EvalNUpdateMatchBlock(block);
        }
    }
}