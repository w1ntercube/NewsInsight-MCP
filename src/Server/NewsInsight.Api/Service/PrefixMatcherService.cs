using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NewsInsight.Api.Services
{
    public class PrefixMatcherService : IPrefixMatcherService
    {
        public bool IsCategoryInitialized() => _isCategoryInitialized;
        public bool IsTopicInitialized() => _isTopicInitialized;
        private readonly ILogger<PrefixMatcherService> _logger;
        private ManagedBridge.PrefixMatcher _categoryMatcher;
        private ManagedBridge.PrefixMatcher _topicMatcher;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<string, List<string>> _categoryCache = new();
        private readonly ConcurrentDictionary<string, List<string>> _topicCache = new();
        private bool _isCategoryInitialized = false;
        private bool _isTopicInitialized = false;

        public PrefixMatcherService(ILogger<PrefixMatcherService> logger)
        {
            _logger = logger;
            _categoryMatcher = new ManagedBridge.PrefixMatcher();
            _topicMatcher = new ManagedBridge.PrefixMatcher();
        }


        public void InitializeCategoryMatcher(IEnumerable<string> categories)
        {
            _lock.EnterWriteLock();
            try
            {
                _logger.LogInformation("初始化类别前缀匹配器...");
                _categoryMatcher = new ManagedBridge.PrefixMatcher();

                foreach (var category in categories.Distinct())
                {
                    _categoryMatcher.AddWord(category);
                }

                _isCategoryInitialized = true;
                _categoryCache.Clear();
                _logger.LogInformation("类别前缀匹配器初始化完成");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void InitializeTopicMatcher(IEnumerable<string> topics)
        {
            _lock.EnterWriteLock();
            try
            {
                _logger.LogInformation("初始化主题前缀匹配器...");
                _topicMatcher = new ManagedBridge.PrefixMatcher();

                foreach (var topic in topics.Distinct())
                {
                    _topicMatcher.AddWord(topic);
                }

                _isTopicInitialized = true;
                _topicCache.Clear();
                _logger.LogInformation("主题前缀匹配器初始化完成");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<string> MatchCategoryPrefix(string prefix)
        {
            return MatchPrefix(prefix, _isCategoryInitialized, _categoryMatcher, _categoryCache, "类别");
        }

        public IEnumerable<string> MatchTopicPrefix(string prefix)
        {
            return MatchPrefix(prefix, _isTopicInitialized, _topicMatcher, _topicCache, "主题");
        }

        private IEnumerable<string> MatchPrefix(string prefix, bool isInitialized,
                                              ManagedBridge.PrefixMatcher matcher,
                                              ConcurrentDictionary<string, List<string>> cache,
                                              string type)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                _logger.LogInformation($"匹配请求: 空{type}前缀");
                return Enumerable.Empty<string>();
            }

            if (!isInitialized)
            {
                _logger.LogWarning($"匹配请求: {type}匹配器未初始化");
                return Enumerable.Empty<string>();
            }

            if (cache.TryGetValue(prefix, out var cachedResult))
            {
                _logger.LogInformation($"缓存命中: {type}前缀 '{prefix}' -> {cachedResult.Count} 个结果");
                return cachedResult;
            }

            _lock.EnterReadLock();
            try
            {
                _logger.LogInformation($"匹配请求: {type}前缀 '{prefix}'");

                var matches = matcher.GetMatches(prefix)
                    .Cast<string>()
                    .OrderBy(word => word)
                    .ToList();

                _logger.LogInformation($"匹配结果: {type}前缀 '{prefix}' -> {matches.Count} 个结果");

                if (prefix.Length <= 3)
                {
                    cache[prefix] = matches;
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
            _categoryCache.Clear();
            _topicCache.Clear();
        }
    }
}