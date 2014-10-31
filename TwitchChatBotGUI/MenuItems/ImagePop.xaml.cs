using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Microsoft.TeamFoundation.MVVM;
using TwitchChatBot;

namespace TwitchChatBotGUI.MenuItems
{
    /// <summary>
    /// Interaction logic for ImagePop.xaml
    /// </summary>
    public partial class ImagePop : System.Windows.Controls.UserControl, ITwitchMenuItem
    {
        public ImagePop(TwitchBot inBot, Popup inCurrentPopup, System.Drawing.Bitmap inImage)
        {
            Bot = inBot;
            CurrentPopup = inCurrentPopup;
            
            if (inImage != null)
            {
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                MemoryStream memoryStream = new MemoryStream();
                inImage.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();
                src = bitmap;
                
            }

            InitializeComponent();
            if (src != null)
            {
                ImageQuestion.Source = src;
            }
        }

        System.Windows.Media.ImageSource src;

        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }
    }
}
