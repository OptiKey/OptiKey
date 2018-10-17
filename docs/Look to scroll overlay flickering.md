The overlay that WPF's debugging tools add to our own "look to scroll" overlay window can cause
flickering or jumpy scrolling behavior as the WPF overlay competes with the actual window 
underneath to be seen as "front-most" at the current point source position. This can be fixed at 
runtime by toggling off the "Show runtime tools in application" toolbar button in Visual Studio's
Live Visual Tree pane. There's also a setting in Visual Studio of the same name under 
Tools > Settings > Debugging > General > "Enable UI Debugging Tools for XAML" that can be disabled.
