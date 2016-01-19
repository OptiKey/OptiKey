using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
    /// <summary>
    ///     Defines a management interface on top of an <see cref="IAutoComplete" /> implementation. It allows dictionary
    ///     entries to be added and removed, thus affecting the returned suggestions.
    /// </summary>
    /// <remarks>This class is for management of an underlying provider and so is declared <c>internal</c>.</remarks>
    internal interface IManageAutoComplete : IAutoComplete
    {
        void AddEntry(string entry, DictionaryEntry metaData);

        void Clear();

        void RemoveEntry(string entry);
    }
}