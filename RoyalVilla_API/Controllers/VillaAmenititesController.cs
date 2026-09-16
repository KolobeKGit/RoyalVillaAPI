using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa-amenities")]
    [ApiController]
    public class VillaAmenititesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAmenititesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmenitiesDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmenitiesDTO>>>> GetVillaAmenities()
        {
            var amenities = await _db.VillaAmenities.ToListAsync();
            var dtoResponseAmenities = _mapper.Map<List<VillaAmenitiesDTO>>(amenities);
            var response = ApiResponse<IEnumerable<VillaAmenitiesDTO>>.Ok(dtoResponseAmenities, "Villa amenities retrieved successfully");
            return Ok(response);
        }
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> GetVillaAmenitiesById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0"));
                }

                var villaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
                if (villaAmenities == null)
                {
                    return NotFound(ApiResponse<VillaAmenitiesDTO>.NotFound($"Villa amenities with ID {id} was not found"));
                }
                return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(villaAmenities), "Records retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while retrieving villa with ID {id}:", ex.Message);
                return StatusCode(500, errorResponse);
            }              
        }
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> CreateVillaAmenity(VillaAmenitiesCreateDTO villaAmenitiesDTO)
        {
            try
            {
                if (villaAmenitiesDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VIlla amenities data is required"));
                }

                var villaExists = await _db.Villa.FirstOrDefaultAsync(u => u.Id == villaAmenitiesDTO.VillaId);

                if (villaExists == null) 
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with ID {villaAmenitiesDTO.VillaId} does not exists"));
                }

                VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmenitiesDTO);
                villaAmenities.CreatedDate = DateTime.Now;
                await _db.VillaAmenities.AddAsync(villaAmenities);
                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaAmenitiesDTO>.CreatedAt(_mapper.Map<VillaAmenitiesDTO>(villaAmenities), "Villa anemity created successfully");
                return CreatedAtAction(nameof(CreateVillaAmenity), new { id = villaAmenities.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured while creating a villa amenities:", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> UpdateVillaAmenity(int id, VillaAmenitiesUpdateDTO villaAmenitiesDTO)
        {
            try
            {
                if (villaAmenitiesDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa anemity ID in URL does not match villa anemities ID in request body"));
                }

                if (id != villaAmenitiesDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa ID in URL does not match ID in request body"));
                }

                var villaExists = await _db.Villa.FirstOrDefaultAsync(u => u.Id == villaAmenitiesDTO.VillaId);

                if (villaExists == null)  
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with ID {villaAmenitiesDTO.VillaId} does not exists"));
                }

                var existingVillaAmenity = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVillaAmenity == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa amenity with ID {id} was not found"));
                }

                //var duplicateVillaAmenity = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Name.ToLower() == villaAmenitiesDTO.Name.ToLower()
                //&& u.Id != id);

                //if (duplicateVillaAmenity != null)  //This means if the duplicate exists
                //{
                //    return Conflict(ApiResponse<object>.Conflict($"Villa amenity with the name {villaAmenitiesDTO.Name} already exists"));
                //}

                _mapper.Map(villaAmenitiesDTO, existingVillaAmenity);
                existingVillaAmenity.UpdatedDate = DateTime.Now;

                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(existingVillaAmenity), "Villa updated successuflly");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured while updating a villa anemity:", ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAnemity(int id)
        {
            try
            {
                var existingVillaAnemity = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVillaAnemity == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa anemity with ID {id} was not found"));
                }

                _db.VillaAmenities.Remove(existingVillaAnemity);
                await _db.SaveChangesAsync();

                var response = ApiResponse<object>.NoContent("Villa anemity deleted siccessfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured while deleting a villa anemity:", ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }
    }
}
