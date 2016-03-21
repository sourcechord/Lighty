# Lighty
Lighty is customizable lightbox library for WPF.

## Features

* Lighty shows dialog on the same visual tree using adorner.
    * Need not to create special elements for Lighty in your xaml code.
* 3way to show lightbox.
    * Show(): show lightbox in modeless format.
    * ShowAsync(): awaitable version of Show() method. 
    * ShowDialog(): Show light box modally, like a Window.MessageBox.ShowDialog() method.
* Can contain any FrameworkElement class in lightbox.(eg. Image / UserControl / etc...)
* Show/Close lightbox with animation.
* Lightbox style can be customized by setting default style of LightBox class.


## Install
Clone project and build it, then add reference to your own projects.

Or, you can install Lighty by Nuget.
```
Install-Package Lighty
```

## Usage
(T.B.D.)

## Lisence
[MIT](LICENSE)
