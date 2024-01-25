using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MTWireGuard.Application.Models.Requests
{
    public class ChangeStateRequest
    {
        [FromRoute(Name = "id"), Required]
        public int Id { get; set; }

        [FromBody, Required]
        public bool Enabled { get; set; }
    }
}
