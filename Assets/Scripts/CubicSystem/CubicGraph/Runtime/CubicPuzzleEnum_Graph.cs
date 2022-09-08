namespace CubicSystem.CubicPuzzle
{
    public enum BoardType { HEX = 0, SQUARE }
    public enum CellType { NORMAL, NONE }
    public enum CellStyle { HEX, SQUARE, NONE }
    public enum BlockType { NORMAL, ITEM, NONE }
    public enum MatchColorType { RED, BLACK, YELLOW, GREEN, BLUE, PINK, VIOLETE, NONE }
    public enum BoardQuestType { DESTROY_COUNT, SCORE, NO_MATCH, NONE }

    public enum BlockNeighType { START, LEFT_UP = 0, UP, RIGHT_UP, RIGHT, RIGHT_DOWN, DOWN, LEFT_DOWN, LEFT, NONE, END }
}

