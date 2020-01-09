using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
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
using System.Xml;
using shared;

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
        private ModulesControl _modulesControl;
        private List<ForeignCurrency> _actualCurrencies;
        private CultureInfo ci;
        
        public int ModulesError { get; set; }
        public int ModulesOk { get; set; }



        public MainWindow()
        {
            InitializeComponent();
            _modulesControl = new ModulesControl();
            _modulesControl.Changed += ModuleLoadNotifyHandler;
            ci = new CultureInfo("en-US");
            LoadModulesInNewThread();
        }

        public void LoadModulesInNewThread()
        {
            Thread t1 = new Thread(_modulesControl.LoadModules);
            t1.Start();
        }

        // obsluha události
        public void ModuleLoadNotifyHandler(object sender, ModuleLoadedEventArgs e)
        {
            _apiDictionary = _modulesControl.GetModules();


            if (e.Error) ModulesError++;
            else ModulesOk++;

            // přepis GUI z jiného vlákna
            if (Application.Current.Dispatcher != null) Application.Current.Dispatcher.Invoke(() =>
            {
                comboBox.ItemsSource = _apiDictionary;
                tbOk.Text = ModulesOk.ToString();
                tbError.Text = ModulesError.ToString();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedPair = (KeyValuePair<string, IExchangeRates>) comboBox.SelectedItem;
            IExchangeRates selectedValue = selectedPair.Value;
            _actualCurrencies = selectedValue.Get();
            dgMoney.ItemsSource = _actualCurrencies;
        }

        private void Button_SaveToXml_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement elementCurrencies = doc.CreateElement(string.Empty, "currencies", string.Empty);
            XmlAttribute attrDate = doc.CreateAttribute("date");
            attrDate.Value = _actualCurrencies[0].Datum.ToString(ci);
            elementCurrencies.Attributes.Append(attrDate);
            doc.AppendChild(elementCurrencies);

            foreach (var currency in _actualCurrencies)
            {
                XmlElement elementCurr = doc.CreateElement(string.Empty, "currency", string.Empty);

                XmlElement mena = doc.CreateElement(string.Empty, "code", string.Empty);
                XmlText text1 = doc.CreateTextNode(currency.Mena);
                mena.AppendChild(text1);

                XmlElement zeme = doc.CreateElement(string.Empty, "country", string.Empty);
                XmlText zemeText = doc.CreateTextNode(currency.Zeme);
                zeme.AppendChild(zemeText);

                XmlElement mnozstvi = doc.CreateElement(string.Empty, "amount", string.Empty);
                XmlText mnozstviText = doc.CreateTextNode(currency.Mnozstvi.ToString(ci));
                mnozstvi.AppendChild(mnozstviText);

                XmlElement devNakup = doc.CreateElement(string.Empty, "exBuy", string.Empty);
                XmlText devNakupText = doc.CreateTextNode(currency.DevizaNakup.ToString(ci));
                devNakup.AppendChild(devNakupText);

                XmlElement devProdej = doc.CreateElement(string.Empty, "exSell", string.Empty);
                XmlText devProdejText = doc.CreateTextNode(currency.DevizaProdej.ToString(ci));
                devProdej.AppendChild(devProdejText);

                elementCurr.AppendChild(mena);
                elementCurr.AppendChild(zeme);
                elementCurr.AppendChild(mnozstvi);
                elementCurr.AppendChild(devNakup);
                elementCurr.AppendChild(devProdej);

                elementCurrencies.AppendChild(elementCurr);
            }

            doc.Save("curr.xml");
        }

        private void Button_LoadFromXml_Click(object sender, RoutedEventArgs e)
        {
            _actualCurrencies = new List<ForeignCurrency>(50);

            XmlDocument doc = new XmlDocument();
            doc.Load("curr.xml");
            XmlNode node = doc.SelectSingleNode("currencies");
            DateTime date = Convert.ToDateTime(node.Attributes["date"].Value, ci);
            XmlNodeList prop = node.SelectNodes("currency");

            foreach (XmlNode n in prop)
            {
                string n_code = n.SelectSingleNode("code").InnerText;
                string n_country = n.SelectSingleNode("country").InnerText;
                decimal n_amount = Convert.ToDecimal(n.SelectSingleNode("amount").InnerText, ci);
                decimal n_exBuy = Convert.ToDecimal(n.SelectSingleNode("exBuy").InnerText, ci);
                decimal n_exSell = Convert.ToDecimal(n.SelectSingleNode("exSell").InnerText, ci);

                var fc = new ForeignCurrency()
                    { Datum = date, Zeme = n_country, Mena = n_code, 
                        Mnozstvi = n_amount, DevizaNakup = n_exBuy, DevizaProdej = n_exSell };
                _actualCurrencies.Add(fc);
            }

            dgMoney.ItemsSource = _actualCurrencies;
        }
    }
}
