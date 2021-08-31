﻿using System;
using System.Globalization;
using System.Windows.Media;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    public class GenericIconToBrushConverter : BaseValueConverter<GenericIconToBrushConverter, GenericIconKind, Brush>
    {
        public override Brush ConvertValue(GenericIconKind value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((GenericIcon)App.Current.FindResource($"GenericIcons.{value}")).IconColor;
        }
    }
}