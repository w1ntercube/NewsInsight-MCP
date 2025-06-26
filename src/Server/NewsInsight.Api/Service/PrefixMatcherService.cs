using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NewsInsight.Api.Services
{
    public class PrefixMatcherService : IPrefixMatcherService
    {
        public bool IsInitialized() => _isInitialized;
        private readonly ILogger<PrefixMatcherService> _logger;
        private ManagedBridge.PrefixMatcher _matcher;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<string, List<string>> _cache = new();
        private bool _isInitialized = false;

        public PrefixMatcherService(ILogger<PrefixMatcherService> logger)
        {
            _logger = logger;
            _matcher = new ManagedBridge.PrefixMatcher();
        }

        public void InitializeMatcher(IEnumerable<string> words)
        {
            _lock.EnterWriteLock();
            try
            {
                _logger.LogInformation("初始化前缀匹配器...");
                _logger.LogInformation($"将添加 {words.Count()} 个类别");

                // 重置匹配器
                _matcher = new ManagedBridge.PrefixMatcher();

                foreach (var word in words.Distinct())
                {
                    _matcher.AddWord(word);
                    _logger.LogDebug($"添加类别: {word}");
                }

                _isInitialized = true;
                _cache.Clear();
                _logger.LogInformation("前缀匹配器初始化完成");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<string> MatchPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                _logger.LogInformation("匹配请求: 空前缀");
                return Enumerable.Empty<string>();
            }

            if (!_isInitialized)
            {
                _logger.LogWarning("匹配请求: 匹配器未初始化");
                return Enumerable.Empty<string>();
            }

            // 尝试从缓存获取
            if (_cache.TryGetValue(prefix, out var cachedResult))
            {
                _logger.LogInformation($"缓存命中: 前缀 '{prefix}' -> {cachedResult.Count} 个结果");
                return cachedResult;
            }

            _lock.EnterReadLock();
            try
            {
                _logger.LogInformation($"匹配请求: 前缀 '{prefix}'");

                // 调用原生匹配器
                var matches = _matcher.GetMatches(prefix)
                    .Cast<string>()
                    .OrderBy(word => word)
                    .ToList();

                _logger.LogInformation($"匹配结果: 前缀 '{prefix}' -> {matches.Count} 个结果");

                // 记录匹配结果用于调试
                if (matches.Any())
                {
                    _logger.LogDebug($"匹配结果: {string.Join(", ", matches)}");
                }

                // 缓存结果（特别是短前缀）
                if (prefix.Length <= 3)
                {
                    _cache[prefix] = matches;
                }

                return matches;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}