using System;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace AddNow.Views
{
    public sealed partial class HomePage : Page
    {
        private readonly MainPage mainPage;

        private CancellationTokenSource cancellationTokenSource;

        public HomePage()
        {
            InitializeComponent();

            // do cache the state of the UI when suspending/navigating
            // this is necessary for AddNow when navigating
            NavigationCacheMode = NavigationCacheMode.Required;

            SizeChanged += Page_SizeChanged;
            Loaded += Page_Loaded;

            mainPage = MainPage.CurrentMainPage;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetPageContentStackPanelWidth();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPageContentStackPanelWidth();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here

            // code here
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // code here

            // code here
        }

        private void SetPageContentStackPanelWidth()
        {
            FirstAddendTextBox.Width = SecondAddendTextBox.Width = SumTextBox.Width = ActualWidth -
                PageContentScrollViewer.Margin.Left -
                PageContentScrollViewer.Padding.Right;
        }

        #region MenuAppBarButton
        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mainPage.GoToHomePage();
            mainPage.MenuNavigationListView.SelectedIndex = 0;
        }
        #endregion MenuAppBarButton

        private async void AddButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            #region check first addend input
            string stringFirstAddend = FirstAddendTextBox.Text.Trim();
            if (string.IsNullOrEmpty(stringFirstAddend))
            {
                mainPage.NotifyUser("First addend is missing.", NotifyType.ErrorMessage);
                return;
            }

            StringBuilder firstAddendStringBuilder = new StringBuilder();
            bool firstAddendCanAddDigit = false;
            bool firstAddendHasDetectedMinusSign = false;
            bool firstAddendHasMinusSign = false;
            foreach (char character in stringFirstAddend)
            {
                if (!firstAddendHasDetectedMinusSign && !firstAddendCanAddDigit)
                {
                    if (character == '-')
                    {
                        firstAddendHasMinusSign = true;
                        firstAddendHasDetectedMinusSign = true;
                    }
                }
                if (char.IsDigit(character))
                {
                    //remove leading zeroes
                    if (!firstAddendCanAddDigit)
                    {
                        if (character != '0')
                        {
                            firstAddendCanAddDigit = true;
                        }
                    }
                    if (firstAddendCanAddDigit)
                    {
                        firstAddendStringBuilder.Append(character);
                    }
                }
            }
            if (firstAddendHasMinusSign)
            {
                firstAddendStringBuilder.Insert(0, '-');
            }
            stringFirstAddend = firstAddendStringBuilder.ToString();

            if (FirstAddendTextBox.Text != stringFirstAddend)
            {
                FirstAddendTextBox.Text = stringFirstAddend;
                if (string.IsNullOrEmpty(stringFirstAddend))
                {
                    mainPage.NotifyUser("First addend is missing.", NotifyType.ErrorMessage);
                    return;
                }
            }
            #endregion check first addend input 

            #region check second addend input
            string stringSecondAddend = SecondAddendTextBox.Text.Trim();
            if (string.IsNullOrEmpty(stringSecondAddend))
            {
                mainPage.NotifyUser("Second addend is missing.", NotifyType.ErrorMessage);
                return;
            }

            StringBuilder secondAddendStringBuilder = new StringBuilder();
            bool secondAddendCanAddDigit = false;
            bool secondAddendHasDetectedMinusSign = false;
            bool secondAddendHasMinusSign = false;
            foreach (char character in stringSecondAddend)
            {
                if (!secondAddendHasDetectedMinusSign && !secondAddendCanAddDigit)
                {
                    if (character == '-')
                    {
                        secondAddendHasMinusSign = true;
                        secondAddendHasDetectedMinusSign = true;
                    }
                }
                if (char.IsDigit(character))
                {
                    //remove leading zeroes
                    if (!secondAddendCanAddDigit)
                    {
                        if (character != '0')
                        {
                            secondAddendCanAddDigit = true;
                        }
                    }
                    if (secondAddendCanAddDigit)
                    {
                        secondAddendStringBuilder.Append(character);
                    }
                }
            }
            if (secondAddendHasMinusSign)
            {
                secondAddendStringBuilder.Insert(0, '-');
            }
            stringSecondAddend = secondAddendStringBuilder.ToString();

            if (SecondAddendTextBox.Text != stringSecondAddend)
            {
                SecondAddendTextBox.Text = stringSecondAddend;
                if (string.IsNullOrEmpty(stringSecondAddend))
                {
                    mainPage.NotifyUser("Second addend is missing.", NotifyType.ErrorMessage);
                    return;
                }
            }
            #endregion check second addend input 


            if (AddButton.Content.ToString() == "Add")
            {
                AddButton.Content = "Cancel";
            }
            else
            {
                AddButton.Content = "Add";
                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenSource.Cancel();
                }
            }

            string stringSum = string.Empty;

            StartProgressRing();

            SumTextBox.Text = stringSum;

            try
            {
                //add cancellationToken
                await Task.Run(async () => stringSum = await AddStringsAsync(stringFirstAddend, stringSecondAddend), cancellationToken);
            }
            catch (TaskCanceledException tcex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.NotifyUser(tcex.Message, NotifyType.ErrorMessage);
                    stringSum = string.Empty;
                    SumTextBox.Text = stringSum;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    stringSum = string.Empty;
                    SumTextBox.Text = stringSum;
                });
            }
            finally
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AddButton.Content = "Add";

                    SumTextBox.Text = stringSum;

                    SumTextBox_SelectionChanged(null, new RoutedEventArgs());

                    StopProgressRing();
                });
            }
        }

        private Task<string> AddStringsAsync(string stringFirstAddend, string stringSecondAddend)
        {
            BigInteger bigIntegerFirstAddend = BigInteger.Parse(stringFirstAddend);
            BigInteger bigIntegerSecondAddend = BigInteger.Parse(stringSecondAddend);
            BigInteger bigIntegerSum = bigIntegerFirstAddend + bigIntegerSecondAddend;
            return Task.FromResult(bigIntegerSum.ToString());

        }

        private void StartProgressRing()
        {
            AddProgressRing.IsActive = true;
            AddProgressRing.Visibility = Visibility.Visible;
        }

        private void StopProgressRing()
        {
            AddProgressRing.IsActive = false;
            AddProgressRing.Visibility = Visibility.Collapsed;
        }

        private void FirstAddendTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (FirstAddendTextBox.Text.Length == 1)
            {
                FirstAddendTextBox.Header = string.Format("Addend is {0} digit.", FirstAddendTextBox.Text.Length);
            }
            else
            {
                FirstAddendTextBox.Header = string.Format("Addend is {0} digits.", FirstAddendTextBox.Text.Length);
            }
        }

        private void SecondAddendTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (SecondAddendTextBox.Text.Length == 1)
            {
                SecondAddendTextBox.Header = string.Format("Addend is {0} digit.", SecondAddendTextBox.Text.Length);
            }
            else
            {
                SecondAddendTextBox.Header = string.Format("Addend is {0} digits.", SecondAddendTextBox.Text.Length);
            }
        }

        private void SumTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (SumTextBox.Text.Length == 1)
            {
                SumTextBox.Header = string.Format("Sum is {0} digit.", SumTextBox.Text.Length);
            }
            else
            {
                SumTextBox.Header = string.Format("Sum is {0} digits.", SumTextBox.Text.Length);
            }
        }
    }
}

