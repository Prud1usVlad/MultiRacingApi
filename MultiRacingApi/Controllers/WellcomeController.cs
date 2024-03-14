using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Net;

namespace MultiRacingApi.Controllers
{
    [Route("")]
    [ApiController]
    public class WellcomeController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipStr = "";

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipStr = ip.ToString();
                }
            }

            return Ok("Welcome to the MultiRacingApi! \n Current ip is: " + ipStr);
        }
    }
}
