using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagManager
{
    public enum TranslationService
    {
        GoogleTranslate,
        ChineseTranslate,
        CustomTranslate,
    }

    public enum AutocompleteMode
    {
        Disable,
        StartWith,
        StartWithAndContains,
        StartWithIncludeTranslations,
        StartWithAndContainsIncludeTranslations
    }

    public enum AutocompleteSort
    {
        Alphabetical,
        ByCount
    }

    public enum FilterType
    {
        And,
        Or,
        Not,
        Xor
    }


    public enum UILang
    {
        English,
        Chinese
    }
}
