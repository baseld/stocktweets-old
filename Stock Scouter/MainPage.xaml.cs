using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Stock_Scouter.Models;

namespace Stock_Scouter
{
    public partial class MainPage : PhoneApplicationPage
    {
        private string _currentPivotTitle;

        public string CurrentPivotTitle
        {
            get
            {
                return this._currentPivotTitle;
            }
            set
            {
                this._currentPivotTitle = value;
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        /*
        // Handle selection changed on ListBox
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (MainListBox.SelectedIndex == -1)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/DetailPage.xaml?selectedItem=" + MainListBox.SelectedIndex, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            MainListBox.SelectedIndex = -1;
        }
        */
        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }
        //app bar add to watchlist
        private void addList_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddList.xaml", UriKind.Relative));
        }

        private void ContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {

        }
        
        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(Pivot.SelectedItemProperty.ToString());
            System.Diagnostics.Debug.WriteLine(Pivot.HeaderTemplateProperty.ToString());
        }
        
        private void SearchButton_onClick(object sender, RoutedEventArgs e)
        {
            
            //string sym = KeywordStr.Text;
            //System.Diagnostics.Debug.WriteLine(sym);
            /*Stock[] s = YahooFinance.CsvToStock("\"XOM\",\"Exxon Mobil Corpo\",\"4/17/2014\",100.42,99.69,100.97,99.69,15439810,\"84.79 - 101.74\",2.52,2.52,13.56");
            Portfolio p = new Portfolio();
            foreach (Stock item in s)
            {
                p.addStock(item);
            }*/
        }

    }
}