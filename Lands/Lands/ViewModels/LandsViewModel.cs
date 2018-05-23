﻿namespace Lands.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using Lands.Serivices;
    using Models;
    using Xamarin.Forms;

    public class LandsViewModel : BaseViewModel
    {

        #region Services

        ApiService apiService;

        #endregion

        #region Atributtes

        private ObservableCollection<Land> lands;

        bool isRefreshing;
        string filter;
        List<Land> landsList;

        #endregion

        #region Properties

        //es observableCollection por que la voy a pintar en un listview:
        public ObservableCollection<Land> Lands
        {
            get => lands;
            set
            {
                if (lands != value)
                {
                    lands = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Filter
        {
            get => filter;
            set
            {
                if (filter != value)
                {
                    filter = value;

                    OnPropertyChanged();

                    Search();
                }
            }
        }

        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                if (isRefreshing != value)
                {
                    isRefreshing = value;
                    OnPropertyChanged();
                }                     

            }

        }

        #endregion

        #region Contrustor

        public LandsViewModel()
        {
            apiService = new ApiService();
            LoadLands();
        }
        #endregion

        #region Commands

        public ICommand RefreshCommand
        {
            get => new RelayCommand(LoadLands);
        }

        public ICommand SearchCommand
        {
            get => new RelayCommand(Search);
        }

        

        #endregion

        #region Methods
        private async void LoadLands()
        {
            IsRefreshing = true;

            var connection = await apiService.CheckConnection();

            if (!connection.IsSuccess)
            {
                IsRefreshing = false;

                await Application.Current.MainPage.DisplayAlert("Error", connection.Message, "Accept");

                //aqui regreso al login page por si hay algun error de conneción con internet
                await Application.Current.MainPage.Navigation.PopAsync();

                return;
            }

            var response = await apiService.GetList<Land>("http://restcountries.eu", "/rest", "/v2/all");

            if (!response.IsSuccess)
            {
                IsRefreshing = false;

                await Application.Current.MainPage.DisplayAlert("Error", response.Message, "Accept");

                //aqui regreeso al longin page si hay algún error con el consumo de datos desde la api...
                await Application.Current.MainPage.Navigation.PopAsync();

                return;
            }

            landsList = (List<Land>)response.Result;

            this.Lands = new ObservableCollection<Land>(landsList);

            IsRefreshing = false;
        }

        private void Search()
        {
            if (string.IsNullOrEmpty(Filter))
            {
                Lands = new ObservableCollection<Land>(landsList);
            }
            else
            {
                Lands = new ObservableCollection<Land>(landsList.Where(
                    l => l.Name.ToLower().Contains(Filter.ToLower()) 
                      ||
                   l.Capital.ToLower().Contains(Filter.ToLower())   
                ));

            }
        }

        #endregion
    }
}
