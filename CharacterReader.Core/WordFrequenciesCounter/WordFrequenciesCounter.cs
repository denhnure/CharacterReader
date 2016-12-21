using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterReader.Core.WordFrequenciesCounter
{
    public class WordFrequenciesCounter : IWordFrequenciesCounter
    {
        public IOrderedEnumerable<KeyValuePair<string, int>> Calculate(IEnumerable<string> words)
        {
            return words
                .AsParallel()
                .GroupBy(o => o, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key.ToLower(), g => g.Count())
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Key);
        }
    }
}