using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharacterReader.Core.CharacterReader;
using CharacterReader.Core.CharacterReaderProcessor;
using CharacterReader.Core.StringSplitter;
using CharacterReader.Core.WordFrequenciesCounter;
using Timer = System.Timers.Timer;

namespace ConcurrentCharacterReaderConsoleApplication
{
    public class Program
    {
        const int TimerFrequency = 10000;
        private static ICharacterReader[] _characterReaders;
        private static readonly StringSplitter StringSplitter = new StringSplitter();
        private static readonly WordFrequenciesCounter WordFrequenciesCounter = new WordFrequenciesCounter();
        private static readonly ConcurrentBag<string> TextsThatAreCompletelyRead = new ConcurrentBag<string>();
        private static readonly List<string> TextsThatArePartiallyRead = new List<string>();
        private static readonly object SyncObjectForProgressReport = new object();
        private static readonly object SyncObjectForConsoleOutput = new object(); //NOTE: prevents situations when 2 or more threads write to console simultaneously

        public static void Main(string[] args)
        {
            using (var timer = new Timer(TimerFrequency))
            {
                using (ICharacterReader characterReader1 = new SlowCharacterReader(), characterReader2 = new SlowCharacterReader(), characterReader3 = new SlowCharacterReader(), characterReader4 = new SlowCharacterReader())
                {
                    Process(new[] { characterReader1, characterReader2, characterReader3, characterReader4 }, timer);
                }
            }
        }

        private static void Process(ICharacterReader[] characterReaders, Timer timer)
        {
            _characterReaders = characterReaders;

            var progress = new Progress<string>(ReportProgress);
            var tasks = new List<Task>();

            timer.Start();

            foreach (var characterReader in characterReaders)
            {
                var task = Task.Run(() =>
                {
                    using (var characterReaderProcessorWithProgressNotification = new CharacterReaderProcessorWithProgressNotification(characterReader, progress, timer))
                    {
                        var text = characterReaderProcessorWithProgressNotification.ReadToEnd();
                        TextsThatAreCompletelyRead.Add(text);
                    }
                });

                tasks.Add(task);
            }

            var bunchOfTasks = Task.WhenAll(tasks);

            try
            {
                bunchOfTasks.Wait();
            }
            catch (AggregateException)
            {
                Console.WriteLine("Ooops. Something crashed. Please, restart!");
            }

            var processedWords = TextsThatAreCompletelyRead.SelectMany(text => StringSplitter.SplitByWords(text)).ToList();

            CalculateWordFrequenciesAndOutputToConsole(processedWords, false);
            
            Console.ReadKey();
        }

        private static void ReportProgress(string partiallyReadText)
        {
            lock (SyncObjectForProgressReport)
            {
                TextsThatArePartiallyRead.Add(partiallyReadText);

                if (TextsThatAreCompletelyRead.Count + TextsThatArePartiallyRead.Count == _characterReaders.Length)
                {
                    var processedWords = TextsThatAreCompletelyRead.SelectMany(text => StringSplitter.SplitByWords(text)).ToList();
                    var wordsOfPartiallyReadTexts = TextsThatArePartiallyRead.SelectMany(text => StringSplitter.SplitByWordsAndRemoveTheLast(text)).ToList();

                    processedWords.AddRange(wordsOfPartiallyReadTexts);

                    TextsThatArePartiallyRead.Clear();

                    CalculateWordFrequenciesAndOutputToConsole(processedWords);
                }
            }
        }

        private static async void CalculateWordFrequenciesAndOutputToConsole(List<string> processedWords, bool intermediateResults = true)
        {
            await Task.Run(() =>
            {
                var wordFrequiencies = WordFrequenciesCounter.Calculate(processedWords);

                OutputToConsole(wordFrequiencies, intermediateResults);
            });
        }

        private static void OutputToConsole(IOrderedEnumerable<KeyValuePair<string, int>> wordFrequencies, bool intermediateResults = true)
        {
            lock (SyncObjectForConsoleOutput)
            {
                if (!wordFrequencies.Any())
                {
                    Console.WriteLine("Nothing processed so far :)");
                    return;
                }

                Console.WriteLine(intermediateResults ? "Intermediate results:" : "Final results:");

                foreach (KeyValuePair<string, int> wordFrequency in wordFrequencies)
                {
                    Console.WriteLine($"{wordFrequency.Key} - {wordFrequency.Value}");
                }

                Console.WriteLine();
            }
        }
    }
}
