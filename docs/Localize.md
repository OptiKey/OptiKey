# Add a new Locale

A locale can be made of different components:

- a keyboard loayout
- a dictionnary, for word completion
- UI element translations

You can provide any of those, but in any case, a new entry to `Langage` enumeration must be added to the code.


## Declare a new language

Go into `src\Enums\Languages.cs` and:

- add a new entry into the Language enumeration.
- add a human-friendly name for this enty in the switch loop

In `src\UI\ViewModels\Management\WordsViewModel.cs`:

- add another `KeyValuePair<string, Languages>` with the new locale.FranceFrench)


## Provide new translation

We're using [Transifex][3] collaboration website to find translators and review translations.

Register on Transifex, and search for the [OptiKey orgnanization][4]. 
Ask to become a member, and submit the language(s) you would like to work on.

Julius will create if necessary the ResX file translation for your language, and once it will be reviewed, 
you can download it for usage.

In the `src\Properties` folder, copy your localized .resx file.

Its name must start with `Resources.` then you'll put your locale code, and after the extension `.resx`
Use .Net [supported locale codes][1], with optionnal regionnal code, like fr-CA for french spoken in Quebec.

You may use the [VisualLocalizer][2] add-on for a more comfortable resx edition.

Then don't forget to add your resx file to the VisualStudio project:

- right click on OptiKey project
- add existing item...
- select your resx file
- move it to the Properties folder in solution explorer if necessary


## Add a new dictionnary

Dictionnaries are stored into `src\Dictionaries` and must be named after their respective locale (case sensitive).

Don't forget to include it in the build: with Visual Studio, right click on it, and in the file properties:

- set the build action to `Content`
- set the 'copy to output directory' to `Always`


## Add a keyboard layout (optionnal)

You must provide two different layout (for regular keyboard and for conversation keyboard).
They are located into `src\UI\Views\Keyboards\{LayoutName}` folder where `LayoutName` may differ from the actual Language name.
In case of reusing an existing layout, just skip the file creation and jump to next paragraph.

- add `Alpha.xaml` and `Alpha.xaml.cs` files for regular keyboard layout
- add `ConversationAlpha.xaml` and `ConversationAlpha.xaml.cs` files for simplified conversation keyboard layout

You may copy existing ones and reorder the keys. Don't forge to update x:Class attribute (xaml) and namespace (cs) !

In `\src\UI\Controls\KeyboardHost.cs`, associate your layout to the right locale(s):

- on file's top, include the views `using FrenchViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.{LayoutName};` (only if using a new layout)
- in `GenerateContent()` method, affect your alpha layout to `newContent` variable under `if (Keyboard is ViewModelKeyboards.Alpha)` loop
- in the same method, affect your conversation layout to `newContent` variable under `if (Keyboard is ViewModelKeyboards.ConversationAlpha)` loop


[1]: https://msdn.microsoft.com/en-us/library/hh441729.aspx
[2]: https://visuallocalizer.codeplex.com/
[3]: https://www.transifex.com
[4]: https://www.transifex.com/optikey/