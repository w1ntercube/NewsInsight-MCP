#pragma once
#include <vector>

#ifdef NATIVE_EXPORTS
#define NATIVE_API __declspec(dllexport)
#else
#define NATIVE_API __declspec(dllimport)
#endif

extern "C" {
    NATIVE_API void* CreateMatcher();
    NATIVE_API void DisposeMatcher(void* matcher);
    NATIVE_API void AddWord(void* matcher, const char* word);
    NATIVE_API bool PrefixMatch(void* matcher, const char* prefix);
    NATIVE_API void GetMatches(void* matcher, const char* prefix, char*** matches, int* count);
    NATIVE_API void FreeMatches(char** matches, int count);
}
