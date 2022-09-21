using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  ExtraPattern Event�� �����ϱ� ���� ��ü
     */
    public class ThreeMatchExtraPatternEvent
    {
        private BoardModel board;

        //Extra Pattern List
        private List<PatternNode> extraPatterns;
        
        public ThreeMatchExtraPatternEvent(BoardModel board, List<PatternNode> extraPatterns)
        {
            this.board = board;
            this.extraPatterns = extraPatterns;
        }


        /**
         *  @brief  Eval Extra Pattern
         *  @param  matchBlocks : Match Block List
         *  @return List<(convert block type, extra match blocks)> : extra match table
         */
        public List<(BlockType, List<BlockModel>)> Evaluator(List<BlockModel> matchBlocks)
        {
            List<BlockModel> blocks = board.Blocks;

            List<(BlockType, List<BlockModel>)> extraMatchTable = ListPool<(BlockType, List<BlockModel>)>.Get();
            List<UniTask> mergeTasks = ListPool<UniTask>.Get();

            //Match�� Block�� �������� Extra Pattern�� �ִ��� ��
            for(int i = 0; i < extraPatterns.Count; i++) {
                for(int index = 0; index< matchBlocks.Count; index++) {
                    //extra match block indices
                    CubicSpanArray<int> extraIndices = new CubicSpanArray<int>(stackalloc int[20]);
                    if(RecursiveEvalExtraPattern(matchBlocks[index], extraPatterns[i], ref extraIndices)) {
                        
                        //Extra Match Block�� ��� ���� Extra�� ���� �� ���� ����
                        List<BlockModel> matchs = ListPool<BlockModel>.Get();
                        for(int j = 0; j < extraIndices.Count; j++) {
                            matchs.Add(blocks[extraIndices[j]]);
                            blocks[extraIndices[j]].SetBlockState(BlockState.EXTRA);
                        }

                        /**
                         *  @Todo : ����� Convert Type�� �Ϲ� Block���� ������
                         *          �� �� Ư�� ������ ���� �ʿ�
                         */
                        extraMatchTable.Add((BlockType.NORMAL, matchs));
                        break;
                    }
                }
            }

            return extraMatchTable;
        }


        /**
         *  @brief  ��� ����� Extra Patten ���� ���� Ž���� ���� ���ȣ���� ���
         *  @param  targetBlock : ������ �� Block
         *  @param  pattern : pattern info
         *  @param  extraIndices : match�� block indices
         *  @return true : match true / false : match false
         */
        private bool RecursiveEvalExtraPattern(BlockModel targetBlock, PatternNode pattern
           , ref CubicSpanArray<int> extraIndices)
        {
            //only default block
            if(!targetBlock.IsCompareType(BlockType.NORMAL)) {
                return false;
            }

            //there must be color
            if(targetBlock.IsCompareColor(MatchColorType.NONE)) {
                return false;
            }

            //there must be match and normal
            if(!targetBlock.IsCompareState(BlockState.MATCH)
                    && !targetBlock.IsCompareState(BlockState.NORMAL)) {
                return false;
            }

            //get neigh block
            BlockModel neighBlock =
                pattern.neighType == BlockNeighType.NONE ?
                targetBlock : board.GetNeighBlock(targetBlock, pattern.neighType);

            if(neighBlock == null) {
                return false;
            }
            //match failed
            else if(!targetBlock.IsCompareColor(neighBlock.Color) || 
                    !(neighBlock.IsCompareState(BlockState.MATCH) 
                        || neighBlock.IsCompareState(BlockState.NORMAL))
            ) {
                return false;
            }
            else {
                //search child node
                List<PatternNode> neighs = pattern.child;
                for(int i = 0; i < neighs.Count; i++) {
                    if(!RecursiveEvalExtraPattern(neighBlock, neighs[i], ref extraIndices)) {
                        return false;
                    }
                }
            }

            extraIndices.Add(neighBlock.Idx);
            return true;
        }


        /**
         *  @brief  extra match �̺�Ʈ ����
         *  @detail ShakeScale->Merge->Convert Block
         *  @param  extraMatchs : List<(convert block type, merge block list)> 
         *  @param  targetBlocks : merge �� targetBlocks
         */
        public async UniTask RunExtraPatternEvent(List<(BlockType, List<BlockModel>)> extraMatchs, BlockModel[] targetBlocks)
        {
            List<UniTask> extraTask = UnityEngine.Pool.ListPool<UniTask>.Get();

            for(int i = 0; i < extraMatchs.Count; i++) {
                List<BlockModel> extraBlocks = extraMatchs[i].Item2;

                //�߽� ���̵� �� ���ϱ�
                //match block list�� target block�� �ִ� ��� �ش� block�� mainblock����
                int mainBlockIdx = Mathf.Max(extraBlocks.IndexOf(targetBlocks[0]), extraBlocks.IndexOf(targetBlocks[1]));
                
                //���� ��� ������ root block�� main���� ����, root block�� ���� ������ block
                if(mainBlockIdx < 0) {
                    mainBlockIdx = extraBlocks.Count - 1;
                }
                BlockModel mainBlock = extraBlocks[mainBlockIdx];
                Vector2 rootPos = board.Cells[mainBlock.Idx].Position;

                for(int j = 0; j < extraBlocks.Count; j++) {
                    int index = j;
                    extraTask.Add(UniTask.Create(async () => {
                        //shake scale
                        await extraBlocks[index].ShakeScale();

                        //merge block
                        await extraBlocks[index].MoveBlock(rootPos, 0.25f, false);
                        
                        //convert main block
                        if(mainBlock == extraBlocks[index]) {
                            mainBlock.Initialize(BlockType.NORMAL, mainBlock.Color, rootPos);
                        }
                        else {
                            extraBlocks[index].SetBlockState(BlockState.EMPTY);
                        }
                    }));
                }
            }
            await UniTask.WhenAll(extraTask);
        }
    }
}
