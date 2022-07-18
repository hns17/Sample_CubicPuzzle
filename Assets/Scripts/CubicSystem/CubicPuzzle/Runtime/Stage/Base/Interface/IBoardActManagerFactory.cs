using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    public interface IBoardActManagerFactory
    {
        public BoardActManager Create(BoardModel boardModel);
    }
}
