using System.Windows;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace RayCarrot.RCP.Metro;

public class DropTargetHighlightAdorner : DropTargetAdorner
{
    public DropTargetHighlightAdorner(UIElement adornedElement, DropInfo dropInfo)
        : base(adornedElement, dropInfo)
    {
        // Have the pen be a bit thicker than the default
        Pen.Thickness = 4;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        UIElement visualTarget = DropInfo.VisualTarget;
        
        if (visualTarget == null) 
            return;

        Rect bounds = VisualTreeExtensions.GetVisibleDescendantBounds(visualTarget);
        Point location = visualTarget.TranslatePoint(bounds.Location, AdornedElement);
        Rect rect = new(location, bounds.Size);
        drawingContext.DrawRoundedRectangle(null, Pen, rect, 3, 3);
    }
}