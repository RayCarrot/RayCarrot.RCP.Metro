using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro;

// For use instead of ClipBorder, see https://github.com/MahApps/MahApps.Metro/issues/4524
public class ClipGeometryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 5)
            return DependencyProperty.UnsetValue;

        if (values[0] is not double width
            || values[1] is not double height
            || values[2] is not CornerRadius cornerRadius
            || values[3] is not Thickness borderThickness
            || values[4] is not Thickness padding)
        {
            return DependencyProperty.UnsetValue;
        }

        if (width <= 0 || height <= 0)
            return DependencyProperty.UnsetValue;

        StreamGeometry clipGeometry = new();
        BorderInfo childBorderInfo = new(cornerRadius, borderThickness, padding, false);
        
        using (StreamGeometryContext ctx = clipGeometry.Open())
            GenerateGeometry(ctx, new Rect(0, 0, width, height), childBorderInfo);

        // Freeze the geometry for better performance
        clipGeometry.Freeze();

        return clipGeometry;
    }

    /// <inheritdoc />
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    internal static void GenerateGeometry(StreamGeometryContext ctx, Rect rect, BorderInfo radii)
    {
        //
        //  compute the coordinates of the key points
        //

        Point topLeft = new Point(radii.LeftTop, 0);
        Point topRight = new Point(rect.Width - radii.RightTop, 0);
        Point rightTop = new Point(rect.Width, radii.TopRight);
        Point rightBottom = new Point(rect.Width, rect.Height - radii.BottomRight);
        Point bottomRight = new Point(rect.Width - radii.RightBottom, rect.Height);
        Point bottomLeft = new Point(radii.LeftBottom, rect.Height);
        Point leftBottom = new Point(0, rect.Height - radii.BottomLeft);
        Point leftTop = new Point(0, radii.TopLeft);

        //
        //  check keypoints for overlap and resolve by partitioning radii according to
        //  the percentage of each one.  
        //

        //  top edge is handled here
        if (topLeft.X > topRight.X)
        {
            double v = (radii.LeftTop) / (radii.LeftTop + radii.RightTop) * rect.Width;
            topLeft.X = v;
            topRight.X = v;
        }

        //  right edge
        if (rightTop.Y > rightBottom.Y)
        {
            double v = (radii.TopRight) / (radii.TopRight + radii.BottomRight) * rect.Height;
            rightTop.Y = v;
            rightBottom.Y = v;
        }

        //  bottom edge
        if (bottomRight.X < bottomLeft.X)
        {
            double v = (radii.LeftBottom) / (radii.LeftBottom + radii.RightBottom) * rect.Width;
            bottomRight.X = v;
            bottomLeft.X = v;
        }

        // left edge
        if (leftBottom.Y < leftTop.Y)
        {
            double v = (radii.TopLeft) / (radii.TopLeft + radii.BottomLeft) * rect.Height;
            leftBottom.Y = v;
            leftTop.Y = v;
        }

        //
        //  add on offsets
        //

        Vector offset = new Vector(rect.TopLeft.X, rect.TopLeft.Y);
        topLeft += offset;
        topRight += offset;
        rightTop += offset;
        rightBottom += offset;
        bottomRight += offset;
        bottomLeft += offset;
        leftBottom += offset;
        leftTop += offset;

        //
        //  create the border geometry
        //
        ctx.BeginFigure(topLeft, true /* is filled */, true /* is closed */);

        // Top line
        ctx.LineTo(topRight, true /* is stroked */, false /* is smooth join */);

        // Upper-right corner
        double radiusX = rect.TopRight.X - topRight.X;
        double radiusY = rightTop.Y - rect.TopRight.Y;
        if (!Utils.IsZero(radiusX)
            || !Utils.IsZero(radiusY))
        {
            ctx.ArcTo(rightTop, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise, true, false);
        }

        // Right line
        ctx.LineTo(rightBottom, true /* is stroked */, false /* is smooth join */);

        // Lower-right corner
        radiusX = rect.BottomRight.X - bottomRight.X;
        radiusY = rect.BottomRight.Y - rightBottom.Y;
        if (!Utils.IsZero(radiusX)
            || !Utils.IsZero(radiusY))
        {
            ctx.ArcTo(bottomRight, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise, true, false);
        }

        // Bottom line
        ctx.LineTo(bottomLeft, true /* is stroked */, false /* is smooth join */);

        // Lower-left corner
        radiusX = bottomLeft.X - rect.BottomLeft.X;
        radiusY = rect.BottomLeft.Y - leftBottom.Y;
        if (!Utils.IsZero(radiusX)
            || !Utils.IsZero(radiusY))
        {
            ctx.ArcTo(leftBottom, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise, true, false);
        }

        // Left line
        ctx.LineTo(leftTop, true /* is stroked */, false /* is smooth join */);

        // Upper-left corner
        radiusX = topLeft.X - rect.TopLeft.X;
        radiusY = leftTop.Y - rect.TopLeft.Y;
        if (!Utils.IsZero(radiusX)
            || !Utils.IsZero(radiusY))
        {
            ctx.ArcTo(topLeft, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise, true, false);
        }
    }

    internal struct BorderInfo
    {
        #region Fields

        internal readonly double LeftTop;
        internal readonly double TopLeft;
        internal readonly double TopRight;
        internal readonly double RightTop;
        internal readonly double RightBottom;
        internal readonly double BottomRight;
        internal readonly double BottomLeft;
        internal readonly double LeftBottom;

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Encapsulates the details of each of the core points of the border which is calculated
        /// based on the given CornerRadius, BorderThickness, Padding and a flag to indicate whether
        /// the inner or outer border is to be calculated.
        /// </summary>
        /// <param name="corners">CornerRadius</param>
        /// <param name="borders">BorderThickness</param>
        /// <param name="padding">Padding</param>
        /// <param name="isOuterBorder">Flag to indicate whether outer or inner border needs 
        /// to be calculated</param>
        internal BorderInfo(CornerRadius corners, Thickness borders, Thickness padding, bool isOuterBorder)
        {
            double left = 0.5 * borders.Left + padding.Left;
            double top = 0.5 * borders.Top + padding.Top;
            double right = 0.5 * borders.Right + padding.Right;
            double bottom = 0.5 * borders.Bottom + padding.Bottom;

            if (isOuterBorder)
            {
                if (corners.TopLeft.IsZero())
                {
                    LeftTop = TopLeft = 0.0;
                }
                else
                {
                    LeftTop = corners.TopLeft + left;
                    TopLeft = corners.TopLeft + top;
                }

                if (corners.TopRight.IsZero())
                {
                    TopRight = RightTop = 0.0;
                }
                else
                {
                    TopRight = corners.TopRight + top;
                    RightTop = corners.TopRight + right;
                }

                if (corners.BottomRight.IsZero())
                {
                    RightBottom = BottomRight = 0.0;
                }
                else
                {
                    RightBottom = corners.BottomRight + right;
                    BottomRight = corners.BottomRight + bottom;
                }

                if (corners.BottomLeft.IsZero())
                {
                    BottomLeft = LeftBottom = 0.0;
                }
                else
                {
                    BottomLeft = corners.BottomLeft + bottom;
                    LeftBottom = corners.BottomLeft + left;
                }
            }
            else
            {
                LeftTop = Math.Max(0.0, corners.TopLeft - left);
                TopLeft = Math.Max(0.0, corners.TopLeft - top);
                TopRight = Math.Max(0.0, corners.TopRight - top);
                RightTop = Math.Max(0.0, corners.TopRight - right);
                RightBottom = Math.Max(0.0, corners.BottomRight - right);
                BottomRight = Math.Max(0.0, corners.BottomRight - bottom);
                BottomLeft = Math.Max(0.0, corners.BottomLeft - bottom);
                LeftBottom = Math.Max(0.0, corners.BottomLeft - left);
            }
        }

        #endregion
    }
}