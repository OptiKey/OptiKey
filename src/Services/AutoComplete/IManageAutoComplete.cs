using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
    public interface IManageAutoComplete<T> : IAutoComplete
    {
        void AddEntry(string entry, T metaData);

        void RemoveEntry(string entry);
    }
}
