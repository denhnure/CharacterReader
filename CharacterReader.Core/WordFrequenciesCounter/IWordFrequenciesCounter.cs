using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterReader.Core.WordFrequenciesCounter
{
    public interface IWordFrequenciesCounter
    {
        IOrderedEnumerable<KeyValuePair<string, int>> Calculate(IEnumerable<string> words);
    }
}
