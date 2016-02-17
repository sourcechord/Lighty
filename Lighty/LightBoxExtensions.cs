using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SourceChord.Lighty
{
    public static class LightBoxExtensions
    {
        public static void Close(this FrameworkElement dialog)
        {
            ApplicationCommands.Close.Execute(dialog, null);
        }
    }
}
