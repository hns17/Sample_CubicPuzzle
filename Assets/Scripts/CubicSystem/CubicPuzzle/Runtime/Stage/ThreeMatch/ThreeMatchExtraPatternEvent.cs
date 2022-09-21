using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  ExtraPattern Event를 수행하기 위한 객체
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

            //Match된 Block을 기준으로 Extra Pattern이 있는지 평가
            for(int i = 0; i < extraPatterns.Count; i++) {
                for(int index = 0; index< matchBlocks.Count; index++) {
                    //extra match block indices
                    CubicSpanArray<int> extraIndices = new CubicSpanArray<int>(stackalloc int[20]);
                    if(RecursiveEvalExtraPattern(matchBlocks[index], extraPatterns[i], ref extraIndices)) {
                        
                        //Extra Match Block인 경우 상태 Extra로 변경 및 정보 구성
                        List<BlockModel> matchs = ListPool<BlockModel>.Get();
                        for(int j = 0; j < extraIndices.Count; j++) {
                            matchs.Add(blocks[extraIndices[j]]);
                            blocks[extraIndices[j]].SetBlockState(BlockState.EXTRA);
                        }

                        /**
                         *  @Todo : 현재는 Convert Type을 일반 Block으로 고정함
                         *          이 후 특수 블럭으로 변경 필요
                         */
                        extraMatchTable.Add((BlockType.NORMAL, matchs));
                        break;
                    }
                }
            }

            return extraMatchTable;
        }


        /**
         *  @brief  노드 기반의 Extra Patten 정보 완전 탐색을 위해 재귀호출을 사용
         *  @param  targetBlock : 기준이 될 Block
         *  @param  pattern : pattern info
         *  @param  extraIndices : match된 block indices
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
         *  @brief  extra match 이벤트 수행
         *  @detail ShakeScale->Merge->Convert Block
         *  @param  extraMatchs : List<(convert block type, merge block list)> 
         *  @param  targetBlocks : merge 될 targetBlocks
         */
        public async UniTask RunExtraPatternEvent(List<(BlockType, List<BlockModel>)> extraMatchs, BlockModel[] targetBlocks)
        {
            List<UniTask> extraTask = UnityEngine.Pool.ListPool<UniTask>.Get();

            for(int i = 0; i < extraMatchs.Count; i++) {
                List<BlockModel> extraBlocks = extraMatchs[i].Item2;

                //중심 점이될 블럭 정하기
                //match block list에 target block이 있는 경우 해당 block을 mainblock으로
                int mainBlockIdx = Mathf.Max(extraBlocks.IndexOf(targetBlocks[0]), extraBlocks.IndexOf(targetBlocks[1]));
                
                //없는 경우 지정된 root block을 main으로 결정, root block은 가장 마지막 block
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
