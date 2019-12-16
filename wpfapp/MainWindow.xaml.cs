using System;
using System.Collections.Generic;
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
        private ModulesControl modulesControl;

        private List<ForeignCurrency> actualCurrencies;
        public int ModulesCountError { get; set; }
        public int ModulesCountOk { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            modulesControl = new ModulesControl();
            modulesControl.Changed += ModuleLoadNotifyHandler;
            LoadModulesInNewThread();
        }

        public void LoadModulesInNewThread()
        {
            Thread t1 = new Thread(modulesControl.LoadModules);
            t1.Start();
        }

        // obsluha události
        public void ModuleLoadNotifyHandler(object sender, ModuleLoadedEventArgs e)
        {
            if (e.Error) ModulesCountError++;
            else ModulesCountOk++;

            // přepis GUI z jiného vlákna
            if (Application.Current.Dispatcher != null) Application.Current.Dispatcher.Invoke(() =>
            {
                tbOk.Text = ModulesCountOk.ToString();
                tbError.Text = ModulesCountError.ToString();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var seznam = modulesControl.getModulesNames();
            var cnb = modulesControl["ČNB"];
            actualCurrencies = cnb.Get();
            dgMoney.ItemsSource = actualCurrencies;
        }

        private void Button_SaveToXml_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement elementCurrencies = doc.CreateElement(string.Empty, "currencies", string.Empty);
            doc.AppendChild(elementCurrencies);

            foreach (var currency in actualCurrencies)
            {
                XmlElement elementCurr = doc.CreateElement(string.Empty, "currency", string.Empty);

                XmlElement mena = doc.CreateElement(string.Empty, "code", string.Empty);
                XmlText text1 = doc.CreateTextNode(currency.Mena);
                mena.AppendChild(text1);

                XmlElement zeme = doc.CreateElement(string.Empty, "country", string.Empty);
                XmlText zemeText = doc.CreateTextNode(currency.Zeme);
                zeme.AppendChild(zemeText);

                XmlElement mnozstvi = doc.CreateElement(string.Empty, "amount", string.Empty);
                XmlText mnozstviText = doc.CreateTextNode(currency.Mnozstvi.ToString());
                mnozstvi.AppendChild(mnozstviText);

                XmlElement devNakup = doc.CreateElement(string.Empty, "exBuy", string.Empty);
                XmlText devNakupText = doc.CreateTextNode(currency.DevizaNakup.ToString());
                devNakup.AppendChild(devNakupText);

                XmlElement devProdej = doc.CreateElement(string.Empty, "exSell", string.Empty);
                XmlText devProdejText = doc.CreateTextNode(currency.DevizaProdej.ToString());
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
    }
}
