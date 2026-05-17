using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sanmon.Utility.UnityExtension
{
    public class RectNavSet : INavSet
    {
        /// <summary>
        /// 坐标 x : 0 → ∞，坐标 y : 0 ↓ ∞
        /// </summary>
        /// <param name="root">根节点</param>
        /// <param name="rowCount">行数，小于等于0时表示无限</param>
        /// <param name="columnCount">列数，小于等于0时表示无限</param>
        public RectNavSet(Transform root, int rowCount, int columnCount)
        {
            var items = root.GetComponentsInChildren<ICanNavigated>().ToList();
            Init(items, rowCount, columnCount);
        }

        /// <summary>
        /// 坐标 x : 0 → ∞，坐标 y : 0 ↓ ∞
        /// </summary>
        /// <param name="items">按坐标顺序排序的对象，先x，再y</param>
        /// <param name="rowCount">行数，小于等于0时表示无限</param>
        /// <param name="columnCount">列数，小于等于0时表示无限</param>
        public RectNavSet(List<ICanNavigated> items, int rowCount, int columnCount)
        {
            Init(items, rowCount, columnCount);
        }

        private readonly List<ICanNavigated> mFocusItems = new();
        
        public int CurIndex { get; private set; }

        private ICanNavigated CurFocus => CurIndex < 0 || CurIndex >= mFocusItems.Count ? null : mFocusItems[CurIndex];
        
        public int RowCount { get; private set; }

        public int ColumnCount { get; private set; }
        
        private void Init(List<ICanNavigated> items, int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            CurIndex = -1;

            CurFocus?.OnUnfocused();
            mFocusItems.Clear();
            mFocusItems.AddRange(items);

            for (var i = 0; i < items.Count; i++)
            {
                mFocusItems[i].Index = i;
                mFocusItems[i].NavSet = this;
            }
            
            ResetFocus(true);
        }

        public void ResetFocus(bool focusFirst)
        {
            CurFocus?.OnUnfocused();

            CurIndex = -1;

            if (focusFirst)
            {
                for (var i = 0; i < mFocusItems.Count; i++)
                {
                    if (mFocusItems[i].Interactable)
                    {
                        CurIndex = i;
                        CurFocus?.OnFocused();
                        break;
                    }
                }
            }
        }
        
        public void SetShow(bool show)
        {
            foreach (var nav in mFocusItems)
            {
                nav.SetShow(show);
            }
        }

        public void Focus(int index)
        {
            Move(index);
        }

        public void Unfocus(int index)
        {
            if (index == CurIndex)
            {
                CurFocus.OnUnfocused();
                CurIndex = -1;
            }
        }

        public bool MoveLeft()
        {
            if (CurFocus == null)
            {
                if (mFocusItems.Count > 0)
                {
                    ResetFocus(true);
                    return true;
                }

                return false;
            }

            var find = false;
            var next = CurIndex - 1;
            next = next < 0 ? mFocusItems.Count - 1 : next;

            for (var i = next; i >= 0; i--)
            {
                if (mFocusItems[i].Interactable)
                {
                    next = i;
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                for (var i = mFocusItems.Count - 1; i >= CurIndex; i--)
                {
                    if (mFocusItems[i].Interactable)
                    {
                        next = i;
                        find = true;
                        break;
                    }
                }
            }

            if (!find)
                return false;

            Move(next);
            return true;
        }

        public bool MoveRight()
        {
            if (CurFocus == null)
            {
                if (mFocusItems.Count > 0 && mFocusItems.Any(x => x.Interactable))
                {
                    ResetFocus(true);
                    return true;
                }

                return false;
            }

            var find = false;
            var next = CurIndex + 1;
            next = next >= mFocusItems.Count ? 0 : next;

            for (var i = next; i < mFocusItems.Count; i++)
            {
                if (mFocusItems[i].Interactable)
                {
                    next = i;
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                for (var i = 0; i <= CurIndex; i++)
                {
                    next = i;
                    find = true;
                    break;
                }
            }

            if (!find)
                return false;

            Move(next);
            return true;
        }

        public bool MoveUp()
        {
            if (CurFocus == null)
            {
                if (mFocusItems.Count > 0)
                {
                    ResetFocus(true);
                    return true;
                }

                return false;
            }

            var next = CurIndex - ColumnCount;
            next = next < 0 ? ColumnCount * RowCount + next : next;
            next = next >= mFocusItems.Count ? mFocusItems.Count - 1 : next;

            if (mFocusItems[next].Interactable)
            {
                Move(next);
                return true;
            }

            if (!mFocusItems.Any(x => x.Interactable))
                return false;

            Move(next);
            return MoveLeft();
        }

        public bool MoveDown()
        {
            if (CurFocus == null)
            {
                if (mFocusItems.Count > 0)
                {
                    ResetFocus(true);
                    return true;
                }

                return false;
            }

            var next = CurIndex + ColumnCount;
            next = next >= ColumnCount * RowCount ? next - ColumnCount * RowCount : next;
            next = next < 0 ? next - mFocusItems.Count : next;
            next = next >= mFocusItems.Count ? mFocusItems.Count - 1 : next;

            if (mFocusItems[next].Interactable)
            {
                Move(next);
                return true;
            }

            if (!mFocusItems.Any(x => x.Interactable))
                return false;

            Move(next);
            return MoveRight();
        }

        public bool MoveIn(int index)
        {
            if (index < 0 || index >= mFocusItems.Count) 
                return false;
            
            Move(index);
            return true;
        }

        public bool MoveOut()
        {
            CurFocus?.OnUnfocused();
            CurIndex = -1;
            return true;
        }

        private void Move(int nextIndex)
        {
            CurFocus?.OnUnfocused();
            CurIndex = nextIndex;
            CurFocus?.OnFocused();
        }
    }
}