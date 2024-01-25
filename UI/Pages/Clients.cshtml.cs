using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using Newtonsoft.Json;

namespace MTWireGuard.Pages
{
    public class ClientsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
