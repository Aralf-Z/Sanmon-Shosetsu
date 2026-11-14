using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameConsole.Extension
{
    /// <summary> 将命令异步返回值观察一次, 宏停止只取消等待而不终止业务操作. </summary>
    internal static class MacroAwaiter
    {
        /// <summary> 返回可重复观察的 Task, 未等待的调用也会报告异步异常. </summary>
        internal static Task ObserveAsync(object value, MonoBehaviour owner, Action<string> output)
        {
            var task = CompleteAsync(value, owner);
            ReportAsync(task, output).Forget();
            return task;
        }

        // 支持泛型 UniTask 的公开 Awaiter, 不反射执行用户业务方法.
        private static async Task CompleteAsync(object value, MonoBehaviour owner)
        {
            if (value == null) return;
            if (value is Task task) { await task; return; }
            if (value is UniTask uniTask) { await uniTask; return; }
            if (value is IEnumerator routine)
            {
                var completion = new TaskCompletionSource<bool>();
                owner.StartCoroutine(GuardRoutine(routine, completion));
                await completion.Task;
                return;
            }
            var type = value.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(UniTask<>)) return;
            var awaiter = type.GetMethod("GetAwaiter", BindingFlags.Instance | BindingFlags.Public)?.Invoke(value, null);
            if (awaiter == null) throw new InvalidOperationException(ToolkitLocalization.Get("ui.MacroAwaiter.CompleteAsync"));
            var awaiterType = awaiter.GetType();
            var completed = awaiterType.GetProperty("IsCompleted");
            var getResult = awaiterType.GetMethod("GetResult");
            while (!(bool)completed.GetValue(awaiter)) await UniTask.Yield(owner.GetCancellationTokenOnDestroy());
            getResult.Invoke(awaiter, null);
        }

        // 观察异常不改变原 Task 的完成状态, WaitFor 仍会收到同一异常.
        private static async UniTask ReportAsync(Task task, Action<string> output)
        {
            try { await task; }
            catch (OperationCanceledException) { }
            catch (Exception exception) { output(exception.GetBaseException().Message); }
        }

        // 展开嵌套迭代器并捕获 MoveNext/Current 异常, 其他 yield 对象交给 Unity.
        private static IEnumerator GuardRoutine(IEnumerator root, TaskCompletionSource<bool> completion)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(root);
            try
            {
                while (stack.Count > 0)
                {
                    object current = null;
                    var moved = false;
                    Exception failure = null;
                    try
                    {
                        moved = stack.Peek().MoveNext();
                        if (moved) current = stack.Peek().Current;
                    }
                    catch (Exception exception) { failure = exception; }
                    if (failure != null) { completion.TrySetException(failure); yield break; }
                    if (!moved) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                    if (current is IEnumerator child) stack.Push(child);
                    else yield return current;
                }
                completion.TrySetResult(true);
            }
            finally
            {
                while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
                completion.TrySetCanceled();
            }
        }
    }
}
