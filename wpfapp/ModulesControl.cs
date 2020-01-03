using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Navigation;

namespace wpfapp
{
    public class ModulesControl
    {
        public event EventHandler<ModuleLoadedEventArgs> Changed;

        private readonly HttpClient _httpClient;
        private Dictionary<string, IExchangeRates> _apiDictionary;
        private string[] _moduleFiles;

        // Indexer -> vrací API podle jména ze slovníku
        public IExchangeRates this[string sourceName] => _apiDictionary[sourceName];

        public List<string> GetModulesNames()
        {
            List<string> list = new List<string>(_apiDictionary.Count);
            foreach (var api in _apiDictionary)
            {
                list.Add(api.Key);
            }

            return list;
        }

        public List<string> GetSortedModulesNames()
        {
            List<string> list = new List<string>(_apiDictionary.Count);

            foreach (var api in _apiDictionary)
            {
                list.Add(api.Key);
            }

            return list;
        }

        public Dictionary<string, IExchangeRates> GetModules()
        {
            return _apiDictionary;
        }

        public ModulesControl()
        {
            _apiDictionary = new System.Collections.Generic.Dictionary<string, IExchangeRates>();
            _httpClient = new HttpClient();
            _moduleFiles = GetPluginFolderModulesList();
        }

        protected string[] GetPluginFolderModulesList()
        {
            var files = Directory.GetFiles(".\\plugins");
            return files;
        }

        public void LoadModules()
        {
            Debug.WriteLine($"ModulesControl.LoadModules()");
            foreach (var file in _moduleFiles)
            {
                try
                {
                    Debug.WriteLine($"Nahravam modul {file}.");
                    var asm = Assembly.LoadFrom(file);

                    var typeExchangeRates = asm.GetType("wpfapp.ExchangeRates");
                    if (typeExchangeRates.GetInterface("IExchangeRates") == null)
                    {
                        Trace.WriteLine($"Chyba v modulu {file}. Neni naimplementovano pozadovane rozhrani.");
                        ModuleLoadedEventArgs argsE = new ModuleLoadedEventArgs() { Error = true };
                        OnModuleLoaded(argsE);
                    }

                    var propApiName = typeExchangeRates.GetProperty("ApiName");

                    IExchangeRates exRates = (IExchangeRates)Activator.CreateInstance(typeExchangeRates, _httpClient);
                    string apiName = (string)propApiName.GetValue(exRates, null);

                    if (apiName == null)
                    {
                        Trace.WriteLine($"Chyba v modulu {file}. Knihovna nema nastavene jmeno.");
                        ModuleLoadedEventArgs argsE = new ModuleLoadedEventArgs() { Error = true };
                        OnModuleLoaded(argsE);
                    }

                    _apiDictionary.Add(apiName, exRates);

                    // vyvoláme událost OK
                    Trace.WriteLine($"Modul {file} byl uspesne nacten.");
                    ModuleLoadedEventArgs argsOK = new ModuleLoadedEventArgs() { Error = false };
                    OnModuleLoaded(argsOK);

                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Knihovnu '{file}' nelze načíst.");

                    // vyvoláme událost ERROR
                    ModuleLoadedEventArgs args = new ModuleLoadedEventArgs() { Error = true };
                    OnModuleLoaded(args);
                }

                // umělé čekání
                Thread.Sleep(1500);
            }
            Debug.WriteLine($"ModulesControl.LoadModules() - KONEC");
        }

        protected virtual void OnModuleLoaded(ModuleLoadedEventArgs e)
        {
            EventHandler<ModuleLoadedEventArgs> handler = Changed;
            if (Changed != null)
            {
                Changed(this, e);
            }
        }
    }
}
