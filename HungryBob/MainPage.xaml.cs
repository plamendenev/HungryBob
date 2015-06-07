using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace HungryBob
{
    public sealed partial class MainPage : Page
    {         

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            startingMusic.Play();
        }        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            startingMusic.Play();         
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PlayArea));
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void InstructionsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuButtonsText.Visibility = Visibility.Visible;
            if (AboutText.Visibility == Visibility.Visible)
                AboutText.Visibility = Visibility.Collapsed;
                
            InstructionsText.Visibility = Visibility.Visible;
        }        

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            MenuButtonsText.Visibility = Visibility.Visible;
            if (InstructionsText.Visibility == Visibility.Visible)
                InstructionsText.Visibility = Visibility.Collapsed;
            
            AboutText.Visibility = Visibility.Visible;    
        }

        private void MenuButtonsText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MenuButtonsText.Visibility == Visibility.Visible)
                MenuButtonsText.Visibility = Visibility.Collapsed;
        }
    }
}
