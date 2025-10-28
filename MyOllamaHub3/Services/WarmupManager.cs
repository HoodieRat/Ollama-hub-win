using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyOllamaHub3.Services
{
    internal enum WarmupState
    {
        Hidden,
        Warming,
        Ready,
        Failed
    }

    internal sealed class WarmupStateChangedEventArgs : EventArgs
    {
        private WarmupStateChangedEventArgs(string? model, WarmupState state, bool fromCache, Exception? error)
        {
            Model = model;
            State = state;
            FromCache = fromCache;
            Error = error;
        }

        public string? Model { get; }
        public WarmupState State { get; }
        public bool FromCache { get; }
        public Exception? Error { get; }

        public static WarmupStateChangedEventArgs Hidden()
            => new WarmupStateChangedEventArgs(null, WarmupState.Hidden, fromCache: false, error: null);

        public static WarmupStateChangedEventArgs Warming(string model)
            => new WarmupStateChangedEventArgs(model, WarmupState.Warming, fromCache: false, error: null);

        public static WarmupStateChangedEventArgs Ready(string model, bool fromCache)
            => new WarmupStateChangedEventArgs(model, WarmupState.Ready, fromCache, error: null);

        public static WarmupStateChangedEventArgs Failed(string model, Exception error)
            => new WarmupStateChangedEventArgs(model, WarmupState.Failed, fromCache: false, error: error);
    }

    internal sealed class WarmupManager : IDisposable
    {
        private readonly Func<string, CancellationToken, Task> _warmupAction;
        private readonly HashSet<string> _warmedModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _gate = new object();
        private CancellationTokenSource? _activeCts;
        private string? _activeModel;
    private HashSet<string>? _activeAliases;

        public WarmupManager(Func<string, CancellationToken, Task> warmupAction)
        {
            _warmupAction = warmupAction ?? throw new ArgumentNullException(nameof(warmupAction));
        }

        public event EventHandler<WarmupStateChangedEventArgs>? StateChanged;
        public event EventHandler<WarmupStateChangedEventArgs>? WarmupReady;
        public event EventHandler<WarmupStateChangedEventArgs>? WarmupFailed;

        public void RequestWarmup(string? model, IEnumerable<string>? aliases = null)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                CancelActiveWarmup();
                OnStateChanged(WarmupStateChangedEventArgs.Hidden());
                return;
            }

            var trimmed = model.Trim();
            var aliasArray = (aliases ?? Array.Empty<string>())
                .Where(static alias => !string.IsNullOrWhiteSpace(alias))
                .Select(static alias => alias.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            bool warmedFromCache;
            bool joinExisting = false;
            CancellationTokenSource? localCts = null;
            HashSet<string>? warmupAliases = null;

            lock (_gate)
            {
                warmedFromCache = _warmedModels.Contains(trimmed) || aliasArray.Any(_warmedModels.Contains);
                if (warmedFromCache)
                {
                    _warmedModels.Add(trimmed);
                    foreach (var alias in aliasArray)
                        _warmedModels.Add(alias);
                }
                else
                {
                    if (_activeCts != null && _activeModel != null && string.Equals(_activeModel, trimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        joinExisting = true;
                        _activeAliases ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var alias in aliasArray)
                            _activeAliases.Add(alias);
                        warmupAliases = _activeAliases;
                    }
                    else
                    {
                        CancelActiveWarmup_NoLock();
                        var cts = new CancellationTokenSource();
                        _activeCts = cts;
                        _activeModel = trimmed;
                        _activeAliases = new HashSet<string>(aliasArray, StringComparer.OrdinalIgnoreCase);
                        warmupAliases = _activeAliases;
                        localCts = cts;
                    }
                }
            }

            if (warmedFromCache)
            {
                OnStateChanged(WarmupStateChangedEventArgs.Ready(trimmed, fromCache: true));
                return;
            }

            if (joinExisting || localCts == null)
                return;

            OnStateChanged(WarmupStateChangedEventArgs.Warming(trimmed));

            var token = localCts.Token;
            Task.Run(async () =>
            {
                try
                {
                    await _warmupAction(trimmed, token).ConfigureAwait(false);
                    if (token.IsCancellationRequested)
                        return;

                    lock (_gate)
                    {
                        _warmedModels.Add(trimmed);
                        if (warmupAliases != null)
                        {
                            foreach (var alias in warmupAliases)
                                _warmedModels.Add(alias);
                        }

                        if (ReferenceEquals(_activeCts, localCts))
                        {
                            _activeCts = null;
                            _activeModel = null;
                            if (ReferenceEquals(_activeAliases, warmupAliases))
                                _activeAliases = null;
                        }
                    }

                    OnStateChanged(WarmupStateChangedEventArgs.Ready(trimmed, fromCache: false));
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    // Expected when a new warmup request supersedes the current one.
                }
                catch (Exception ex)
                {
                    lock (_gate)
                    {
                        if (ReferenceEquals(_activeCts, localCts))
                        {
                            _activeCts = null;
                            _activeModel = null;
                            if (ReferenceEquals(_activeAliases, warmupAliases))
                                _activeAliases = null;
                        }
                    }

                    OnStateChanged(WarmupStateChangedEventArgs.Failed(trimmed, ex));
                }
                finally
                {
                    localCts.Dispose();
                }
            });
        }

        public bool IsWarmed(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return false;

            lock (_gate)
            {
                return _warmedModels.Contains(model.Trim());
            }
        }

        public void MarkAsWarmed(params string[] models)
        {
            if (models == null || models.Length == 0)
                return;

            lock (_gate)
            {
                foreach (var model in models)
                {
                    if (!string.IsNullOrWhiteSpace(model))
                        _warmedModels.Add(model.Trim());
                }
            }
        }

        public void CancelActiveWarmup()
        {
            CancellationTokenSource? previous;
            lock (_gate)
            {
                previous = _activeCts;
                _activeCts = null;
                _activeModel = null;
                _activeAliases = null;
            }

            if (previous == null)
                return;

            try
            {
                previous.Cancel();
            }
            finally
            {
                previous.Dispose();
            }
        }

        public void Dispose()
        {
            CancelActiveWarmup();
        }

        private void CancelActiveWarmup_NoLock()
        {
            var previous = _activeCts;
            if (previous == null)
                return;

            _activeCts = null;
            _activeModel = null;
            _activeAliases = null;

            try
            {
                previous.Cancel();
            }
            finally
            {
                previous.Dispose();
            }
        }

        private void OnStateChanged(WarmupStateChangedEventArgs args)
        {
            StateChanged?.Invoke(this, args);

            switch (args.State)
            {
                case WarmupState.Ready:
                    WarmupReady?.Invoke(this, args);
                    break;
                case WarmupState.Failed:
                    WarmupFailed?.Invoke(this, args);
                    break;
            }
        }
    }
}
