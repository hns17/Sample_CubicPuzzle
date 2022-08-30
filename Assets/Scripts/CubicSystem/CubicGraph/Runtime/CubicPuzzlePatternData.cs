using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubicSystem.CubicPuzzle
{
    public class CubicPuzzlePatternData : ScriptableObject
    {
        public SerializeDictionary<BoardType, ListContainer<PatternNode>> patternData;
    }

    [Serializable]
    public class PatternNode
    {
        public BlockNeighType neighType;
        [SerializeReference] public List<PatternNode> child;

        public PatternNode(BlockNeighType neighType)
        {
            this.neighType = neighType;
            child = new List<PatternNode>();
        }

    }
}

