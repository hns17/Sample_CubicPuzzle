namespace CubicSystem.CubicPuzzle
{
    public enum BoardState { READY, NO_MORE_MATCH, MATCH_EVENT, INITIALIZE, DESTROYED, CLEAR, GAME_OVER }
    public enum CellState { EMPTY, NORMAL, DESTROYED }
    public enum BlockState { START, EMPTY = 0, NORMAL, MATCH, CLEAR, FILL_WAIT, DESTROYED, EXTRA, END }
}

