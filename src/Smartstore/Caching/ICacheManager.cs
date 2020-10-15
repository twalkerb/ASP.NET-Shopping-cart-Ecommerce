﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smartstore.Caching
{
    /// <summary>
    /// Contract for a composite multi-level cache manager.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Gets a cache item associated with the specified key
        /// </summary>
        /// <typeparam name="T">The type of the item to get</typeparam>
        /// <param name="key">The cache item key</param>
        /// <param name="independent">When <c>true</c>, no attemp will be made to invalidate depending/parent cache entries.</param>
        /// <returns>Cached item value or <c>null</c> if item with specified key does not exist in the cache</returns>
        T Get<T>(string key, bool independent = false);

        /// <summary>
        /// Gets a cache item associated with the specified key
        /// </summary>
        /// <typeparam name="T">The type of the item to get</typeparam>
        /// <param name="key">The cache item key</param>
        /// <param name="independent">When <c>true</c>, no attemp will be made to invalidate depending/parent cache entries.</param>
        /// <returns>Cached item value or <c>null</c> if item with specified key does not exist in the cache</returns>
        Task<T> GetAsync<T>(string key, bool independent = false);

        /// <summary>
        /// Gets a cache item associated with the specified key or adds the item
        /// if it doesn't exist in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the item to get or add</typeparam>
        /// <param name="key">The cache item key</param>
        /// <param name="acquirer">Func which returns the value to be added to the cache</param>
        /// <param name="independent">When <c>true</c>, no attemp will be made to invalidate depending/parent cache entries.</param>
        /// <param name="allowRecursion">When <c>false</c>, an exception will be thrown when the acquirer tries to acces the same cache item.</param>
        /// <returns>Cached item value</returns>
        T Get<T>(string key, Func<CacheEntryOptions, T> acquirer, bool independent = false, bool allowRecursion = false);

        /// <summary>
        /// Gets a cache item associated with the specified key or adds the item
        /// if it doesn't exist in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the item to get or add</typeparam>
        /// <param name="key">The cache item key</param>
        /// <param name="acquirer">Func which returns the value to be added to the cache</param>
        /// <param name="independent">When <c>true</c>, no attemp will be made to invalidate depending/parent cache entries.</param>
        /// <param name="allowRecursion">When <c>false</c>, an exception will be thrown when the acquirer tries to acces the same cache item.</param>
        /// <returns>Cached item value</returns>
        Task<T> GetAsync<T>(string key, Func<CacheEntryOptions, Task<T>> acquirer, bool independent = false, bool allowRecursion = false);

        /// <summary>
        /// Gets or creates a provider specific hashset implementation.
        /// If key does not exist, a new set is created and put to cache automatically
        /// </summary>
        /// <param name="key">The set cache item key</param>
        /// <param name="acquirer">Optional acquirer callback that is invoked when requested set does not exist yet.</param>
        /// <returns>The hashset</returns>
        ISet GetHashSet(string key, Func<IEnumerable<string>> acquirer = null);

        /// <summary>
        /// Gets or creates a provider specific hashset implementation.
        /// If key does not exist, a new set is created and put to cache automatically
        /// </summary>
        /// <param name="key">The set cache item key</param>
        /// <param name="acquirer">Optional acquirer callback that is invoked when requested set does not exist yet.</param>
        /// <returns>The hashset</returns>
        Task<ISet> GetHashSetAsync(string key, Func<Task<IEnumerable<string>>> acquirer = null);

        /// <summary>
        /// Adds a cache item with the specified key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value. Can be <c>null</c>.</param>
        /// <param name="options">Options (expiration and dependencies).</param>
        void Put(string key, object value, CacheEntryOptions options = null);

        /// <summary>
        /// Adds a cache item with the specified key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value. Can be <c>null</c>.</param>
        /// <param name="options">Options (expiration and dependencies).</param>
        Task PutAsync(string key, object value, CacheEntryOptions options = null);

        /// <summary>
        /// Gets a value indicating whether an entry associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        bool Contains(string key);

        /// <summary>
        /// Gets a value indicating whether an entry associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        Task<bool> ContainsAsync(string key);

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">key</param>
        void Remove(string key);

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">key</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Scans for all keys matching the input pattern 
        /// </summary>
        /// <param name="pattern">A key pattern. Can be <c>null</c>.</param>
        /// <returns>An array of matching key names</returns>
        /// <remarks>
        /// Supported glob-style patterns:
        /// - h?llo matches hello, hallo and hxllo
        /// - h*llo matches hllo and heeeello
        /// - h[ae]llo matches hello and hallo, but not hillo
        /// - h[^e]llo matches hallo, hbllo, ... but not hello
        /// - h[a-b]llo matches hallo and hbllo
        /// </remarks>
        IEnumerable<string> Keys(string pattern = "*");

        /// <summary>
        /// Scans for all keys matching the input pattern 
        /// </summary>
        /// <param name="pattern">A key pattern. Can be <c>null</c>.</param>
        /// <returns>An array of matching key names</returns>
        /// <remarks>
        /// Supported glob-style patterns:
        /// - h?llo matches hello, hallo and hxllo
        /// - h*llo matches hllo and heeeello
        /// - h[ae]llo matches hello and hallo, but not hillo
        /// - h[^e]llo matches hallo, hbllo, ... but not hello
        /// - h[a-b]llo matches hallo and hbllo
        /// </remarks>
        IAsyncEnumerable<string> KeysAsync(string pattern = "*");

        /// <summary>
        /// Removes all entries with keys matching the input pattern.
        /// </summary>
        /// <param name="pattern">Glob pattern</param>
        /// <returns>Count of removed cache items</returns>
        /// <remarks>
        /// Supported glob-style patterns:
        /// - h?llo matches hello, hallo and hxllo
        /// - h*llo matches hllo and heeeello
        /// - h[ae]llo matches hello and hallo, but not hillo
        /// - h[^e]llo matches hallo, hbllo, ... but not hello
        /// - h[a-b]llo matches hallo and hbllo
        /// </remarks>
        long RemoveByPattern(string pattern);

        /// <summary>
        /// Removes all entries with keys matching the input pattern.
        /// </summary>
        /// <param name="pattern">Glob pattern</param>
        /// <returns>Count of removed cache items</returns>
        /// <remarks>
        /// Supported glob-style patterns:
        /// - h?llo matches hello, hallo and hxllo
        /// - h*llo matches hllo and heeeello
        /// - h[ae]llo matches hello and hallo, but not hillo
        /// - h[^e]llo matches hallo, hbllo, ... but not hello
        /// - h[a-b]llo matches hallo and hbllo
        /// </remarks>
        Task<long> RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Clear all cache data
        /// </summary>
        void Clear();

        /// <summary>
        /// Clear all cache data
        /// </summary>
        Task ClearAsync();
    }
}