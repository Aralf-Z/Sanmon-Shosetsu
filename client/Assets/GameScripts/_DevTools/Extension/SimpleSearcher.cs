using System;
using System.Collections.Generic;
using System.Linq;

namespace GameConsole.Extension
{
    public class SimpleSearcher
    {
        public const int HISTORY_COUNT = 32;
        public readonly char[] splitChars = { ' ' };

        public string LastQuery => mHistoryIndex < 0 || mHistoryIndex >= mHistory.Count ? null : mHistory[mHistoryIndex];

        public string PreviousQuery
        {
            get
            {
                if(LastQuery == null ) return null;
                var pre = mHistoryIndex - 1;
                if(pre < 0) pre = mHistory.Count - 1;
                return mHistory[pre];
            }
        }

        public string NextQuery
        {
            get
            {
                if(LastQuery == null ) return null;
                var next = mHistoryIndex + 1;
                if(next >= mHistory.Count) next = 0;
                return mHistory[next];
            }
        }
        
        // 缓存上一次的查询和结果
        private readonly List<string> mHistory = new(HISTORY_COUNT);
        private List<string> mLastResult = new();
        private int mHistoryIndex = -1;
        
        private readonly List<string> mItems;
        
        public SimpleSearcher(List<string> items)
        {
            mItems = items;
        }

        /// <summary>
        /// 查询包含关键字的字符串
        /// </summary>
        /// <param name="query">空格分隔的关键字</param>
        /// <returns>匹配结果</returns>
        public List<string> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return mItems;
            
            query = query.Trim();

            if(query == LastQuery) return mLastResult;
            
            // 如果本次查询是上一次查询的前缀，可以在上一次结果基础上过滤
            var isIncremental = LastQuery != null && query.StartsWith(LastQuery, StringComparison.OrdinalIgnoreCase);
            var source = isIncremental ? mLastResult : mItems;
            var keywords = query.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            mLastResult = source.Where(item => keywords.All(k => item.Contains(k, StringComparison.OrdinalIgnoreCase))).ToList();

            // 按第一个关键字出现位置排序（可选）
            mLastResult.Sort((a, b) =>
            {
                var indexA = a.IndexOf(keywords[0], StringComparison.OrdinalIgnoreCase);
                var indexB = b.IndexOf(keywords[0], StringComparison.OrdinalIgnoreCase);
                return indexA.CompareTo(indexB);
            });

            // 更新缓存
            if (mHistoryIndex == -1 || mHistory.Count < HISTORY_COUNT)
            {
                mHistoryIndex++;
                mHistory.Add(query);
            }
            else if(mHistoryIndex == mHistory.Count - 1)
            {
                mHistoryIndex = 0;
                mHistory[mHistoryIndex] = query;
            }
            else
            {
                mHistoryIndex++;
                mHistory[mHistoryIndex] = query;
            }

            return mLastResult;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void ClearCache()
        {
            mHistory.Clear();
            mLastResult.Clear();
            mHistoryIndex = -1;
        }
    }
}