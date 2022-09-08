using System.Collections.Generic;
using UnityEngine;


/**
 *  @brief  Runtime Project에 공유될 정보들
 */
namespace CubicSystem.CubicPuzzle
{
    public static class CubicPuzzleUtility
    {
        private readonly static Dictionary<BoardType, List<BlockNeighType>> NeighTypeTable
            = new Dictionary<BoardType, List<BlockNeighType>>() {
                { BoardType.HEX, new List<BlockNeighType>(){
                    BlockNeighType.LEFT_UP, BlockNeighType.UP, BlockNeighType.RIGHT_UP,
                    BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN
                }},
                { BoardType.SQUARE, new List<BlockNeighType>(){
                    BlockNeighType.LEFT, BlockNeighType.UP, BlockNeighType.RIGHT, BlockNeighType.DOWN
                }},
            };

        //Block Color Table
        private const int alpha = 160;
        private readonly static Dictionary<MatchColorType, Color> ColorTable = new Dictionary<MatchColorType, Color>{
            { MatchColorType.RED, new Color32(255, 0, 0, alpha) },
            { MatchColorType.BLACK, new Color32(20, 20, 20, alpha) },
            { MatchColorType.YELLOW, new Color32(255, 255, 0, alpha) },
            { MatchColorType.GREEN, new Color32(0, 255, 0, alpha) },
            { MatchColorType.BLUE, new Color32(0, 0, 255, alpha) },
            { MatchColorType.PINK, new Color32(255, 204, 255, alpha) },
            { MatchColorType.VIOLETE, new Color32(100, 0, 255, alpha) },
        };

        public static Color GetMatchColor(MatchColorType colorType)
        {
            if(!ColorTable.ContainsKey(colorType)) {
                return Color.white;
            }
            return ColorTable[colorType];
        }


        public static List<BlockNeighType> GetBlockNeighTypes(BoardType boardType)
        {
            return NeighTypeTable[boardType];
        }

        public static int GetNeighIndex(BoardType boardType, int col, int row, int targetIdx, BlockNeighType neighType)
        {
            if(boardType == BoardType.HEX) {
                return GetNeighIndex_Hex(col, row, targetIdx, neighType);
            }
            else if(boardType == BoardType.SQUARE) {
                return GetNeighIndex_Square(col, row, targetIdx, neighType);
            }
            return -1;
        }

        private static int GetNeighIndex_Hex(int col, int row, int targetIndex, BlockNeighType neighType)
        {
            int res = -1;
            int mod = targetIndex % col;
            int objectCount = col * row;

            //Up Block
            if(neighType == BlockNeighType.UP) {
                res = targetIndex - col;
            }
            //Down Block
            else if(neighType == BlockNeighType.DOWN) {
                res = targetIndex + col;
            }
            //Left Side(LeftUp, LeftDown)
            else if(neighType == BlockNeighType.LEFT_UP || neighType == BlockNeighType.LEFT_DOWN) {
                if(mod != 0) {
                    res = targetIndex - 1;

                    //Left Up Block
                    if(neighType == BlockNeighType.LEFT_UP && mod % 2 == 0) {
                        res -= col;
                    }
                    //Left Down Block
                    else if(neighType == BlockNeighType.LEFT_DOWN && mod % 2 != 0) {
                        res += col;
                    }
                }
            }
            //Right Side(RightUp, RightDown)
            else if(neighType == BlockNeighType.RIGHT_UP || neighType == BlockNeighType.RIGHT_DOWN) {
                if((targetIndex + 1) % col != 0) {
                    res = targetIndex + 1;

                    //Right Down Block
                    if(neighType == BlockNeighType.RIGHT_DOWN && mod % 2 != 0) {
                        res += col;
                    }
                    //Right Up Block
                    else if(neighType == BlockNeighType.RIGHT_UP && mod % 2 == 0) {
                        res -= col;
                    }
                }
            }

            //이웃 Block을 찾지 못한 경우
            if(res >= objectCount || res < 0) {
                res = -1;
            }
            return res;
        }

        private static int GetNeighIndex_Square(int col, int row, int targetIndex, BlockNeighType neighType)
        {
            int res = -1;
            int mod = targetIndex % col;
            int objectCount = col * row;

            //Up Block
            if(neighType == BlockNeighType.UP) {
                res = targetIndex - col;
            }
            //Down Block
            else if(neighType == BlockNeighType.DOWN) {
                res = targetIndex + col;
            }
            else if(neighType == BlockNeighType.RIGHT) {
                res = (targetIndex + 1) % col != 0 ? targetIndex + 1 : -1;
            }
            else if(neighType == BlockNeighType.LEFT) {
                res = targetIndex % col != 0 ? targetIndex - 1 : -1;
            }


            //이웃 Block을 찾지 못한 경우
            if(res >= objectCount || res < 0) {
                res = -1;
            }
            return res;
        }
    }

}

