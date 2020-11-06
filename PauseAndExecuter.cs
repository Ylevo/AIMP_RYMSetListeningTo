using System;
using System.Threading.Tasks;

namespace AIMP_RYMSetListeningTo
{
    public static class PauseAndExecuter
    {
        public static System.Threading.CancellationTokenSource tokenSource = null;
        public static async void Execute(Action action, int timeoutInMilliseconds)
        {
            try
            {
                tokenSource = new System.Threading.CancellationTokenSource();
                await Task.Delay(timeoutInMilliseconds, tokenSource.Token);
                tokenSource.Token.ThrowIfCancellationRequested();
                action();
                tokenSource = null;
            }
            catch (OperationCanceledException) { tokenSource = null; }
        }

        public static void AbortLast()
        {
            tokenSource?.Cancel();
        }
    }
}
