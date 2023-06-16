using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.CustomActionFilters;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;
using System.Collections.Generic;
using System.Text.Json;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext dbContext;
        private readonly IRegionRepository regionRepository;
        private readonly IMapper mapper;
        private readonly ILogger<RegionsController> logger;

        public RegionsController(NZWalksDbContext dbContext, 
            IRegionRepository regionRepository, 
            IMapper mapper,
            ILogger<RegionsController> logger)
        {
            this.dbContext = dbContext;
            this.regionRepository = regionRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        // Get All Regions
        // GET: https://localhost:portnumber/api/regions
        [HttpGet]
        [Authorize(Roles = "Reader, Writer")]
        public async Task<IActionResult> GetAll()
        {
            logger.LogInformation("GetAllRegions Action Method was invoked");
            
            // Get Data From Database - Domain Models
            var regionsDomain = await regionRepository.GetAllAsync();

            // Return DTOs

            logger.LogInformation($"Finished GetAllRegions request with data: {JsonSerializer.Serialize(regionsDomain)}");

            return Ok(mapper.Map<List<RegionDto>>(regionsDomain));
        }

        // Get Region By Id 
        // GET: https://localhost:portnumber/api/regions/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Reader, Writer")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            // Get Data From Database - Domain Model
            // The Find method it can only be used for the primary key (id) 
            var regionDomain = await regionRepository.GetByIdAsync(id);

            if(regionDomain == null)
            {
                return NotFound();
            }

            // Return DTO 
            return Ok(mapper.Map<RegionDto>(regionDomain));
        }

        // Create a new Region
        // POST: https://localhost:portnumber/api/regions
        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Create([FromBody] AddRegionRequestDto addRegionRequestDto)
        {
            if (ModelState.IsValid)
            {
                // Map DTO to Domain Model
                var regionDomain = mapper.Map<Region>(addRegionRequestDto);

                // Create Region with Domain Model 
                regionDomain = await regionRepository.CreateAsync(regionDomain);

                // Map Domain Model to DTO
                var regionDto = mapper.Map<RegionDto>(regionDomain);


                return CreatedAtAction(nameof(GetById), new { id = regionDto.Id }, regionDto);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        // Update a Region 
        // PUT: https://localhost:portnumber/api/regions/{id}
        [HttpPut]
        [Route("{id:Guid}")]
        [ValidateModel]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateRegionRequestDto updateRegionRequestDto)
        {
            if (ModelState.IsValid)
            {
                // Map Dto to Domain model 
                var regionDomain = mapper.Map<Region>(updateRegionRequestDto);

                // Check if Region exists 
                regionDomain = await regionRepository.UpdateAsync(id, regionDomain);

                if (regionDomain == null)
                {
                    return NotFound();
                }

                // Return Region DTO
                return Ok(mapper.Map<RegionDto>(regionDomain));
            }
            else
            {
                return BadRequest(ModelState);
            }

        }

        // Delete Region
        // DELETE: https://localhost:portnumber/api/regions/{id}
        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Delete([FromRoute] Guid id) 
        {
            var regionDomain = await regionRepository.DeleteAsync(id);

            if (regionDomain == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
