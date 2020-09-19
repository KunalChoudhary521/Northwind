using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Northwind.API.Models;
using Northwind.API.Models.Users;
using Northwind.API.Services;
using Northwind.Data.Entities;

namespace Northwind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = nameof(PolicyService.Admin))]
        public async Task<ActionResult<UserModel[]>> GetUsers()
        {
            return _mapper.Map<UserModel[]>(await _userService.GetAll());
        }

        [HttpGet("{userId:int}")]
        [Authorize(Policy = nameof(PolicyService.Admin))]
        public async Task<ActionResult<UserModel>> GetUser(int userId)
        {
            var user = await _userService.GetById(userId);

            if (user == null)
                return NotFound();

            return _mapper.Map<UserModel>(user);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<UserModel>> GetUser(string username)
        {
            var user = await _userService.GetByUserName(username);

            if (user == null)
                return NotFound();

            var claimName = User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            if (!user.UserIdentifier.ToString().Equals(claimName) && !User.IsInRole(nameof(Role.Admin)))
                return Forbid();

            return _mapper.Map<UserModel>(user);
        }

        [HttpPost]
        [Authorize(Policy = nameof(PolicyService.Admin))]
        public async Task<ActionResult<UserModel>> AddUser(UserRequestModel userRequestModel)
        {
            if (await _userService.GetByUserName(userRequestModel.UserName) != null)
                return BadRequest($"user with username {userRequestModel.UserName} already exists");

            if(!Enum.TryParse(userRequestModel.Role, true, out Role _))
                return BadRequest($"The role '{userRequestModel.Role}' is invalid");

            var user = _mapper.Map<User>(userRequestModel);
            _userService.Add(user, userRequestModel.Password);

            if (await _userService.IsSavedToDb())
            {
                var persistedUserModel = _mapper.Map<UserModel>(user);
                return CreatedAtAction(nameof(GetUser),
                                       new { userId = persistedUserModel.UserId },
                                       persistedUserModel);
            }

            return BadRequest();
        }

        [HttpDelete("{userId:int}")]
        [Authorize(Policy = nameof(PolicyService.Admin))]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            var existingUser = await _userService.GetById(userId);
            if (existingUser == null)
                return NotFound();

            _userService.Delete(existingUser);
            if(await _userService.IsSavedToDb())
                return Ok($"User with id '{existingUser.UserId}' has been deleted");

            return BadRequest();
        }
    }
}