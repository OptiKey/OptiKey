# OptiKey Localization guide

This document will outline the steps one needs to take to successfully localize OptiKey in 
a Latin-based language. Unfortunately support for languages having a complex alphabet, 
such as Japanese, Chinese etc. is not ready yet for prime-time use.

## Introduction: Adding a new Locale

A **Locale** is the completed localization which involves providing four distinct things:

- UI element translations
- a word dictionary
- a keyboard layout
- integrate them into OptiKey

You can provide any of those, but in order to have a complete **Localization** 
for a particular language all four must be implemented. The rest of the guide is split
into two major parts:

 1. UI Translation (performed on [Transifex][3])
 2. Integration of the translation into OptiKey (performed using [Visual Studio][10])

## UI Translation

Before touching any of the code in the project the first thing that should be completed is
the UI resource string translations, which can be easily done through [Transifex][3]. This
process depending on how much time you have available to allocate might take a while, so it's
better to do that first. This task has also the benefit that it requires no knowledge of 
programming at all, so anyone can take a jab at it.

### Create a Transifex account

The first thing that you have to do is to create an account with [Transifex][3], 
if you don't have one already; you can follow this link [here][4] to create one. 
You can use your LinkedIn or Github profiles to create a linked account, if you
don't want to create a separate account.

### Join OptiKey project in Transifex

The next thing to do, after your account creation is to join the OptiKey project 
in [Transifex][3] so that you have access to the translation resources. To join the
project there you have two choices, either use [Transifex][3] to directly request access
to the OptiKey team project by going [here][5] or better yet create an issue in 
OptiKey Github issue tracker [here][7] letting us know that you want to 
translate a specific language.


### Translating the UI strings in Transifex

After the successful creation of your account and your addition to the translation team of a
specific language you are ready to start translating the UI resource strings of OptiKey to
your language. Start this process by going to your [Transifex dashboard][6], there you should
see your projects, including OptiKey as it is shown in the figure below.

