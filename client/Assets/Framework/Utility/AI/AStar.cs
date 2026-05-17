using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanmon.Utility.AI
{
    public partial class AStar
    {
        private readonly List<Node> mProcessingSet = new List<Node>();
        private readonly HashSet<Node> mProcessedSet = new HashSet<Node>();
        
        /// <summary>
        /// 返回节点列表，从终点返回到起点
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IEnumerable<Node> FindPath(IGrid grid, Node start, Node end)
        {
            if (start == null || end == null)
                return  Enumerable.Empty<Node>();
            if (!start.Walkable || !end.Walkable)
                return  Enumerable.Empty<Node>();
            
            //重置
            mProcessingSet.Clear();
            mProcessedSet.Clear();
            mProcessingSet.Add(start);

            // 初始化起点
            start.Parent = null;
            start.GCost = 0;
            start.HCost = CalculateHeuristic(start, end);

            while (mProcessingSet.Count > 0)
            {
                // 获取FCost最小的节点
                var current = mProcessingSet[0];
                for (var i = 1; i < mProcessingSet.Count; i++)
                {
                    if (mProcessingSet[i].FCost < current.FCost || (mProcessingSet[i].FCost == current.FCost && mProcessingSet[i].HCost < current.HCost))
                    {
                        current = mProcessingSet[i];
                    }
                }

                // 如果到达终点，重构路径
                if (current == end)
                {
                    return ReconstructPath(current);
                }

                mProcessingSet.Remove(current);
                mProcessedSet.Add(current);

                // 检查所有邻居
                foreach (var neighbor in grid.Neighbours(current))
                {
                    if(neighbor is not { Walkable: true })
                        continue;
                    if (mProcessedSet.Contains(neighbor))
                        continue;

                    var costToNeighbor = current.GCost + neighbor.Cost;

                    if (!mProcessingSet.Contains(neighbor) || costToNeighbor < neighbor.GCost)
                    {
                        neighbor.Parent = current;
                        neighbor.GCost = costToNeighbor;
                        neighbor.HCost = CalculateHeuristic(neighbor, end);
                    }
                    
                    if (!mProcessingSet.Contains(neighbor))
                    {
                        mProcessingSet.Add(neighbor);
                    }
                }
            }

            // 没有找到路径
            return  Enumerable.Empty<Node>();
        }

        // 启发式函数（曼哈顿距离）
        private static int CalculateHeuristic(Node a, Node b)
        {
            return (int)(MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y));
        }

        // 返回路径
        private static IEnumerable<Node> ReconstructPath(Node endNode)
        {
            var current = endNode;
            
            yield return current;
            
            while (null != current)
            {
                current = current.Parent;
                if(null != current)
                    yield return current;
            }
        }
        
        public class Node
        {
            public int X { get; }
            public int Y { get; }
            public bool Walkable => mCostFunction?.Invoke(X, Y).walkable ?? mWalkable;
            public int Cost => mCostFunction?.Invoke(X, Y).cost ?? mCost;
            public int GCost { get; set; } // 从起点到当前节点的代价
            public int HCost { get; set; } // 启发式代价（到终点的估计代价）
            public int FCost => GCost + HCost; // 总代价
            public Node Parent { get; set; } // 路径中的父节点
            
            private readonly Func<int, int, (bool walkable, int cost)> mCostFunction;
            private readonly int mCost;
            private readonly bool mWalkable;
            
            public Node(int x, int y, Func<int, int, (bool walkable, int cost)> costFunction)
            {
                X = x;
                Y = y;
                mCostFunction = costFunction;
            }

            public Node(int x, int y, int cost = 1, bool walkable = true)
            {
                X = x;
                Y = y;
                mCost = cost;
                mWalkable = walkable;
            }
        }

        public interface IGrid
        {
            Node this[int x, int y] { get; set; }
            int Width { get; }
            int Height { get; }
            IEnumerable<Node> Neighbours(Node node);
        }
    }
}