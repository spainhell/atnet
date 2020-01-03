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
        public int ModulesError { get; set; }
        public int ModulesOk { get; set; }


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
            _apiDictionary = modulesControl.GetModules();


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
            var selectedValue = selectedPair.Value;
            selectedValue.Get();


        }
    }
}
