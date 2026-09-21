using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using System.Collections;

namespace RoyalVilla_API.Controllers.v2
{
    [Route("api/v{version:apiVersion}/villa")]
    [ApiVersion("2.0")]  //this required 2 nuget packages for mvc versioning
    //[Authorize(Roles = "Customer,Admin")]
    [ApiController]
    public class VillaController : ControllerBase
    {

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<String>> GetVillas()
        {
            return "This is v2";
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<String>> GetVillaById(int id)
        {
            return "This is v2 with ID : "+id;
        }
    }
}