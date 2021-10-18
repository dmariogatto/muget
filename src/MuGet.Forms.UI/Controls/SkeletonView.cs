using Xamarin.Forms;

namespace MuGet.Forms.UI.Controls
{
    public class SkeletonView : BoxView
    {
        private const string AnimationHandle = "FadeInOut";

        private readonly Animation _smoothAnimation;

        public SkeletonView()
        {
            BackgroundColor = ThemeManager.Current == MuGet.Services.Theme.Dark
                ? Color.DarkGray
                : Color.LightGray;

            _smoothAnimation = new Animation();
            _smoothAnimation.WithConcurrent((f) => Opacity = f, 0.30, 0.80, Easing.Linear);
            _smoothAnimation.WithConcurrent((f) => Opacity = f, 0.80, 0.30, Easing.Linear);

            Start();
        }

        public void Start()
        {
            if (IsVisible && !this.AnimationIsRunning(AnimationHandle))
                this.Animate(AnimationHandle, _smoothAnimation, 16, 2200, Easing.Linear, null, () => IsVisible);
        }

        public void Stop()
        {
            if (this.AnimationIsRunning(AnimationHandle))
                this.AbortAnimation(AnimationHandle);
        }
    }
}
