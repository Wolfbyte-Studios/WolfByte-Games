using System;

namespace terrainslicer.utils
{
    public static class CommonUtils
    {
        public static void VisitAllGridCells(int sideSize, Action<CellContext> cellAction)
        {
            var cellContext = new CellContext();
            for (var y = 0; y < sideSize; y++)
            {
                for (var x = 0; x < sideSize; x++)
                {
                    cellAction(cellContext.Update(x, y));
                }
            }
        }
    }
    
    public class CellContext
    {
        public int x, y;

        public CellContext Update(int x, int y)
        {
            this.x = x;
            this.y = y;
            return this;
        }
    }
}