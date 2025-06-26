using System.Collections.Generic;

namespace NewsInsight.Api.Services
{
    public interface IPrefixMatcherService
    {
        void InitializeMatcher(IEnumerable<string> words);
        IEnumerable<string> MatchPrefix(string prefix);
        void ClearCache();
        bool IsInitialized();
    }
}