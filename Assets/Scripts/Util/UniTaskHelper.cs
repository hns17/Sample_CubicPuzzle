using Cysharp.Threading.Tasks;
using System;


public static class UniTaskHelper
{
    public static Func<T, R> Func<T, R>(Func<T, R> asyncAction)
    {
        return (t) => asyncAction(t);
    }


    public static Action<T> Action<T>(Func<T, UniTaskVoid> asyncAction)
    {
        return (t) => asyncAction(t).Forget();
    }
}
