﻿using System;
using System.Windows.Media.Imaging;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    class ScreenCaputredEventArgs : EventArgs
    {
        public BitmapSource Bmp { get; private set; }

        public ScreenCaputredEventArgs(BitmapSource bmp)
        {
            Bmp = bmp;
        }
    }
}
