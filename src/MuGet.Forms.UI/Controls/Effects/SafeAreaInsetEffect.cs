using System;
using System.Linq;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Effects
{
    [Flags]
    public enum SafeAreaInsets
    {
        None = 0,
        Left = 1,
        Top = 2 << 0,
        Right = 2 << 1,
        Bottom = 2 << 2,
        All = 2 << 3,
    }

    public class SafeAreaInsetEffect : RoutingEffect
    {
        public SafeAreaInsetEffect() : base($"MuGet.Effects.{nameof(SafeAreaInsetEffect)}")
        {
        }

        public static readonly BindableProperty InsetsProperty =
            BindableProperty.CreateAttached(
                "Insets",
                typeof(SafeAreaInsets),
                typeof(SafeAreaInsetEffect),
                SafeAreaInsets.None,
                propertyChanged: OnInsetsChanged);

        public static SafeAreaInsets GetInsets(BindableObject view)
        {
            return (SafeAreaInsets)view.GetValue(InsetsProperty);
        }

        public static void SetInsets(BindableObject view, SafeAreaInsets value)
        {
            view.SetValue(InsetsProperty, value);
        }

        private static void OnInsetsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is VisualElement element)
            {
                // Automatically add the effect to the view once the property is attached
                var toRemove = element.Effects.FirstOrDefault(e => e is SafeAreaInsetEffect);
                if (toRemove != null)
                {
                    element.Effects.Remove(toRemove);
                }

                var insets = newValue as SafeAreaInsets?;
                if (insets.HasValue)
                {
                    element.Effects.Add(new SafeAreaInsetEffect());
                }
            }
        }
    }
}
