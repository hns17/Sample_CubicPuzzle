using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    public enum BoardState { READY, NO_MORE_MATCH, MATCH_EVENT, INITIALIZE, DESTROYED, CLEAR, GAME_OVER }
    public enum CellState { EMPTY, NORMAL, NONE, DESTROYED }
    public enum BlockState { START, EMPTY, NORMAL, MATCH, CLEAR, FILL_WAIT, DESTROYED, NONE }
    public enum BlockNeighType {START, LEFT_UP = 0, UP, RIGHT_UP, RIGHT_DOWN, DOWN, LEFT_DOWN, NONE }
}

