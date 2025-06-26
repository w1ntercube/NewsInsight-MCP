using System.Collections.Generic;

namespace NewsInsight.Api.Services
{
    public interface IPrefixMatcherService
    {
        void InitializeCategoryMatcher(IEnumerable<string> categories);
        void InitializeTopicMatcher(IEnumerable<string> topics);
        IEnumerable<string> MatchCategoryPrefix(string prefix);
        IEnumerable<string> MatchTopicPrefix(string prefix);
        void ClearCache();

        // 添加状态检查方法
        bool IsCategoryInitialized();
        bool IsTopicInitialized();
    }
}