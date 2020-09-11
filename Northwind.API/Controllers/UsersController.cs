using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Northwind.API.Models;
using Northwind.API.Models.Auth;
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
        public async Task<ActionResult<UserModel[]>> GetUsers()
        {
            return _mapper.Map<UserModel[]>(await _userService.GetAll());
        }

        [HttpGet("{userId:int}")]
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

            return _mapper.Map<UserModel>(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserModel>> AddUser(AuthRequestModel authRequestModel)
        {
            if (await _userService.GetByUserName(authRequestModel.UserName) != null)
                return BadRequest($"user with username {authRequestModel.UserName} already exists");

            var user = _mapper.Map<User>(authRequestModel);
            _userService.Add(user, authRequestModel.Password);

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