using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Visual.cs.Components
{
    class AboutDialog : Window
    {
        Grid bodyGrid;
        Button okButton;
        Image aboutImage;
        public AboutDialog()
        {
            bodyGrid = new Grid();
            this.Height = 200;
            this.Width = 150;
            bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
            bodyGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.Content = bodyGrid;
            initGridComps();
        }

        private void initGridComps()
        {
            BitmapImage bmpImg = new BitmapImage();
            bmpImg.BeginInit();
            bmpImg.UriSource = new Uri("https://img.bhs4.com/04/5/04501799668e96b8da278ef17c31619a4a35a039_large.jpg", UriKind.Absolute);
            bmpImg.EndInit();
            aboutImage = new Image();
            aboutImage.Source = bmpImg;
            Grid.SetRow(aboutImage, 0);
            okButton = new Button();
            okButton.Content = "OK";
            okButton.Click += okBtnClick;
            okButton.VerticalAlignment = VerticalAlignment.Bottom;
            Grid.SetRow(okButton, 1);

            bodyGrid.Children.Add(okButton);
            bodyGrid.Children.Add(aboutImage);
        }

        private void okBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
