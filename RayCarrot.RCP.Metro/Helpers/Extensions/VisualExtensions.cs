using System;
using System.Windows;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Visual"/>
    /// </summary>
    public static class VisualExtensions
    {
        /// <summary>
        /// Gets the first descendant element by type
        /// </summary>
        /// <typeparam name="T">The type of element to get</typeparam>
        /// <param name="element">The element to get the type from</param>
        /// <returns>The element</returns>
        public static T GetDescendantByType<T>(this Visual element) 
            where T : class
        {
            return element.GetDescendantByType(typeof(T)) as T;
        }

        /// <summary>
        /// Gets the first descendant element by type
        /// </summary>
        /// <param name="element">The element to get the type from</param>
        /// <param name="descendantType">The type of element to get</param>
        /// <returns>The element</returns>
        public static object GetDescendantByType(this Visual element, Type descendantType)
        {
            if (element == null)
                return default;

            if (element.GetType() == descendantType)
                return element;

            if (element is FrameworkElement frameworkElement)
                frameworkElement.ApplyTemplate();

            object foundElement = null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = visual.GetDescendantByType(descendantType);

                if (foundElement != null)
                    break;
            }

            return foundElement;
        }
    }
}