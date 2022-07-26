using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace CubicSystem.CubicPuzzle
{
    public interface IDropAndFillEvent
    {
        public UniTask StartDropAndFill();
    }
}
