# "Look to Scroll" Overlay Flickering

When the overlay from WPF's debugging tools is added to our own "Look to scroll" overlay window, flickering or jumpy scrolling behavior can occur as the WPF overlay competes with the actual window beneath to be seen as "front-most" at the current point source position.

This can be changed at runtime by toggling the "Show runtime tools in application" toolbar button in Visual Studio's `Live Visual Tree` pane. The same setting is also available from Visual Studio, by going to `Tools` (menu) > `Settings` > `Debugging` > `General` > `Enable UI Debugging Tools for XAML`.
