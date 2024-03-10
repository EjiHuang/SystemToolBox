using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    class ScreenCaputre
    {
        public void StartCaputre(int timeOutSeconds)
        {
            StartCaputre(timeOutSeconds, null);
        }

        public void StartCaputre(int timeOutSeconds, Size? defaultSize)
        {
            var mask = new MaskWindow(this);
            mask.Show(timeOutSeconds, defaultSize);
        }

        public event EventHandler<ScreenCaputredEventArgs> ScreenCaputred;
        public event EventHandler<EventArgs> ScreenCaputreCancelled;

        internal void OnScreenCaputred(object sender, BitmapSource caputredBmp)
        {
            if (null != ScreenCaputred)
                ScreenCaputred(sender, new ScreenCaputredEventArgs(caputredBmp));
        }

        internal void OnScreenCaputredCancelled(object sender)
        {
            if (null != ScreenCaputreCancelled)
                ScreenCaputreCancelled(sender, EventArgs.Empty);
        }
    }
}
