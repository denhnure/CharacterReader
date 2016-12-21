using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CharacterReader.Core.CharacterReader;
using CharacterReader.Core.CharacterReaderProcessor;
using CharacterReader.Core.StringSplitter;
using CharacterReader.Core.WordFrequenciesCounter;

namespace ConcurrentCharacterReaderConsoleApplication
{
    public class Program
    {
        private static readonly StringSplitter StringSplitter = new StringSplitter();
        private static readonly WordFrequenciesCounter WordFrequenciesCounter = new WordFrequenciesCounter();
        private static readonly ConcurrentBag<string> TextsThatAreCompletelyRead = new ConcurrentBag<string>();
        private static readonly ConcurrentStack<string> TextsThatArePartiallyRead = new ConcurrentStack<string>();
        private static ICharacterReader[] _characterReaders;

        public static void Main(string[] args)
        {
            using (var timer = new Timer(10000))
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

            var tasks = new List<Task>();

            timer.Start();

            foreach (var characterReader in characterReaders)
            {
                var task = Task.Run(() =>
                {
                    using (var characterReaderProcessorWithProgressNotification = new CharacterReaderProcessorWithProgressNotification(characterReader, new Progress<string>(ReportProgress), timer))
                    {
                        var text = characterReaderProcessorWithProgressNotification.ReadToEnd();
                        TextsThatAreCompletelyRead.Add(text);
                    }
                });

                tasks.Add(task);
            }

            var bunchOfTasks = Task.WhenAll(tasks);
            bunchOfTasks.Wait();

            var complete = TextsThatAreCompletelyRead.SelectMany(text => StringSplitter.SplitByWords(text)).ToList();
            var wordFrequiencies = WordFrequenciesCounter.Calculate(complete);

            Console.WriteLine("Final results:");

            OutputToConsole(wordFrequiencies);

            Console.ReadKey();
        }

        private static void ReportProgress(string partiallyReadText)
        {
            lock (new object())
            {
                TextsThatArePartiallyRead.Push(partiallyReadText);

                if (TextsThatAreCompletelyRead.Count + TextsThatArePartiallyRead.Count == _characterReaders.Length)
                {
                    var complete = TextsThatAreCompletelyRead.SelectMany(text => StringSplitter.SplitByWords(text)).ToList();
                    var partiall = TextsThatArePartiallyRead.SelectMany(text => StringSplitter.SplitByWordsAndRemoveTheLast(text)).ToList();

                    complete.AddRange(partiall);

                    TextsThatArePartiallyRead.Clear();

                    var wordFrequiencies = WordFrequenciesCounter.Calculate(complete);

                    Console.WriteLine("Intermediate results:");

                    OutputToConsole(wordFrequiencies);
                }
            }
        }

        private static void OutputToConsole(IOrderedEnumerable<KeyValuePair<string, int>> wordFrequencies)
        {
            foreach (KeyValuePair<string, int> wordFrequency in wordFrequencies)
            {
                Console.WriteLine($"{wordFrequency.Key} - {wordFrequency.Value}");
            }

            Console.WriteLine();
        }
    }
}
