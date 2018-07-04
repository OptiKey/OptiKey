# OptiKey

OptiKey is an on-screen keyboard that is designed to help Motor Neuron Disease (MND) patients interact with Windows computers. When used with an eye-tracking device, OptiKey's on-screen keyboard allows MND patients to complete tasks, such as email composition, using only their eyes. OptiKey can also be used with a mouse or webcam. Unlike expensive and unreliable Alternative and Augmentative Communication (AAC) products that are difficult to use, Optikey is free, reliable, and easy to use.

The purpose of this fork is to make the keyboard layouts used in the 
original OptiKey distribution configurable. One application of this 
configurability is to allow users with motor disabilities more easily 
write computer programming language source code without the need of 
repeatedly switching keyboard screens to type non-alphanumeric characters. 
See the usage below the *Getting Started* section for information on 
how to configure the customizable keymaps for use with OptiKey. 
Users with additional special needs related to custom keymaps for use 
with this application should contact the developer at maxieds@gmail.com. 

# Configuring custom keyboards with OptiKey

The primary end modification goal of this fork is to create custom keyboard layouts for users who write text with 
OptiKey in computer programming languages. As such our configuration instructions are based around the new self-documenting 
[examples](https://github.com/maxieds/OptiKey/blob/master/keymaps/OptiKey_coders_keyboard_layout_v2.pdf) we have created for this purpose.
If you wish to modify our examples to suit your own custom needs for a keyboard layout set, see the referenced three XML 
files below and the [FunctionKeys enum](https://github.com/OptiKey/OptiKey/blob/master/src/JuliusSweetland.OptiKey/Enums/FunctionKeys.cs) 
to update the ``ACTION`` entries in the ``ActionKey`` entries in the sample file. You can run this keyboard layout configuration 
at the start up of OptiKey by changing the start keyboard to ``CustomKeyboard`` in the settings window (left click on the keyboard) and 
selecting these XML files placed in the Dynamic Keyboards directory listed below this setting in the panel. 

## Anna's Hacker Keyboard (Page 1)

<a href="https://github.com/maxieds/OptiKey/blob/master/keymaps/HackerKeyboard1.xml"><img src="https://github.com/maxieds/OptiKey/blob/master/screenshots/Screenshot-CodingKdb1.png" width="550" alt="HackerKeyboard1.xml" /></a>

## Anna's Hacker Keyboard (Page 2)

<a href="https://github.com/maxieds/OptiKey/blob/master/keymaps/HackerKeyboard2.xml"><img src="https://github.com/maxieds/OptiKey/blob/master/screenshots/Screenshot-CodingKdb2.png" width="550" alt="HackerKeyboard2.xml" /></a>

## Anna's Hacker Keyboard Symbols (Page 3)

<a href="https://github.com/maxieds/OptiKey/blob/master/keymaps/HackerKeyboardSymbols1.xml"><img src="https://github.com/maxieds/OptiKey/blob/master/screenshots/Screenshot-CodingKdb3.png" width="550" alt="HackerKeyboardSymbols1.xml" /></a>

## Screenshot of these keyboards placed in the dynamic keyboards directory

<img src="https://github.com/maxieds/OptiKey/blob/master/screenshots/Screenshot-CodingKdb4.png" width="550" alt="Dynamically Loaded Keyboards" />

<hr/> 

# Getting Started

[**The OptiKey Wiki**](https://github.com/OptiKey/OptiKey/wiki) contains OptiKey's user guides, installation and system requirements, and additional support information. OptiKey's Windows installer can be downloaded from the [latest release](https://github.com/JuliusSweetland/OptiKey/releases/latest). To get an understanding of OptiKey's use, users should watch [Optikey's introduction video](https://www.youtube.com/watch?v=HLkyORh7vKk).

# Supported Platforms

OptiKey uses the .Net 4.6 Framework and is designed to run on Windows 8 / 8.1 / 10.

# Problems?

If users encounter an issue with OptiKey, such as a software crash or an unexpected behaviour, users should add an issue ticket to [OptiKey's issue tracker](https://github.com/OptiKey/OptiKey/issues). Users are encouraged to check if their issue is already being tracked by OptiKey before creating a new issue ticket.

The following information should be specified in an issue ticket:

* How OptiKey was being used
* What the user expected to happen
* What the user actually experienced
* Steps to reproduce the issue
* Any other information that helps to describe and/or reproduce the problem (ex. screenshots)

# License

Licensed under the GNU GENERAL PUBLIC LICENSE (Version 3, 29th June 2007)

# Contact

To ask a question, or to discuss information that is not on the [**OptiKey Wiki**](https://github.com/JuliusSweetland/OptiKey/wiki/), please use <optikeyfeedback@gmail.com> to contact Julius.
