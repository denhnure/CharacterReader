using System;
using System.Collections.Generic;
using System.Linq;
using CharacterReader.Core.CharacterReader;
using CharacterReader.Core.CharacterReaderProcessor;
using CharacterReader.Core.StringSplitter;
using CharacterReader.Core.WordFrequenciesCounter;

namespace CharacterReader
{
    public class Program
    {
        private static readonly StringSplitter StringSplitter = new StringSplitter();
        private static readonly WordFrequenciesCounter WordFrequenciesCounter = new WordFrequenciesCounter();

        public static void Main(string[] args)
        {
            using (var simpleCharacterReader = new SimpleCharacterReader())
            {
                var simpleCharacterReaderProcessor = new SimpleCharacterReaderProcessor(simpleCharacterReader);

                var textToBeProcessed = simpleCharacterReaderProcessor.ReadToEnd();
                var words = StringSplitter.SplitByWords(textToBeProcessed);
                var wordFrequencies = WordFrequenciesCounter.Calculate(words);

                OutputToConsole(wordFrequencies);
            }

            Console.ReadKey();
        }

        private static void OutputToConsole(IOrderedEnumerable<KeyValuePair<string, int>> wordFrequencies)
        {
            foreach (KeyValuePair<string, int> wordFrequency in wordFrequencies)
            {
                Console.WriteLine($"{wordFrequency.Key} - {wordFrequency.Value}");
            }
        }
    }
}
