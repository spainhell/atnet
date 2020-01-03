using System;
using System.Collections.Generic;
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
            foreach (var file in _moduleFiles)
            {
                try
                {
                    var asm = Assembly.LoadFrom(file);

                    var typeExchangeRates = asm.GetType("wpfapp.ExchangeRates");
                    if (typeExchangeRates.GetInterface("IExchangeRates") == null)
                    {
                        throw new Exception("Knihovna neimplementuje požadované rozhraní.");
                    }

                    var propApiName = typeExchangeRates.GetProperty("ApiName");

                    IExchangeRates exRates = (IExchangeRates)Activator.CreateInstance(typeExchangeRates, _httpClient);
                    string apiName = (string)propApiName.GetValue(exRates, null);

                    if (apiName == null)
                    {
                        throw new Exception("Knihovna nemá nastaven název.");
                    }

                    _apiDictionary.Add(apiName, exRates);

                    // vyvoláme událost OK
                    ModuleLoadedEventArgs args = new ModuleLoadedEventArgs() { Error = false };
                    OnModuleLoaded(args);

                }
                catch (Exception ex)
                {
                    //logger.Error($"Knihovnu '{name}' nelze načíst.");

                    // vyvoláme událost ERROR
                    ModuleLoadedEventArgs args = new ModuleLoadedEventArgs() { Error = true };
                    OnModuleLoaded(args);
                }

                // umělé čekání
                Thread.Sleep(1500);
            }
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
