using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using NLog;

namespace wpfapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private static Logger logger;
        private Dictionary<string, IExchangeRates> _apiDictionary;

        private HttpClient _httpClient;
        public MainWindow()
        {
            InitializeComponent();
            _apiDictionary = new Dictionary<string, IExchangeRates>();
            _httpClient = new HttpClient();
            LoadLibrary("kb-api.dll");
        }

        public void LoadLibrary(string name)
        {
            try
            {
                var asm = Assembly.LoadFrom(name);

                var t = asm.GetTypes();

                var typeExchangeRates = asm.GetType("wpfapp.ExchangeRates");
                if (typeExchangeRates.GetInterface("IExchangeRates") == null)
                {
                    // vyvoláme výjimku
                }

                var propApiName = typeExchangeRates.GetProperty("ApiName");
                
                IExchangeRates exRates = (IExchangeRates) Activator.CreateInstance(typeExchangeRates, _httpClient);
                string apiName = (string) propApiName.GetValue(exRates, null);

                _apiDictionary.Add(apiName, exRates);

            }
            catch (Exception ex)
            {
                //logger.Error($"Knihovnu '{name}' nelze načíst.");
            }
        }
    }
}
