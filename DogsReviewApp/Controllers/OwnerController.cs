﻿using AutoMapper;
using DogsReviewApp.Dto;
using DogsReviewApp.Interfaces;
using DogsReviewApp.Repository;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Models;

namespace DogsReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : Controller
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public OwnerController(IOwnerRepository ownerRepository, ICountryRepository countryRepository, IMapper mapper)
        {
            _ownerRepository = ownerRepository;
            _countryRepository = countryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Owner>))]
        public IActionResult GetOwners()
        {
            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepository.GetOwners());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(owners);
        }

        [HttpGet("{ownerId}")]
        [ProducesResponseType(200, Type = typeof(Owner))]
        [ProducesResponseType(400)]
        public IActionResult GetOwner(int ownerId)
        {
            if (!_ownerRepository.OwnersExist(ownerId))
                return NotFound();

            var owner = _mapper.Map<OwnerDto>(_ownerRepository.GetOwner(ownerId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(owner);
        }

        [HttpGet("{ownerId}/dog")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Dog>))]
        [ProducesResponseType(400)]
        public IActionResult GetDogByOwner(int ownerId)
        { 
            if(!_ownerRepository.OwnersExist(ownerId))
                return NotFound();

            var dogs = _mapper.Map<List<DogDto>>(_ownerRepository.GetDogByOwner(ownerId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(dogs);
        }

        [HttpGet("{dogId}/owner")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Owner>))]
        [ProducesResponseType(400)]
        public IActionResult GetOwnerByDog(int dogId)
        {
            if (!_ownerRepository.OwnersExist(dogId))
                return NotFound();

            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepository.GetOwnerOfADog(dogId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(owners);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateOwner([FromQuery] int countryId, [FromBody] OwnerDto ownerCreate)
        {
            if (ownerCreate == null)
                return BadRequest(ModelState);

            var owners = _ownerRepository.GetOwners()
                .Where(c => c.LastName.Trim().ToUpper() == ownerCreate.LastName.TrimEnd().ToUpper())
                .FirstOrDefault();

            if (owners != null)
            {
                ModelState.AddModelError("", "Owner already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ownerMap = _mapper.Map<Owner>(ownerCreate);

            ownerMap.Country = _countryRepository.GetCountry(countryId);

            if (!_ownerRepository.CreateOwner(ownerMap))
            {
                ModelState.AddModelError("", "Something went wrong while savin");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully created");
        }

        [HttpPut("{ownerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateOwner(int ownerId, [FromBody] OwnerDto updateOwner)
        {
            if (updateOwner == null)
                return BadRequest(ModelState);

            if (ownerId != updateOwner.Id)
                return BadRequest(ModelState);

            if (!_countryRepository.CountryExist(ownerId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var ownerMap = _mapper.Map<Owner>(updateOwner);

            if (!_ownerRepository.UpdateOwner(ownerMap))
            {
                ModelState.AddModelError("", "Something went wrong to updating owner!");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{ownerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteOwner(int ownerId)
        {
            if (!_ownerRepository.OwnersExist(ownerId))
                return NotFound();

            var ownerToDelete = _ownerRepository.GetOwner(ownerId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_ownerRepository.DeleteOwner(ownerToDelete))
            {
                ModelState.AddModelError("", "Something went wrong to delete owner!");
            }

            return NoContent();
        }
    }
}
