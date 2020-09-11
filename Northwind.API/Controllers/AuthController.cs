using System;
using System.Threading.Tasks;
using AutoMapper;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Northwind.API.Models.Auth;
using Northwind.API.Services;

namespace Northwind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("access")]
        public async Task<ActionResult<AuthResponseModel>> CreateCredentials(AuthRequestModel authRequestModel)
        {
            var user = await _authService.GetByCredentials(authRequestModel.UserName, authRequestModel.Password);

            if (user == null)
                throw new ProblemDetailsException(StatusCodes.Status401Unauthorized,
                                                  "Invalid user credentials. User not found");

            var authenticatedUser = _authService.CreateCredentials(user);
            if (await _authService.IsSavedToDb())
                return _mapper.Map<AuthResponseModel>(authenticatedUser);

            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseModel>> RefreshCredentials(RefreshTokenRequest refreshTokenRequest)
        {
            var user = await _authService.GetByRefreshToken(refreshTokenRequest.RefreshToken);

            if (user == null)
                throw new ProblemDetailsException(StatusCodes.Status401Unauthorized, "Invalid refresh token");

            if(user.RefreshToken.ExpiryDate < DateTimeOffset.UtcNow)
                throw new ProblemDetailsException(StatusCodes.Status401Unauthorized,
                                                  "Refresh token has expired. Regenerate tokens");

            var refreshedUser = _authService.RefreshCredentials(user);
            if (await _authService.IsSavedToDb())
                return _mapper.Map<AuthResponseModel>(refreshedUser);

            return BadRequest();
        }

        [HttpPost("revoke")]
        public async Task<ActionResult> RevokeCredentials(RefreshTokenRequest refreshTokenRequest)
        {
            var refreshToken = refreshTokenRequest.RefreshToken;
            var user = await _authService.GetByRefreshToken(refreshToken);

            if (user == null)
                throw new ProblemDetailsException(StatusCodes.Status401Unauthorized, "Invalid refresh token");

            _authService.RevokeCredentials(user);
            if (await _authService.IsSavedToDb())
                return Ok($"Access for user with refresh token '{refreshToken}' has been revoked");

            return BadRequest();
        }
    }
}