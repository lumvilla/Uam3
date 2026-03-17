using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace RpaServiceDesk.IniciarSesionSD
{
    public class ServicesDesk
    {
        private readonly string _url;
        private readonly string _usuario;
        private readonly string _password;
        private EdgeDriver _driver;
        private WebDriverWait _wait;
        public IWebDriver Driver => _driver;


        public ServicesDesk(string url, string usuario, string password)
        {
            _url = url;
            _usuario = usuario;
            _password = password;
        }

        public void IniciarSesion()
        {
            try
            {
                Console.WriteLine(" Iniciando Microsoft Edge...");

                var options = new EdgeOptions();
                options.AddArgument("--start-maximized"); // lo abro en toda la pantalla
                options.AddArgument("--disable-notifications");
                // activo el modo oculto (headless) descomenta
                options.AddArgument("headless");
                options.AddArgument("disable-gpu");

                _driver = new EdgeDriver(options);
                _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

                Console.WriteLine($" Navegando a {_url}");
                _driver.Navigate().GoToUrl(_url);

                Thread.Sleep(2000); 

                if (EstaEnPantallaLogin())
                {
                    Console.WriteLine("🔑 En pantalla de inicio de sesión. Intentando iniciar...");

                    var campoUsuario = _wait.Until(d => d.FindElement(By.Id("USERNAME")));
                    var campoPassword = _wait.Until(d => d.FindElement(By.Id("PIN")));

                    campoUsuario.Clear();
                    campoUsuario.SendKeys(_usuario);

                    campoPassword.Clear();
                    campoPassword.SendKeys(_password);

                    var botonLogin = _driver.FindElement(By.XPath("//input[@type='submit' or @value='Iniciar sesión' or @value='Ingresar']"));
                    botonLogin.Click();

                    Thread.Sleep(4000); 

                    if (!EstaEnPantallaLogin())
                        Console.WriteLine(" Sesión iniciada correctamente.");
                    else
                        Console.WriteLine(" No se pudo iniciar sesión. Revisa credenciales o ID de los campos.");
                }
                else
                {
                    Console.WriteLine(" Ya hay una sesion activa. Omitiendo login.");
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Error al iniciar sesión: {ex.Message}");
                Console.ResetColor();
            }
        }

        private bool EstaEnPantallaLogin()
        {
            try
            {
                return _driver.FindElements(By.Id("USERNAME")).Count > 0 &&
                       _driver.FindElements(By.Id("PIN")).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public void Cerrar()
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
                Console.WriteLine(" Navegador cerrado correctamente.");
            }
            catch { }
        }
    }
}
