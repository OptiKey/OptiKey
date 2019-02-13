# Physical Setup and Configuration of Eye Trackers

* Don't look down - keep the keyboard level with your eyeline. Looking down closes your eyelid over your eye, reducing tracking accuracy.
* Calibrate the eye tracker to an area less than the total screen size.
* Only screen sizes up to 24 inches are supported by trackers like [GazeTracker][] and [TheEyeTribe][]. Check the limitations.
* If available, calibrate for more points (12 rather than 9, for example).
* If available, when calibrating, try to match the background/foreground colours to the application you will end up using, e.g. this on-screen keyboard.
* A head rest can help prevent head movement.
* Sit so that your head is within the effective range of your eye tracker. For [GazeTracker][] and [TheEyeTribe][] this is between 45 and 75cm (18 and 30 inches) from the tracker/screen.
* Try your eye tracker at different refresh rates. For example, [TheEyeTribe][] tracker can be set to 30fps or 60fps, which is achieved by starting the EyeTribe server with the `-f=30` or `-f=60` command-line switch.  
  *Example*: `"C:\Program Files (x86)\EyeTribe\Server\EyeTribe.exe" -f=60`

[GazeTracker]: http://www.eyetellect.com/gazetracker/ "Eyetellect's GazeTracker"
[TheEyeTribe]: http://theeyetribe.com/ "The Eye Tribe"