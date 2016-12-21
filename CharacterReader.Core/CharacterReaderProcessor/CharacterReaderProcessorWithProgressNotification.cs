using System;
using System.Timers;
using CharacterReader.Core.CharacterReader;

namespace CharacterReader.Core.CharacterReaderProcessor
{
    public class CharacterReaderProcessorWithProgressNotification : SimpleCharacterReaderProcessor, IDisposable
    {
        private readonly Progress<string> _progressIndicator;
        private readonly Timer _timer;

        public CharacterReaderProcessorWithProgressNotification(ICharacterReader characterReader,
            Progress<string> progressIndicator,
            Timer timer)
            :base(characterReader)
        {
            _progressIndicator = progressIndicator;
            _timer = timer;

            SubscribeToTimerEvent();
        }

        public void ReportProgress(object source, ElapsedEventArgs e)
        {
            ((IProgress<string>) _progressIndicator)?.Report(CurrentlyRetrievedText);
        }

        public void Dispose()
        {
            UnsubscribeFromTimerEvent();
        }

        private void SubscribeToTimerEvent()
        {
            _timer.Elapsed += ReportProgress;
        }

        private void UnsubscribeFromTimerEvent()
        {
            _timer.Elapsed -= ReportProgress;
        }
    }
}