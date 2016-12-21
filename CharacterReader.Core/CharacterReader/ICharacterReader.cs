using System;

namespace CharacterReader.Core.CharacterReader
{
    public interface ICharacterReader : IDisposable
    {
        char GetNextChar();
    }
}