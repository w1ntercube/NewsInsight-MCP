#include "pch.h"
#include "NativeMatcher.h"
#include <unordered_set>
#include <unordered_map>
#include <vector>
#include <string>

class TrieNode {
public:
    std::unordered_map<char, TrieNode*> children;
    bool isEnd = false;
    std::string word; // 存储完整单词
};

void CollectWords(TrieNode* node, std::vector<std::string>& results) {
    if (node->isEnd) {
        results.push_back(node->word);
    }
    for (auto& pair : node->children) {
        CollectWords(pair.second, results);
    }
}

NATIVE_API void* CreateMatcher() {
    return new TrieNode();
}

NATIVE_API void DisposeMatcher(void* matcher) {
    delete static_cast<TrieNode*>(matcher);
}

NATIVE_API void AddWord(void* matcher, const char* word) {
    TrieNode* root = static_cast<TrieNode*>(matcher);
    TrieNode* curr = root;
    for (const char* p = word; *p; ++p) {
        if (curr->children.find(*p) == curr->children.end()) {
            curr->children[*p] = new TrieNode();
        }
        curr = curr->children[*p];
    }
    curr->isEnd = true;
    curr->word = word; // 存储完整单词
}

NATIVE_API bool PrefixMatch(void* matcher, const char* prefix) {
    TrieNode* root = static_cast<TrieNode*>(matcher);
    TrieNode* curr = root;
    for (const char* p = prefix; *p; ++p) {
        if (curr->children.find(*p) == curr->children.end()) {
            return false;
        }
        curr = curr->children[*p];
    }
    return true;
}

NATIVE_API void GetMatches(void* matcher, const char* prefix, char*** matches, int* count) {
    TrieNode* root = static_cast<TrieNode*>(matcher);
    TrieNode* curr = root;

    // 遍历到前缀末尾节点
    for (const char* p = prefix; *p; ++p) {
        if (curr->children.find(*p) == curr->children.end()) {
            *matches = nullptr;
            *count = 0;
            return;
        }
        curr = curr->children[*p];
    }

    // 收集所有匹配单词
    std::vector<std::string> results;
    CollectWords(curr, results);

    // 转换为C风格数组
    *matches = new char* [results.size()];
    for (int i = 0; i < results.size(); ++i) {
        (*matches)[i] = _strdup(results[i].c_str());
    }
    *count = static_cast<int>(results.size());
}

NATIVE_API void FreeMatches(char** matches, int count) {
    for (int i = 0; i < count; ++i) {
        free(matches[i]);
    }
    delete[] matches;
}