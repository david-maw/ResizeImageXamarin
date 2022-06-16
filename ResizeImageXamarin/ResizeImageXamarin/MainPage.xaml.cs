﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Image = SixLabors.ImageSharp.Image;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace ResizeImageXamarin
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("ResizeImageXamarin.large.jpg");
            imageControl.Source = ImageSource.FromStream(() => stream);
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("ResizeImageXamarin.large.jpg");
            imageControl.Source = ImageSource.FromStream(() => stream);
            labelControl.Text = "Converting Image, please wait";
            await Task.Delay(1000);
            stream = assembly.GetManifestResourceStream("ResizeImageXamarin.large.jpg");
            using (Image image = await Image.LoadAsync(stream))
            {
                Debug.WriteLine($"Original image Width={image.Width}, Height={image.Height}");
                // We have to do a little dance here because it is possible that the exif orientation data says to rotate this image by 90 degrees
                // meaning the bitmap width is actually the height of the final image and vice versa
                int exifOrientation = 0;
                foreach (var item in image.Metadata.ExifProfile.Values)
                    if (item.Tag == SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Orientation)
                    {
                        exifOrientation = (UInt16)item.GetValue();
                        break;
                    }
                int newBitmapWidth = 0, newBitmapHeight = 0;
                if (exifOrientation > 4) // 6 is common but 5,7 & 8 all transpose width and height
                    newBitmapWidth = 1000;
                else
                    newBitmapHeight = 1000;
                DateTime started = DateTime.Now;
                image.Mutate(x => x
                    .Resize(newBitmapWidth, newBitmapHeight)
                    .Grayscale());
                TimeSpan timeSpan = DateTime.Now - started;
                Debug.WriteLine($"Mutated image Width={image.Width}, Height={image.Height} - took {timeSpan}");
                labelControl.Text = $"Image conversion took {timeSpan.TotalMilliseconds:F0} mS";

                var newStream = new MemoryStream();
                    await image.SaveAsJpegAsync(newStream, new JpegEncoder() { ColorType = JpegColorType.Luminance });
                    newStream.Position = 0;
                    imageControl.Source = ImageSource.FromStream(() => newStream);
            }

        }
    }
}
