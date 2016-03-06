﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace SourceChord.Lighty.Common
{
    public static class StoryboardExtensions
    {
        public static Task<bool> BeginAsync(this Storyboard storyboard, FrameworkElement target)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (storyboard != null && target != null)
            {
                var animation = storyboard.Clone();
                animation.Completed += (s, e) =>
                {
                    tcs.SetResult(true);
                };
                animation.Freeze();
                animation.Begin(target);
            }
            else
            {
                tcs.SetResult(false);
            }

            return tcs.Task;
        }
    }
}