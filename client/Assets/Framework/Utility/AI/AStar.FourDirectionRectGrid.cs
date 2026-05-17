using System.Collections.Generic;

namespace Sanmon.Utility.AI
{
    public partial class AStar
    {
        public class FourDirectionRectGrid: IGrid
        {
            private Node[,] Nodes { get; set; }
            private readonly int[] mDx = { -1, 1, 0, 0 };
            private readonly int[] mDy = { 0, 0, -1, 1 };
            
            public FourDirectionRectGrid(int width, int height)
            {
                Width = width;
                Height = height;
                Nodes = new Node[width, height];
            }
            
            public Node this[int x, int y]
            {
                get => (x >= 0 && x < Width && y >= 0 && y < Height) ? Nodes[x, y] : null;
                set => Nodes[x, y] = value;
            }

            public int Width { get;}
            public int Height { get;}
            
            public IEnumerable<Node> Neighbours(Node node)
            {
                for (var i = 0; i < 4; i++)
                {
                    var newX = node.X + mDx[i];
                    var newY = node.Y + mDy[i];
                    var neighbor = this[newX, newY];
                    yield return neighbor;
                }
            }
        }
    }
}