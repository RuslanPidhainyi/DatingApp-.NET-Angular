﻿using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
{
   [HttpGet] //Ządania HTTP Get //Metoda do zwracania odpowiedzi HTTP  do klienta 
   public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams) /*publiczna metoda.

                                                         Result action - jako typ rzeczy, ktore zamierzamy zwrocic z tego  punktu koncowego API

                                                         Następnie okreslamy typ  zwracanej rzeczy  - teraz zwrocimy liste użytkownikow (Istneje  wiele róznych  rodzajów list w C#, jedna z metod, ktorej mozemy do tego użyc jest enumarable - uzywane tylko dla kolekcji okreslonego typu) - zwrocimy IEnumereble typu appUser*/
   {
      userParams.CurrentUsername = User.GetUsername();
      
      //Stąd możemy zwracac odpowiedz HTTP
      var users = await unitOfWork.UserRepository.GetMembersAsync(userParams); //w ten sposob otrzymamy liste user'ow z naszej bazy

      Response.AddPaginationHeader(users); 

      return Ok(users); //Ok - nie dba o to co umiescili. Bo to co umiscili nie jest az tak dobre, przez to ze uzywamy kolekcje "IEnumerable"
   }


   //W tym przypadku chcemy uzysjac indywidualnego uzytkownaka
   //oprocz user'a API, chelibysmy wiedzic id user'a np w URL musi byc cos  takiego
   //[HttpGet("{id:int}")]  //np w URL musi byc cos  takiego - api/user/1 //:int - to jest bezpeczenstwo typu i okreslic ograniczenie - mowi nam ze nasz identyfikator biedzie typu integer.  

   //[HttpGet("{username:string}")] //parametr trasy jest domyslnie stringiem
   [HttpGet("{username}")]
   public async Task<ActionResult<MemberDto>> GetUser(string username)
   {
      var user = await unitOfWork.UserRepository.GetMemberAsync(username);
      if (user == null) return NotFound();
      return user;
   }

   [HttpPut]
   public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
   {

      var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());//Pobieramy naszego usera z Repository

      if (user is null) return BadRequest("Could not found user");

      mapper.Map(memberUpdateDto, user);//Func mapowania bierze naszą memberUpdateDto, która zawiera w siebie rozne wlasciwosci

      if (await unitOfWork.Complete()) return NoContent(); //Sprawdza ile zmian zostawo zapisano w bd to wysyla - NoContent() 204, W przeciwnym przepadku jezeli tak samo, czyli nic nie zmienilismy wysli BadRequest("Failed to update the user");

      return BadRequest("Failed to update the user");
   }

   [HttpPost("add-photo")]
   public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
   {
      var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

      if (user == null) return BadRequest("Cannot update user");

      var result = await photoService.AddPhotoAsync(file);

      if (result.Error != null) return BadRequest(result.Error.Message);

      var photo = new Photo
      {
         Url = result.SecureUrl.AbsoluteUri,
         PublicId = result.PublicId
      };

      if(user.Photos.Count == 0) photo.IsMain = true;

      user.Photos.Add(photo);

      if (await unitOfWork.Complete())
         return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));

      return BadRequest("Problem adding photo");
   }

   [HttpPut("set-main-photo/{photoId:int}")]
   public async Task<ActionResult> SetMainPhoto(int photoId)
   {
      var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

      if (user == null) return BadRequest("Could not found user");

      var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

      if (photo == null || photo.IsMain) return BadRequest("Could not use this as main photo");

      var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
      if (currentMain != null) currentMain.IsMain = false;
      photo.IsMain = true;

      if (await unitOfWork.Complete()) return NoContent();

      return BadRequest("Problem setting main photo");
   }


   [HttpDelete("delete-photo/{photoId:int}")]
   public async Task<ActionResult> DeletePhoto(int photoId)
   {
      var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

      if (user == null) return BadRequest("User not found");

      var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

      if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

      if (photo.PublicId != null)
      {
         var result = await photoService.DeletePhotoAsync(photo.PublicId);
         if (result.Error != null) return BadRequest(result.Error.Message);
      }

      user.Photos.Remove(photo);

      if (await unitOfWork.Complete()) return Ok();

      return BadRequest("Problem deleting photo");
   }
}