![OptiKey Dashboard](https://raw.githubusercontent.com/OptiKey/OptiKey/master/docs/OptiKeyLocaleGuideImages/transifex_dashboard.JPG)

There you can find the languages that their translation process has started, 
as well as the percentage of translated strings that each one has so far.


#### Translating the strings

After you have checked out and made yourself somewhat familiar with 
[Transifex][3] interface you are ready to start translating your language. This can 
be done by going to your [dashboard][6] and selecting the language you applied
to translate. Then click the OptiKey version you want to translate for:

![OptiKey Project Selection](https://raw.githubusercontent.com/OptiKey/OptiKey/master/docs/OptiKeyLocaleGuideImages/transifex_project_sel.JPG)

Then select the option to translate *English strings from Resource.resx* shown
below.

![OptiKey Project Selection](https://raw.githubusercontent.com/OptiKey/OptiKey/master/docs/OptiKeyLocaleGuideImages/transifex_project_str_resx.JPG)

After clicking the last link you should be greeted with the following box

![OptiKey Project Selection](https://raw.githubusercontent.com/OptiKey/OptiKey/master/docs/OptiKeyLocaleGuideImages/transifex_project_trans_box.JPG)


Click the translate button, which should be Green colored if you have the permissions
to translate that language; if not please contact us in order to fix this! This should 
lead you to the following

![OptiKey Project Selection](https://raw.githubusercontent.com/OptiKey/OptiKey/master/docs/OptiKeyLocaleGuideImages/transifex_project_trans_page.JPG)

Which is the actual translation interface. You can easily start translating by first
clicking at the desired string to be translated and typing the representative 
translation to your language in the bottom right box clicking **Save** to store the
translation. Play around with the [Transifex][3] interface a bit, it's relatively 
easy to use so you shouldn't have any problems -- but should you do, let us know so 
we can help you!

After completing the translation in [Transifex][3], you should notify us as soon as possible
so the translation you made can be checked and have it's `.resx` file generated as
you will need this in order to implement the new locale inside OptiKey. Having acquired
your language `.resx` file it's time to start adding the new **Locale** inside OptiKey.

## Integrate the translation into OptiKey

For this step you should have installed a recent version of [Visual Studio][10], along with
it's [Git extension][8] and [Git][9] itself. After you ensure that these components are installed
and ready to use you should make a fresh clone of the OptiKey repository; then open the
solution inside Visual Studio.

Tip: Now it would be a good time to create a new branch for your language if not 
already present. Please contact us to create a new branch which you can then fork and work against.

Tip2: Where possible adhere to the coding/naming style that is already present inside
the project to avoid breaking the style consistency.

### Adding the translation specific files into OptiKey

Before starting anything else it would be a good idea to copy and place correctly 
the necessary translation specific files for your language. These are the following:

 1. The `.resx` file which is generated by [Transifex][3] and contains the translated UI strings
 2. The `.dic` file which is the dictionary that will be used for word prediction.

#### Add the `.resx` file

The UI strings resource file is not difficult to generate, as that's done automatically by
[Transifex][3] itself; thus grab that file from [Transifex][3] and **before** putting it
inside the correct folder rename it using the following  scheme: 
`Resources.CultureInfoCode.resx` where `CultureInfoCode` is the value for 
our language from the row using the same name located [here][11]. Thus for `Greek` the
name of the `.resx` file would be `Resources.el-GR.resx`. Now that we have named our 
resources file correctly we have to place it in the following directory 
`src\JuliusSweetland.OptiKey\Properties`.

After placing the file in the correct folder go back in Visual Studio in OptiKey solution
and do the following:

 1. Right-click `JuliusSweetland.OptiKey` project
 2. Select `Add` 
 3. Select `Existing Item`
 4. Go to `src\JuliusSweetland.OptiKey\Properties` and select 
    the `Resources.el-GR.resx` file you just placed
 5. Click `OK` to add the resource file
 6. The final step is to place the `Resources.el-GR.resx` file inside the `Properties` folder
    in the project (which has a wrench icon), do that by dragging and dropping the file 
    inside the solution to the correct place.

Please ensure that the property of the file named `Build Action` is set to 
`Embedded Resource`.

#### Add the `.dic` file

This process is extremely similar to the above, but the naming scheme is a bit different;
here the naming scheme follows the `LanguageRegion.dic` rule, where the `Language` term
is our language and `Region` our region value; so in our case we would name the 
dictionary as `GreekGreece.dic`. After naming the file correctly you should place it inside
`src\JuliusSweetland.OptiKey\Dictionaries`. Now go back into Visual Studio with an 
open OptiKey solution and perform the following steps:

 1. Right-click `JuliusSweetland.OptiKey` project
 2. Select `Add` 
 3. Select `Existing Item`
 4. Go to `src\JuliusSweetland.OptiKey\Dictionaries` and select 
    the `GreekGreece.dic` file you just placed
 5. Click `OK` to add the dictionary file
 6. Place the `GreekGreece.dic` file inside the `Dictionaries` folder
    in the project, do that by dragging and dropping the file inside the solution to 
    the correct place.
 7. The final step is to select the `GreekGreece.dic` file and set the following two
    properties of the file:
    1. `Build action` to `Content`
    2. `Copy to Output Directory` to `Always`  

Please be aware that the dictionary file must be quite small at this stage due to internal
performance limitations; the accepted dictionary size is less than about 100k entries -- the lower the better but it has to provide a satisfactory experience! Effort 
is being made in order to make the performance acceptable with larger dictionary sizes 
but unfortunately this enhancement is still a work in progress.

Now we are ready to start implementing the logic required to add the new language.

### Adding a new Language

The first thing we need to do is to go inside `JuliusSweetland.OptiKey` project 
and open `Enums\Languages.cs` (full path 
`src\JuliusSweetland.OptiKey\Enums\Languages.cs`) 
in order to perform the following additions:

- add a new entry into the `Language` enumeration.
- add a human-friendly name for this entry in the `ToDescription` function switch statement
- add the return value for this entry in the `ToCultureInfo` function switch statement

Let's tackle those tasks step by step.

#### Add an entry to the `Language` enumeration

Let's say, for this example that our language is `Greek` and we see that
the `Language` enumeration is as follows:

```cs
public enum Languages
    {
	    DutchBelgium,
        DutchNetherlands,
        EnglishCanada,
        EnglishUK,
        EnglishUS,
        FrenchFrance,
        GermanGermany,
        RussianRussia
    }
```

Now we see that the naming convention used here, is `Language` and `Region` 
stitched together, so for `Greek` we should have `Greek` for the `Language` term
and `Greece` for the `Region` term; so the final entry should be `GreekGreece`.

Additionally we observe that entries in `Language` enumeration are in 
lexicographical order, so we should put our entry in the correct place based on
that ordering. Of course this trait doesn't affect the program correctness in 
any way but as we said above you should, where possible adhere with the coding 
style that is already present... so the end result would look like the following:

```cs
public enum Languages
    {
	    DutchBelgium,
        DutchNetherlands,
        EnglishCanada,
        EnglishUK,
        EnglishUS,
        FrenchFrance,
        GermanGermany,
        GreekGreece,
        RussianRussia
    }
```

#### Add a switch case to the `ToDescription` and `ToCultureInfo` methods

##### Adding `ToCultureInfo` entry

This part is very similar to what we did in the previous section, so without further addo here
is the switch statement inside `ToCultureInfo` method:

```cs
    switch (languages)
    {
        case Languages.DutchBelgium: return CultureInfo.GetCultureInfo("nl-BE");
        case Languages.DutchNetherlands: return CultureInfo.GetCultureInfo("nl-NL");			
        case Languages.EnglishUS: return CultureInfo.GetCultureInfo("en-US");
        case Languages.EnglishUK: return CultureInfo.GetCultureInfo("en-GB");
        case Languages.EnglishCanada: return CultureInfo.GetCultureInfo("en-CA");
        case Languages.FrenchFrance: return CultureInfo.GetCultureInfo("fr-FR");
        case Languages.GermanGermany: return CultureInfo.GetCultureInfo("de-DE");
        case Languages.RussianRussia: return CultureInfo.GetCultureInfo("ru-RU");
    }
```

We can easily see that when the `switch` statement is matched to a 
specific locale we return the `CultureInfo` for that locale based on 
the available `.Net` locales. To find what is the specific locale 
codename for the language you are currently translating you can look it 
up [here][11], the value you are looking for is the one inside the 
**CultureInfo** column. In our case the `CultureInfo` code for `Greek` 
is `el-GR`; thus the resulting `switch` statement with the added line
for our language would be the following:

```cs
    switch (languages)
    {
        case Languages.DutchBelgium: return CultureInfo.GetCultureInfo("nl-BE");
        case Languages.DutchNetherlands: return CultureInfo.GetCultureInfo("nl-NL");			
        case Languages.EnglishUS: return CultureInfo.GetCultureInfo("en-US");
        case Languages.EnglishUK: return CultureInfo.GetCultureInfo("en-GB");
        case Languages.EnglishCanada: return CultureInfo.GetCultureInfo("en-CA");
        case Languages.FrenchFrance: return CultureInfo.GetCultureInfo("fr-FR");
        case Languages.GermanGermany: return CultureInfo.GetCultureInfo("de-DE");
        case Languages.GreekGreece: return CultureInfo.GetCultureInfo("el-GR");
        case Languages.RussianRussia: return CultureInfo.GetCultureInfo("ru-RU");
    }
```

##### Adding `ToDescription` entry

This part is a bit trickier, but let's first observe the `switch` 
statement inside `ToDescription` method:

```cs
switch (languages)
    {
        case Languages.DutchBelgium: return Resources.DUTCH_BELGIUM;
        case Languages.DutchNetherlands: return Resources.DUTCH_NETHERLANDS;
        case Languages.EnglishCanada: return Resources.ENGLISH_CANADA;
        case Languages.EnglishUK: return Resources.ENGLISH_UK;
        case Languages.EnglishUS: return Resources.ENGLISH_US;
        case Languages.FrenchFrance: return Resources.FRENCH_FRANCE;
        case Languages.GermanGermany: return Resources.GERMAN_GERMANY;
        case Languages.RussianRussia: return Resources.RUSSIAN_RUSSIA;
    }
```

Here we can see that each language entry is *identified* by a unique identifier 
which is present in the `Resources` object but for our language this is currently
missing. To amend that before adding the new entry we have to do the following:

 1. Add the look-up methods for our locate in `Resources.Designer.cs` file
 2. Add the data points in the `Resources.resx` file

We will first add the new language **identifiers** in the `Resources.resx` file 
as this is a bit tricky. To do that we have to open `Resources.resx` this can 
either be done by clicking while holding `Ctrl` on an already existing 
language element like `ENGLISH_UK` or opening the `Properties\Resources.resx`
file from the `JuliusSweetland.OptiKey` project.

There we  have to add two entries to the `XML` like file; one will be named
using the convention `Language_Region` (all caps) for the first one and for the
second `Language_Region_SPLIT_WITH_NEWLINE` (again all caps). Based on 
the previously mentioned rules ours would be `GREEK_GREECE` and 
`GREEK_GREECE_SPLIT_WITH_NEWLINE`.

Now that we have the names set in stone let's see what the entries of inside
the `Resources.resx` would look like:

```xml
  <data name="GREEK_GREECE" xml:space="preserve">
    <value>Greek (Greece) / Ελληνικά (Greece)</value>
  </data>
  <data name="GREEK_GREECE_SPLIT_WITH_NEWLINE" xml:space="preserve">
    <value>Greek
(Greece) /
Ελληνικά
(Greece)</value>
  </data>
```

These entries contain in the `name` tag the values we previously said and in the
`value` tag a description of the locale written in both English as well as in 
the translated language, which in  this case is Greek. Please be careful when
entering the second value as you have to separate each word with a newline.

Now the modification and regeneration of `Resources.Designer.cs` file 
will be done automatically by Visual Studio itself after it detects a changed
`Resources.resx` file; thus at this point just rebuild the application -- which
should happen without any problems. Finally, after all this work we are able 
to add the new entry in the `ToDescription` method `switch` statement; the 
end result should look like this:

```cs
switch (languages)
    {
        case Languages.DutchBelgium: return Resources.DUTCH_BELGIUM;
        case Languages.DutchNetherlands: return Resources.DUTCH_NETHERLANDS;
        case Languages.EnglishCanada: return Resources.ENGLISH_CANADA;
        case Languages.EnglishUK: return Resources.ENGLISH_UK;
        case Languages.EnglishUS: return Resources.ENGLISH_US;
        case Languages.FrenchFrance: return Resources.FRENCH_FRANCE;
        case Languages.GermanGermany: return Resources.GERMAN_GERMANY;
        case Languages.GreekGreece: return Resources.GREEK_GREECE;
        case Languages.RussianRussia: return Resources.RUSSIAN_RUSSIA;
    }
```

Again we adhere to the coding style as well as the lexicographical ordering of the
entries -- and you should as well!

### Add Language entry in WordsViewModel

Now that we finished modifying `Language.cs` file we have to shift our
attention to `WordsViewModel.cs` which is located in `JuliusSweetland.OptiKey`
project and specifically in `UI/ViewModels/Management/WordsViewModel.cs` 
(full path `src/JuliusSweetland.OptiKey/src/UI/ViewModels/Management/WordsViewModel.cs`)

After opening the file find the `Languages` property which should look like this:

```cs
public List<KeyValuePair<string, Languages>> Languages
{
    get
    {
        return new List<KeyValuePair<string, Languages>>
        {
            new KeyValuePair<string, Languages>(Resources.DUTCH_BELGIUM, 
                                        Enums.Languages.DutchBelgium),
            new KeyValuePair<string, Languages>(Resources.DUTCH_NETHERLANDS, 
                                        Enums.Languages.DutchNetherlands),
            new KeyValuePair<string, Languages>(Resources.ENGLISH_CANADA, 
                                        Enums.Languages.EnglishCanada),
            new KeyValuePair<string, Languages>(Resources.ENGLISH_UK, 
                                        Enums.Languages.EnglishUK),
            new KeyValuePair<string, Languages>(Resources.ENGLISH_US, 
                                        Enums.Languages.EnglishUS),
            new KeyValuePair<string, Languages>(Resources.FRENCH_FRANCE, 
                                        Enums.Languages.FrenchFrance),
            new KeyValuePair<string, Languages>(Resources.GERMAN_GERMANY, 
                                        Enums.Languages.GermanGermany),
            new KeyValuePair<string, Languages>(Resources.RUSSIAN_RUSSIA, 
                                        Enums.Languages.RussianRussia)
        };
    }
}
```

Again as with the previous cases we have to add an entry for our language, so
we observe that we have to add a `KeyValuePair<string, Languages>` -- 
which in our case the `(Key, Value)` pair would be 
`(Resources.Language_Region, Enums.Languages.LanguageRegion)`. 
Thus after adding the new entry the result should be the following:

```cs
public List<KeyValuePair<string, Languages>> Languages
{
    get
    {
        return new List<KeyValuePair<string, Languages>>
        {
            new KeyValuePair<string, Languages>(Resources.DUTCH_BELGIUM, 
                                        Enums.Languages.DutchBelgium),
            new KeyValuePair<string, Languages>(Resources.DUTCH_NETHERLANDS, 
                                        Enums.Languages.DutchNetherlands),
            new KeyValuePair<string, Languages>(Resources.ENGLISH_CANADA, 
                                        Enums.Languages.EnglishCanada),
            new KeyValuePair<string, Languages>(Resources.ENGLISH_UK, 
                                        Enums.Languages.EnglishUK),
            new KeyValuePair<string, Languages>(Resources.ENGLISH_US, 
                                        Enums.Languages.EnglishUS),
            new KeyValuePair<string, Languages>(Resources.FRENCH_FRANCE, 
                                        Enums.Languages.FrenchFrance),
            new KeyValuePair<string, Languages>(Resources.GERMAN_GERMANY, 
                                        Enums.Languages.GermanGermany),
            new KeyValuePair<string, Languages>(Resources.GREEK_GREECE, 
                                        Enums.Languages.GreekGreece),
            new KeyValuePair<string, Languages>(Resources.RUSSIAN_RUSSIA, 
                                        Enums.Languages.RussianRussia)
        };
    }
}
```

### Add Language button in `FunctionKeys` enumeration

Now we have to open `FunctionKeys.cs` file which is located in `Enums\FunctionsKeys.cs`
 (full path: `src\JuliusSweetland.OptiKey\Enums\FunctionKeys.cs`). This file
contains the enumeration that is responsible for containing all of OptiKey's 
distinct buttons. Now the naming convention followed here is `LanguageRegion` 
so in our case we would name that entry `GreekGreece`; thus before adding the entry:

```cs
public enum FunctionKeys
{
    // more entries...
    FrenchFrance,
    GermanGermany,
    Home,
    IncreaseOpacity,
    // more entries...
}
```
After our addition:

```cs
public enum FunctionKeys
{
    // more entries...
    FrenchFrance,
    GermanGermany,
    GreekGreece,
    Home,
    IncreaseOpacity,
    // more entries...
}
```


### Add Language `Key` in OptiKey `KeyValues`

Let's now open `KeyValues.cs` file which is located in `Models\KeyValues.cs` 
(full path `src\JuliusSweetland.OptiKey\Models\KeyValues.cs`). This file contains
all the KeyValue pairs and has one entry for each one of the `FunctionKeys` enumeration entry.
Thus our entry would be something like this:

```cs
public static readonly KeyValue GreekGreeceKey = new KeyValue(FunctionKeys.GreekGreece);
```

Use the naming conventions we know and slot your entry into the correct place based on 
the lexicographical order; so our entry would be placed here:

```cs
// more entries...
public static readonly KeyValue FrenchFranceKey = new KeyValue(FunctionKeys.FrenchFrance);
public static readonly KeyValue GermanGermanyKey = new KeyValue(FunctionKeys.GermanGermany);
public static readonly KeyValue GreekGreeceKey = new KeyValue(FunctionKeys.GreekGreece);
public static readonly KeyValue HomeKey = new KeyValue(FunctionKeys.Home);
public static readonly KeyValue IncreaseOpacityKey = new KeyValue(FunctionKeys.IncreaseOpacity);
// more entries...
```

There might be one more thing to do here depending on your language and that is
if your language uses non-Latin characters in their alphabets and hence to their
keyboard mappings. `Greek` is a perfect example of that so if you language falls into that
category you have to go in `KeyValues` method and add the following:

```cs
multiKeySelectionKeys = new Dictionary<Languages, List<KeyValue>>
{
    { Languages.DutchBelgium, defaultList },
    { Languages.DutchNetherlands, defaultList },
    // more entries...
    { Languages.GermanGermany, 
        "abcdefghijklmnopqrstuvwxyzß".ToCharArray()
         .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
         .ToList()
    },
    { Languages.GreekGreece, 
        "ασδφγηξκλ;ςερτυθιοπζχψωβνμ".ToCharArray()
        .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
        .ToList()
    },     
	{ Languages.RussianRussia, 
        "абвгдеёжзийклмнопрстуфхцчшщъыьэюя"
        .ToCharArray()
        .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
        .ToList() }
};
```

What we are basically doing here is adding all the characters that are in each of
the keyboard keys in a string. So as you might observe the only thing that changes
is the first entry which is the our language value from `Languages` enumeration and then
either a string that contains the characters for the keyboard of the `defaultList` which
represents the standard Latin alphabet.

### Add Language default fixation time to `Settings.settings`

In the `Properties` folder open the `Settings.settings` file and edit the `KeySelectionTriggerFixationCompleteTimesByKeyValues` setting. It might be easier to copy the value to notepad and add a new `item` for your new language. Once you have updated the `Settings.settings` file your `App.config` file should automatically be updated to include this new setting value. Double check that this has happened as Visual Studio can be a bit flakey on this. The `App.config` file is actually NOT required, so if in doubt just delete it. Example of new `item` added to the `KeySelectionTriggerFixationCompleteTimesByKeyValues` setting:

```xml
<item>
    <key>
        <keyValue>
            <functionKey>GreekGreece</functionKey>
        </keyValue>
    </key>
    <value>
        <ticks>PT1.75S</ticks>
    </value>
</item>
```

The only thing that will be different for each language is the value of `functionKey` tag, which will be
the FunctionKeys (enum) value you used before for the particular language you are currently translating. After creating the entry,
place it in the correct lexicographical slot.


### Add Language entry in `PointingAndSelectingViewModel`

We now had to edit `PointAndSelectingViewModel.cs` in order to add our language:

```cs
new KeyValueAndTimeSpanGroup(Resources.LANGUAGES_KEY_GROUP, 
    new List<KeyValueAndTimeSpan>
{
    // more entries...
    new KeyValueAndTimeSpan(Resources.GERMAN_GERMANY, KeyValues.GermanGermanyKey, 
        dictionary.ContainsKey(KeyValues.GermanGermanyKey) ? 
        dictionary[KeyValues.GermanGermanyKey] : (TimeSpan?)null),
    new KeyValueAndTimeSpan(Resources.RUSSIAN_RUSSIA, KeyValues.RussianRussiaKey, 
        dictionary.ContainsKey(KeyValues.RussianRussiaKey) ? 
        dictionary[KeyValues.RussianRussiaKey] : (TimeSpan?)null),
    // more entries...
}),
```

And after the changes:

    
```cs
new KeyValueAndTimeSpanGroup(Resources.LANGUAGES_KEY_GROUP, 
    new List<KeyValueAndTimeSpan>
{
    // more entries...
    new KeyValueAndTimeSpan(Resources.GERMAN_GERMANY, KeyValues.GermanGermanyKey, 
        dictionary.ContainsKey(KeyValues.GermanGermanyKey) ? 
        dictionary[KeyValues.GermanGermanyKey] : (TimeSpan?)null),
    new KeyValueAndTimeSpan(Resources.GREEK_GREECE, KeyValues.GreekGreeceKey, 
        dictionary.ContainsKey(KeyValues.GreekGreeceKey) ? 
        dictionary[KeyValues.GreekGreeceKey] : (TimeSpan?) null),
    new KeyValueAndTimeSpan(Resources.RUSSIAN_RUSSIA, KeyValues.RussianRussiaKey, 
        dictionary.ContainsKey(KeyValues.RussianRussiaKey) ? 
        dictionary[KeyValues.RussianRussiaKey] : (TimeSpan?)null),
    // more entries...
}),
```
### Add a Language change button

Let's now add the language button in the interface; to do that we have to open `Language.xaml` which is located
in `UI\Keyboards\Common\Language.xaml`. Don't worry it Visual Studio doesn't render the view, that is normal.
Poking around the code you will find to defined layouts, one for Portrait and one for Landscape. You will have
to implement a button for both layouts and this is very easy to do. You should open OptiKey and go into UI 
Languages selection and find the spot where you should place your language based on the lexicographical ordering;
in our case `Greek` will be placed after `German`. 

It has to be noted that indexing is *different* in both cases and the indices start from 0 not 1 -- so please 
be aware of these facts when making changes.

#### Landscape

Landscape layout is where the grid has more columns than rows; currently used configuration is 2 rows x 5 columns. 
This will probably need to be adjusted as we add more languages. As mentioned above `Greek` will be placed after
`German` so the section will be here:

```cs
<controls:Key Grid.Row="1" Grid.Column="1"
    Text="{x:Static resx:Resources.GERMAN_GERMANY_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.GermanGermanyKey}"/>
<controls:Key Grid.Row="1" Grid.Column="3"
    Text="{x:Static resx:Resources.RUSSIAN_RUSSIA_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.RussianRussiaKey}"/>
```

In Landscape we will use the entry that has *newlines* so our entry for `Greek` for the Landscape
orientation would be the following:

```cs
<controls:Key Grid.Row="1" Grid.Column="1"
    Text="{x:Static resx:Resources.GERMAN_GERMANY_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.GermanGermanyKey}"/>
<controls:Key Grid.Row="1" Grid.Column="2"
    Text="{x:Static resx:Resources.GREEK_GREECE_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.GreekGreeceKey}"/>
<controls:Key Grid.Row="1" Grid.Column="3"
    Text="{x:Static resx:Resources.RUSSIAN_RUSSIA_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.RussianRussiaKey}"/>
```

#### Portrait

Portrait layout is where the grid has more columns than rows; currently used configuration is 
5 rows x 2 columns. This will probably need to be adjusted as we add more languages. Similarly
to the above the section is:

```cs
<controls:Key Grid.Row="0" Grid.Column="1"
    Text="{x:Static resx:Resources.GERMAN_GERMANY_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.GermanGermanyKey}"/>
<controls:Key Grid.Row="2" Grid.Column="1"
    Text="{x:Static resx:Resources.RUSSIAN_RUSSIA_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.RussianRussiaKey}"/>
```

After slotting in our entry the result would look like this:

```cs
<controls:Key Grid.Row="0" Grid.Column="1"
    Text="{x:Static resx:Resources.GERMAN_GERMANY_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.GermanGermanyKey}"/>
<controls:Key Grid.Row="1" Grid.Column="1"
    Text="{x:Static resx:Resources.GREEK_GREECE_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.GreekGreeceKey}"/>
<controls:Key Grid.Row="2" Grid.Column="1"
    Text="{x:Static resx:Resources.RUSSIAN_RUSSIA_SPLIT_WITH_NEWLINE}"
    SharedSizeGroup="KeyWithText"
    Value="{x:Static models:KeyValues.RussianRussiaKey}"/>
```

### Add Language change handler to `MainViewModel.ServiceEventHandlers`

The `HandleFunctionKeySelectionResult` method needs to know how to handle the selection of the new language key from the Languages keyboard. Add a case (in the correct alphabetical position) for your new language, for example:

```
	case FunctionKeys.GreekGreece:
		Log.Info("Changing keyboard language to GreekGreece.");
		Settings.Default.KeyboardAndDictionaryLanguage = Languages.GreekGreece;
		Log.Info("Changing keyboard to Menu");
		Keyboard = new Menu(() => Keyboard = currentKeyboard);
		break;
```

### Add a keyboard layout

In order to complete the translation of the current language one final step remains, which
involves the creation of OptiKey's keyboard layouts for this language. To successfully 
complete that we need to create two distinct keyboards:

 1. a Normal keyboard layout.
 2. a simplified "Conversation" keyboard layout.

This these steps are easily done if you base your work on the already available 
English keyboards while making little changes to them. Before starting 
let's do a bit of setup work:

 1. Go to `UI\Views\Keyboards`
 2. Create a folder for you language (in our case `Greek`)
 3. Create a file inside the newly created folder named `Alpha.xaml`
 4. Create a file inside the same folder you just created named `ConversationAlpha.xaml`

That's it for the setup work, we shall now create each one of these 
keyboards separately.

#### Normal keyboard layout

For your convenience you are expected to base this keyboard on the one located in 
`src\UI\Views\Keyboards\English\Alpha.xaml`; for starters you should first copy
and paste it's contents to your `Alpha.xaml` file. You will immediately see lot's of
stuff but the only thing you have to alter is only entries that look like this:

```xml
<controls:Key Grid.Row="3" Grid.Column="2" Grid.ColumnSpan="2"
    ShiftUpText="α" ShiftDownText="Α"
    SharedSizeGroup="KeyWithSingleLetter"
    Value="α"/>
```

This is mapped to the letter `a` on the English keyboard. Fortunately 
the only things you have to alter are the following three:

 1. `ShiftUpText` value shown with `Shift`-key released 
 2. `ShiftDownText` value shown with `Shift`-key pressed
 3. `Value` value OptiKey receives upon key-press (regardless of `Shift` status)

So in our case `a`-key in Greek is:

```xml
<controls:Key Grid.Row="3" Grid.Column="2" Grid.ColumnSpan="2"
    ShiftUpText="α" ShiftDownText="Α"
    SharedSizeGroup="KeyWithSingleLetter"
    Value="α"/>
```

Please note that the `Value` tag takes the actual value the key has in current 
language so `Value` would be `α` not `a`. Similarly, you have to do that for all
of the keyboard entry keys. You can distinguish the entries that you need to change
by checking if they have `ShiftUpText` and `ShiftDownText` tags.

```xml
<controls:Key Grid.Row="3" Grid.Column="2" Grid.ColumnSpan="2"
    ShiftUpText="α" ShiftDownText="Α"
    SharedSizeGroup="KeyWithSingleLetter"
    Value="α"/>
```

Also you have to change the value of `x:Class` attribute located in the schema definition
at the top of the `.xaml` file. The result would look like the following:

```xml
<controls:KeyboardView 
    x:Class="JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek.Alpha"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:controls="clr-namespace:JuliusSweetland.OptiKey.UI.Controls"
    xmlns:models="clr-namespace:JuliusSweetland.OptiKey.Models"
    xmlns:resx="clr-namespace:JuliusSweetland.OptiKey.Properties"
    mc:Ignorable="d" 
    d:DesignHeight="300" d:DesignWidth="300">
```

Please note the value of `x:Class` which is 
`JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek.Alpha`; this will be our namespace
which will be declared in the respective `Alpha.xaml.cs` file. To do that open a 
file named `Alpha.xaml.cs` that should have been also created with `Alpha.xaml`
(you can view it by expanding the arrow on the left of the `Alpha.xaml` file) and change 
the `namespace` from `JuliusSweetland.OptiKey.UI.Views.Keyboards.English` to 
`JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek`.

So the original that should look like this:

```cs
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.English
{
    // stuff...
}
```

After editing the namespace value would be like this:

```cs
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek
{
    // stuff...
}
```

The final step before this keyboard is ready is to let OptiKey know it exists; to do that
we have to open `KeyboardHost.cs` file which is located in `UI\Controls\KeyboardHost.cs`.



After opening it you will have to check if the `using` directives at the top of the file
contain the `namespace` you just created before, which is 
`JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek`; if they don't just add it like this:

```cs
using GreekViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek;
```

Then search for `if (Keyboard is ViewModelKeyboards.Alpha)` and locate the
`switch` statement within that `if`. This is the segment before the addition:

```cs
switch (Settings.Default.KeyboardAndDictionaryLanguage)
{
    // more cases...
    case Languages.GermanGermany:
        newContent = new GermanViews.Alpha { DataContext = Keyboard };
        break;
    case Languages.RussianRussia:
        newContent = new RussianViews.Alpha { DataContext = Keyboard };
        break;
    default:
        newContent = new EnglishViews.Alpha { DataContext = Keyboard };
        break;
}
```

After adding the case for the `Greek` Alpha keyboard the result would look like this:

```cs
switch (Settings.Default.KeyboardAndDictionaryLanguage)
{
    // more cases...
    case Languages.GermanGermany:
        newContent = new GermanViews.Alpha { DataContext = Keyboard };
        break;
    case Languages.GreekGreece:
        newContent = new GreekViews.Alpha { DataContext = Keyboard };
        break;
    case Languages.RussianRussia:
        newContent = new RussianViews.Alpha { DataContext = Keyboard };
        break;
    default:
        newContent = new EnglishViews.Alpha { DataContext = Keyboard };
        break;
}
```

#### Simplified "Conversation" keyboard layout

Similarly to the previous keyboard, you should base the this keyboard on the 
one located in `src\UI\Views\Keyboards\English\ConversationAlpha.xaml`; so copy
its contents to the `ConversationAlpha.xaml` file you previously created. You will also
have to edit the keyboard keys like you did with the normal keyboard (remember to distinguish
which entries to change by checking for the `ShiftUpText` and `ShiftDownText` tags).

Again we have to change the value of `x:Class` attribute in the schema definition 
here as well, so the result for this keyboard would look like this:

```xml
<controls:KeyboardView 
    x:Class="JuliusSweetland.OptiKey.UI.Views.Keyboards.English.ConversationAlpha"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:controls="clr-namespace:JuliusSweetland.OptiKey.UI.Controls"
    xmlns:models="clr-namespace:JuliusSweetland.OptiKey.Models"
    xmlns:properties="clr-namespace:JuliusSweetland.OptiKey.Properties"
    xmlns:resx="clr-namespace:JuliusSweetland.OptiKey.Properties"
    mc:Ignorable="d" 
    d:DesignHeight="300" d:DesignWidth="300">
```

As with the normal keyboard we have to open the `ConversationAlpha.xaml.cs` file 
of our keyboard and change the `namespace` value to the following:

```cs
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek
{
    // stuff...
}
```

As with the normal keyboard the final step before this keyboard is ready to use 
is to  let OptiKey know of its existence; to do that we have to open `KeyboardHost.cs` 
file which is located in `UI\Controls\KeyboardHost.cs`. Please check that the 
`namespace` is being used -- check previous section. Then search for 
`if (Keyboard is ViewModelKeyboards.ConversationAlpha)` and locate the
`switch` statement within that `if`. This is the segment before the addition:

```cs
switch (Settings.Default.KeyboardAndDictionaryLanguage)
{
    // more cases...
    case Languages.GermanGermany:
        newContent = new GermanViews.Alpha { DataContext = Keyboard };
        break;
    case Languages.RussianRussia:
        newContent = new RussianViews.Alpha { DataContext = Keyboard };
        break;
    default:
        newContent = new EnglishViews.Alpha { DataContext = Keyboard };
        break;
}
```

After adding the case for the `Greek` Alpha keyboard the result would look like this:

```cs
switch (Settings.Default.KeyboardAndDictionaryLanguage)
{
    // more cases...
    case Languages.GermanGermany:
        newContent = new GermanViews.Alpha { DataContext = Keyboard };
        break;
    case Languages.GreekGreece:
        newContent = new GreekViews.Alpha { DataContext = Keyboard };
        break;
    case Languages.RussianRussia:
        newContent = new RussianViews.Alpha { DataContext = Keyboard };
        break;
    default:
        newContent = new EnglishViews.Alpha { DataContext = Keyboard };
        break;
}
```

# End

That's it! It was quite a bit of work... but now we have a new and complete 
localization for OptiKey, thank you!

[1]: https://msdn.microsoft.com/en-us/library/hh441729.aspx
[2]: https://visuallocalizer.codeplex.com/
[3]: https://www.transifex.com
[4]: https://www.transifex.com/signup/
[5]: https://www.transifex.com/optikey/
[6]: https://www.transifex.com/optikey/optikey/dashboard/
[7]: https://github.com/OptiKey/OptiKey/issues
[8]: https://visualstudio.github.com/
[9]: https://git-scm.com/download/win
[10]: https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx
[11]: http://timtrott.co.uk/culture-codes/
