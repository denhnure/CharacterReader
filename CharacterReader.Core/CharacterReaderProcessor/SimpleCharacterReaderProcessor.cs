using System.Text;
using CharacterReader.Core.CharacterReader;

namespace CharacterReader.Core.CharacterReaderProcessor
{
    public class SimpleCharacterReaderProcessor
    {
        private readonly ICharacterReader _characterReader;
        private readonly StringBuilder _stringBuilder;

        public SimpleCharacterReaderProcessor(ICharacterReader characterReader)
        {
            _characterReader = characterReader;
            _stringBuilder = new StringBuilder();
        }

        public string ReadToEnd()
        {
            try
            {
                while (true)
                {
                    _stringBuilder.Append(_characterReader.GetNextChar());
                }
            }
            catch (System.IO.EndOfStreamException)
            {
            }

            return _stringBuilder.ToString();
        }

        protected string CurrentlyRetrievedText => _stringBuilder.ToString();
    }
}
