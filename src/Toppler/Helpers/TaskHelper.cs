﻿using System.Threading.Tasks;

namespace Toppler.Helpers
{
    internal class TaskHelper
    {
        public static readonly Task<bool> AlwaysTrue = MakeTask(true);
        public static readonly Task<bool> AlwaysFalse = MakeTask(false);
        public static readonly Task<object> Empty = MakeTask<object>(null);

        private static Task<T> MakeTask<T>(T value)
        {
            return FromResult<T>(value);
        }

        public static Task<T> FromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}
