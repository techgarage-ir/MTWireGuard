﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MTWireGuard.Models.Requests
{
    public class ChangeStateRequest
    {
        [FromQuery(Name = "ID"), Required]
        public int Id { get; set; }
        [FromQuery(Name = "IsEnabled"), Required]
        public bool Enabled { get; set; }
    }
}