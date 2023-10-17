using Newtonsoft.Json;
using PizzaApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PizzaApp
{
    public partial class MainPage : ContentPage
    {
        List<Pizza> pizzas;

        List<string> pizzasFav = new List<string>();

        enum e_tri
        {
            TRI_AUCUN,
            TRI_NOM,
            TRI_PRIX,
            TRI_FAV
        }

        e_tri tri = e_tri.TRI_AUCUN;

        // Application.Current.Properties
        // Key <-> String

        const string KEY_TRI = "tri";
        const string KEY_FAV = "fav";


        // "pizzas.json 

        string tempFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");
        string jsonFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pizzas.json");

        public MainPage()
        {
            InitializeComponent();

            /* pizzasFav.Add("3 fromages");
             pizzasFav.Add("tartiflette");
             pizzasFav.Add("indienne"); */

            LoadFavList();

            if (Application.Current.Properties.ContainsKey(KEY_TRI))
            {
                tri = (e_tri)Application.Current.Properties[KEY_TRI];
                sortButton.Source = GetImageSourceFromTri(tri);

            }

            

            
            // string pizzasJson = "";
            listView.RefreshCommand = new Command((obj) =>
            {
                Console.WriteLine("RefreshCommand");
                DownloadData((pizzas) =>
                {
                    if (pizzas != null)
                    {
                        listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);

                    }

                    listView.IsRefreshing = false;

                });
            });

            listView.IsVisible = false;
            waitLayout.IsVisible = true;
            
            if (File.Exists(jsonFileName))
            {
                string pizzasJson = File.ReadAllText(jsonFileName);
                if (!String.IsNullOrEmpty(pizzasJson))
                {
                    pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzasJson);
                    listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);
                    listView.IsVisible = true;
                    waitLayout.IsVisible = false;

                }
                
            }

            Console.WriteLine("Etape 1");

            // Appel a ma fonction
            DownloadData((pizzas) =>
            {
                if(pizzas != null)
                {
                    listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);

                }

                listView.IsVisible = true;
                waitLayout.IsVisible = false;
                // listView.IsRefreshing = false;

            });


            // Thread
            
            Console.WriteLine("ETAPE 4");

        }
        
        public void DownloadData(Action<List<Pizza>> action)
        {

            const string LOL = "https://drive.google.com/uc?export=download&id=1DSuapxEmrP83_Gj6SxhzHaaZjLlKrElg";

            using (var webclient = new WebClient())
            {
                
                    // Thread Main (UI)
                    // pizzasJson = webclient.DownloadString(LOL);

                    Console.WriteLine("Etape 2");

                    webclient.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                    {
                        Console.WriteLine("ETAPE 5");

                        Console.WriteLine("Donnée téléchargée: " );

                        Exception ex = e.Error;

                        if (ex == null)
                        {
                            File.Copy(tempFileName, jsonFileName, true);

                            string pizzasJson = File.ReadAllText(jsonFileName);

                            pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzasJson);


                            // 


                            Device.BeginInvokeOnMainThread(() =>
                            {
                                action.Invoke(pizzas);
                            });
                        } 
                        else
                        {
                            Device.BeginInvokeOnMainThread(async() =>
                            {
                                await DisplayAlert("Erreur", "Une erreur réseau s'est produite: " + ex.Message, "OK");
                                action.Invoke(null);

                            });

                        }

                        
                    };
                    Console.WriteLine("Etape 3");

                    webclient.DownloadFileAsync(new Uri(LOL), tempFileName);

                
            }
        }

        void SortButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("SortButtonClicked");

            if (tri == e_tri.TRI_AUCUN)
            {
                tri = e_tri.TRI_NOM;

            } else if (tri == e_tri.TRI_NOM)
            {
                tri = e_tri.TRI_PRIX;
            }
            else if (tri == e_tri.TRI_PRIX)
            {
                tri = e_tri.TRI_FAV;
            }
            else if (tri == e_tri.TRI_FAV)
            {
                tri = e_tri.TRI_AUCUN;
            }

            sortButton.Source = GetImageSourceFromTri(tri);
            listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav); ;

            Application.Current.Properties[KEY_TRI] = (int)tri;
            Application.Current.SavePropertiesAsync();

        }

        private string GetImageSourceFromTri(e_tri t)
        {
            switch (t) {
                case e_tri.TRI_NOM:
                    return "sort_nom.png";
                case e_tri.TRI_PRIX:
                    return "sort_prix.png";
                case e_tri.TRI_FAV:
                    return "sort_fav.png";
            }

            return "sort_none.png";
        }

        private List<Pizza> GetPizzasFromTri(e_tri t, List<Pizza> l)
        {
            if (l == null)
            {
                return null;
            }


            switch (t)
            {
                case e_tri.TRI_NOM:
                    {
                        List<Pizza> ret = new List<Pizza>(l);

                        ret.Sort((p1, p2) => { return p1.Titre.CompareTo(p2.Titre); });

                        return ret;

                    }
                    
                case e_tri.TRI_PRIX:
                case e_tri.TRI_FAV:
                    {
                        List<Pizza> ret = new List<Pizza>(l);

                        ret.Sort((p1, p2) => { return p2.prix.CompareTo(p1.prix); });

                        return ret;

                    }
                   
            }

            return l;

        }
        private void OnFavPizzaChanged(PizzaCell pizzaCell)
        {
            // pizzasFav

            // Ajouter ou supprimer
            // Pizzacell.piza.nom
            // Pizzacell.isFavorite

            bool isInFavList = pizzasFav.Contains(pizzaCell.pizza.nom);

            if (pizzaCell.isFavorite && !isInFavList)
            {
                pizzasFav.Add(pizzaCell.pizza.nom);
                SaveFavList();
            }
            else if (!pizzaCell.isFavorite && isInFavList)
            {
                pizzasFav.Remove(pizzaCell.pizza.nom);
                SaveFavList();

            }

        }

        private List<PizzaCell> GetPizzaCells(List<Pizza> p, List<string> f)
        {
            List<PizzaCell> ret = new List<PizzaCell>();

            if (p == null)
            {
                return ret;
            }


            foreach(Pizza pizza in p)
            {
                bool isFav = f.Contains(pizza.nom);

                if (tri == e_tri.TRI_FAV)
                {
                    if (isFav)
                    {
                        ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnFavPizzaChanged });

                    }

                }
                else
                {
                    ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnFavPizzaChanged });

                }  
            }

            return ret;
        }

        // pizzasFav

        private void SaveFavList()
        {
            string json = JsonConvert.SerializeObject(pizzasFav);
            Application.Current.Properties[KEY_FAV] = json;
            Application.Current.SavePropertiesAsync();

        } 

        private void LoadFavList()
        {
            if (Application.Current.Properties.ContainsKey(KEY_FAV))
            {
                string json =  Application.Current.Properties[KEY_FAV].ToString();
                pizzasFav = JsonConvert.DeserializeObject<List<string>>(json);
            }

        }
    }
}
