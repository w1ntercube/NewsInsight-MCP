#pragma once

#include "../NativePrefixMatcher/NativeMatcher.h"  // 包含原生头文件

namespace ManagedBridge {
    public ref class PrefixMatcher {
    public:
        PrefixMatcher();
        ~PrefixMatcher();
        void AddWord(System::String^ word);
        bool MatchPrefix(System::String^ prefix);

        array<System::String^>^ GetMatches(System::String^ prefix);

    private:
        void* nativeMatcher;
    };
}