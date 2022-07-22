using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    public interface IOneTouchBlockEvent
    {
        public UniTask DoTouchAction(BlockModel target);
    }

}
