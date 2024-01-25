using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MTWireGuard.Application.Models.Requests
{
    public class SyncUserRequest
    {
        [FromRoute(Name = "id"), Required]
        public int ID { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "PrivateKey is required.")]
        public string PrivateKey { get; set; }

        [Required(ErrorMessage = "PublicKey is required.")]
        public string PublicKey { get; set; }
    }
}
