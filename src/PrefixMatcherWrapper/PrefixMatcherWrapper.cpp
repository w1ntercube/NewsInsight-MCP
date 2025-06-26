#include "pch.h"
#include "PrefixMatcherWrapper.h"
#include "../NativePrefixMatcher/NativeMatcher.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace ManagedBridge {
    PrefixMatcher::PrefixMatcher() {
        nativeMatcher = ::CreateMatcher();
    }

    PrefixMatcher::~PrefixMatcher() {
        ::DisposeMatcher(nativeMatcher);
    }

    void PrefixMatcher::AddWord(System::String^ word) {
        System::IntPtr ptr = Marshal::StringToHGlobalAnsi(word);
        const char* nativeWord = static_cast<const char*>(ptr.ToPointer());
        ::AddWord(nativeMatcher, nativeWord);
        Marshal::FreeHGlobal(ptr);
    }

    bool PrefixMatcher::MatchPrefix(System::String^ prefix) {
        System::IntPtr ptr = Marshal::StringToHGlobalAnsi(prefix);
        const char* nativePrefix = static_cast<const char*>(ptr.ToPointer());
        bool result = ::PrefixMatch(nativeMatcher, nativePrefix);
        Marshal::FreeHGlobal(ptr);
        return result;
    }

    array<String^>^ PrefixMatcher::GetMatches(String^ prefix) {
        System::IntPtr ptr = Marshal::StringToHGlobalAnsi(prefix);
        const char* nativePrefix = static_cast<const char*>(ptr.ToPointer());

        char** matches = nullptr;
        int count = 0;
        ::GetMatches(nativeMatcher, nativePrefix, &matches, &count);

        // 创建托管数组
        array<String^>^ result = gcnew array<String^>(count);
        for (int i = 0; i < count; ++i) {
            result[i] = gcnew String(matches[i]);
        }

        ::FreeMatches(matches, count);
        Marshal::FreeHGlobal(ptr);
        return result;
    }
}