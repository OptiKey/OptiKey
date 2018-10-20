# Icon Design Notes

Themes such as `Android_Dark` and `Android_Light` use a combination of `ViewBox`es and `Path.Stretch` to size key symbols/icons. This can be a bit confusing, but the golden rule is to make your icons (paths) uniformly fill a square shape. I have had problems with making diagonal and horizontal/vertical arrows look the same size as each other, which boiled down to the fact that horizontal/vertical arrows do not fill a square shape, i.e. they are long or tall and therefore fill a rectangle.  

Modifying the icon paths to correctly fill a square boundary fixes these sort of problems.

Check the [CREDITS.md](CREDITS.md) file for information on where I found the original SVG icons and also for which tools I used to edit them ([Inkscape](https://inkscape.org/)).
