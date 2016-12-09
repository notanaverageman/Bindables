using System.Windows;

namespace Bindables.Test.Attached
{
	public class Temp
	{
		public static int Prop { get; set; } = 1;

		public static DependencyProperty HoverBrushProperty =
		DependencyProperty.RegisterAttached("HoverBrush",
											typeof(int),
											typeof(Temp),
											new PropertyMetadata(null));
		public static void SetHoverBrush(DependencyObject obj, int value)
		{
			obj.SetValue(HoverBrushProperty, value);
		}
		public static int GetHoverBrush(DependencyObject obj)
		{
			return (int)obj.GetValue(HoverBrushProperty);
		}
	}
}