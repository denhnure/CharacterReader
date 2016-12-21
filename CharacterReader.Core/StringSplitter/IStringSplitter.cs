using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterReader.Core.StringSplitter
{
    public interface IStringSplitter
    {
        string[] SplitByWords(string input);
        string[] SplitByWordsAndRemoveTheLast(string input);
    }
}
