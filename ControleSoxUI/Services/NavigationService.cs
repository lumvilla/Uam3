using System;

namespace ControleSoxUI.Services
{
    public class NavigationService
    {
        public event Action<string>? NavigationRequested;

        public void NavigateTo(string destination)
        {
            NavigationRequested?.Invoke(destination);
        }
    }
}