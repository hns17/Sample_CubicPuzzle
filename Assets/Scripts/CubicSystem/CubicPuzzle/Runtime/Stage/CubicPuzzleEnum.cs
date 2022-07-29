namespace CubicSystem.CubicPuzzle
{
    public enum BoardState { READY, NO_MORE_MATCH, MATCH_EVENT, INITIALIZE, DESTROYED, CLEAR, GAME_OVER }
    public enum CellState { EMPTY, NORMAL, DESTROYED }
    public enum BlockState { START, EMPTY = 0, NORMAL, MATCH, CLEAR, FILL_WAIT, DESTROYED, END }
    public enum BlockNeighType {START, LEFT_UP = 0, UP, RIGHT_UP, RIGHT_DOWN, DOWN, LEFT_DOWN, NONE, END }
}

