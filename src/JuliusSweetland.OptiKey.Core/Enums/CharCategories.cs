// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
namespace JuliusSweetland.OptiKey.Enums
{
    // this class was added for BackMany functionality
    // to split a word into contiguous groups / split on 
    // character boundary. It is also used for splitting
    // a string to find 'last word present to be basing
    // prediction on'. 
    public enum CharCategories
    {
        NewLine,
        Space,
        Tab,
        OtherLetter,
        WordSeparator, // character that can be used to split a word into 2 valid word-like objects, e.g. '-' 
        WordCharacter, // character in a word, including apostrophes
        OtherSymbol,
        SomethingElse
    }
}
