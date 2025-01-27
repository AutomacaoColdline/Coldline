using ColdlineAPI.Application.Interfaces;

namespace ColdlineAPI.Application.Services
{
    public class HelloWorldService : IHelloWorldService
    {
        public string GetHelloWorldMessage()
        {
            return "Hello World!";
        }
    }
}
