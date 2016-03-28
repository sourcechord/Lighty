# Lighty
Lighty is customizable lightbox library for WPF.

## Features

* Lighty shows dialog on the same visual tree using adorner.
    * Need not to create special elements for Lighty in your xaml code.
* 3way to show lightbox.
    * Show(): show lightbox in modeless format.
    * ShowAsync(): awaitable version of Show() method. 
    * ShowDialog(): Show light box modally, like a System.Windows.Window.ShowDialog() method.
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

### Preparation
#### code
Add using syntax for lighty namespace.
```cs
using SourceChord.Lighty;
```

#### XAML
Add xmlns to xaml code.
```xml
xmlns:lighty="clr-namespace:SourceChord.Lighty;assembly=Lighty"
```

### Show Lightbox

```cs
        private void OnClickButton(object sender, RoutedEventArgs e)
        {
            // show UserControl(SampleDialog is derived from UserControl.)
            LightBox.Show(this, new SampleDialog());

            // show FrameworkElement.
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            LightBox.Show(this, image);
        }
```


### Modal, Modeless, Async way to show lightbox

```cs
        private async void OnClickButton(object sender, RoutedEventArgs e)
        {
            // modeless dialog
            LightBox.Show(this, new SampleDialog());

            // async method
            await LightBox.ShowAsync(this, new SampleDialog());

            // modal dialog
            LightBox.ShowDialog(this, new SampleDialog());
        }
```



## Lisence
[MIT](LICENSE)
