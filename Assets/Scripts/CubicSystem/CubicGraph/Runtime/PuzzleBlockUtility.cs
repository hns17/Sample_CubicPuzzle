using System.Collections.Generic;
using UnityEngine;


/**
 *  @brief  Runtime Project에 공유될 정보들
 */
namespace CubicSystem.CubicPuzzle
{
    public enum CellType { EMPTY, NORMAL }
    public enum BlockType { EMPTY, NORMAL, ITEM }
    public enum MatchColorType { RED, BLACK, YELLOW, GREEN, BLUE, PINK, VIOLETE, NONE }
    public enum BoardQuestType { DESTROY_COUNT, SCORE, NO_MATCH, NONE }


    public static class PuzzleBlockUtility
    {
        private const int alpha = 160;

        //Block Color Table
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
    }

}

