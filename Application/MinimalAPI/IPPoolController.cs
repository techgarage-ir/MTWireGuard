using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MTWireGuard.Application.Models.Models.Responses;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Models;
using MikrotikAPI.Models;

namespace MTWireGuard.Application.MinimalAPI
{
    internal class IPPoolController
    {
        public static async Task<Results<Ok<List<IPPoolViewModel>>, NotFound>> GetAll([FromServices] IMikrotikRepository API)
        {
            var ippools = await API.GetIPPools();
            return ippools.Any() ? TypedResults.Ok(ippools) : TypedResults.NotFound();
        }

        public static async Task<Ok<ToastMessage>> Create(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromBody] CreatePoolRequest request)
        {
            var model = mapper.Map<PoolCreateModel>(request);
            var make = await API.CreateIPPool(model);
            var message = mapper.Map<ToastMessage>(make);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> Update(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            int id,
            [FromBody] UpdateIPPoolRequest request)
        {
            request.Id = id;
            var model = mapper.Map<PoolUpdateModel>(request);
            var update = await API.UpdateIPPool(model);
            var message = mapper.Map<ToastMessage>(update);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> Delete(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            int id)
        {
            var delete = await API.DeleteIPPool(id);
            var message = mapper.Map<ToastMessage>(delete);
            return TypedResults.Ok(message);
        }
    }
}
