using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace RpaServiceDesk.ValidarEstadoOc
{
    public class EstadoOC
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public EstadoOC(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        }

        public void SeleccionarChangeOrder()
        {
            try
            {
                Console.WriteLine(" Buscando frame principal...");
                _wait.Until(d => d.SwitchTo().Frame(0));

                Console.WriteLine(" En frame principal. Buscando combo 'ticket_type'...");
                var comboTipo = _wait.Until(d => d.FindElement(By.Id("ticket_type")));
                var select = new SelectElement(comboTipo);
                select.SelectByValue("go_chg");

                Console.WriteLine(" Tipo de ticket cambiado a 'Change Order' correctamente.");
                _driver.SwitchTo().DefaultContent();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Error al seleccionar 'Change Order': {ex.Message}");
                Console.ResetColor();
            }
        }

        public void ProcesarMultiplesOCs()
        {
            Console.WriteLine("\n Ingrese uno o varios numeros de OC (separados por espacio o salto de linea).");
            Console.WriteLine("Cuando termine, deje una linea vacía y presione Enter:");
            Console.WriteLine("---------------------------------------------------------");

            var listaOCs = new List<string>();

            while (true)
            {
                var linea = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(linea)) break;

                var normalizada = Regex.Replace(linea, @"\s+", " ");
                normalizada = normalizada.Replace("\u00A0", " ").Trim();

                var partes = normalizada.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var oc in partes)
                {
                    var limpio = oc.Trim();
                    if (!string.IsNullOrEmpty(limpio))
                        listaOCs.Add(limpio);
                }
            }

            if (listaOCs.Count == 0)
            {
                Console.WriteLine(" No se ingresaron OCs validas.");
                return;
            }

            Console.WriteLine($"\n Se procesaran {listaOCs.Count} OC(s): {string.Join(", ", listaOCs)}\n");

            foreach (var oc in listaOCs)
            {
                BuscarOC(oc);
                Thread.Sleep(2000); 
            }

            Console.WriteLine("\n Todas las OCs fueron procesadas correctamente.");
        }

        public void BuscarOC(string numeroOC)
        {
            try
            {
                Console.WriteLine($" Buscando OC: {numeroOC}");
                _wait.Until(d => d.SwitchTo().Frame(0));

                var campoBusqueda = _wait.Until(d => d.FindElement(By.XPath("//form/table/tbody/tr/td[3]/input")));
                campoBusqueda.Clear();
                campoBusqueda.SendKeys(numeroOC);

                var botonGo = _wait.Until(d => d.FindElement(By.CssSelector("#imgBtn0 span")));
                botonGo.Click();

                _driver.SwitchTo().DefaultContent();
                Console.WriteLine("🕓 Esperando la ventana emergente...");

                string ventanaPrincipal = _driver.CurrentWindowHandle;
                string ventanaPopup = EsperarVentanaPopup(ventanaPrincipal, 10);

                if (ventanaPopup == null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" No se detecto ventana emergente para la OC {numeroOC}. Es posible que no exista.");
                    Console.ResetColor();
                    return;
                }

                _driver.SwitchTo().Window(ventanaPopup);
                Console.WriteLine("🪟 Ventana emergente detectada. Buscando estado...");

                Thread.Sleep(2000);

                _driver.SwitchTo().Frame(3);

                var posiblesEstados = new[] { "Complete", "Pending", "Rejected", "In Progress" };
                string estadoEncontrado = null;

                foreach (var estado in posiblesEstados)
                {
                    var elementos = _driver.FindElements(By.XPath($"//*[contains(text(), '{estado}')]"));
                    if (elementos.Any())
                    {
                        estadoEncontrado = estado;
                        break;
                    }
                }

                if (estadoEncontrado != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" Estado de la OC {numeroOC}: {estadoEncontrado}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" No se pudo determinar el estado de la OC {numeroOC} (sin texto conocido).");
                    Console.ResetColor();
                }

                _driver.Close();
                _driver.SwitchTo().Window(ventanaPrincipal);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Error al buscar la OC {numeroOC}: {ex.Message}");
                Console.ResetColor();
            }
        }

        private string EsperarVentanaPopup(string ventanaPrincipal, int segundos)
        {
            for (int i = 0; i < segundos * 2; i++)
            {
                var ventanas = _driver.WindowHandles;
                if (ventanas.Count > 1)
                    return ventanas.FirstOrDefault(h => h != ventanaPrincipal);

                Thread.Sleep(500);
            }
            return null;
        }
    }
}
