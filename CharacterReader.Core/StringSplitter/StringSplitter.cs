using System;
using System.Linq;

namespace CharacterReader.Core.StringSplitter
{
    public class StringSplitter : IStringSplitter
    {
        public string[] SplitByWords(string input)
        {
            return input.Split(new[] { '\n', '\r', '\t', ' ', ':', '-', ';', '!', '?', ',', '.', '\'', '"', '`', '(', ')' },
                StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] SplitByWordsAndRemoveTheLast(string input)
        {
            var words = SplitByWords(input);
            return words.Take(words.Length - 1).ToArray(); // only part of the word can be processed and returned (for example: "He" instead of "Hello" in SlowCharacterReader -> excluding this from results)
        }
    }
}