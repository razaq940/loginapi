using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using DAL.Models;
using BLL.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DAL.Models.Dto.User;

namespace LoginApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        public readonly UserService _user;

        public UserController(IMapper mapper,UserService user)
        {
            _user = user;
            _mapper = mapper;
        }

        [HttpPost("Login")]
        public ActionResult Login(UserLoginDto data)
        {
            try
            {
                var result = _user.Login(data);
                if (result.Item1 == -1)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Header = Response.Headers,
                        Error = true,
                        Message = result.Item4
                    });
                }
                else if (result.Item1 == 2)
                {
                    return Ok(new
                    {
                        StatusCode = 200,
                        Header = Response.Headers,
                        UserId = result.Item2,
                        Email = result.Item3,
                        Message = result.Item4
                    });
                }
                return Ok(new
                {
                    StatusCode = 200,
                    Header = Response.Headers,
                    UserId = result.Item2,
                    Token = result.Item3,
                    Error = false,
                    Message = result.Item4
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Headers = Response.Headers,
                    Error = true,
                    Message = Ex.Message
                });
            }
        }


        [HttpPut("ActivationUser")]
        public ActionResult ActivationUser(ActivationUserDto data)
        {
            try
            {
                var result = _user.ActivationUser(data);
                if (result.Item1 == -1)
                {
                    return BadRequest(new
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Headers = Response.Headers,
                        Error = true,
                        Message = result.Item2
                    });
                }
                else
                {
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Headers = Response.Headers,
                        Error = false,
                        UserId = result.Item2,
                        Token = result.Item3,
                        Message = result.Item4
                    });
                }
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Headers = Response.Headers,
                    Error = true,
                    Message = Ex.Message
                });
            }
        }

        [HttpPost("[action]")]
        public ActionResult RegisterNewUser(UserRegisterDto data)
        {
            try
            {
                var user = _mapper.Map<User>(data);
                var result = _user.RegisterNewUser(user);
                if (result.Item1 == -1)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Headers = Response.Headers,
                        Error = true,
                        Message = result.Item2
                    });
                }
                return Ok(new
                {
                    StatusCode = 200,
                    Headers = Response.Headers,
                    Error = false,
                    Message = result.Item2
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    StatusCode = 500,
                    Headers = Response.Headers,
                    Error = true,
                    Message = Ex.Message
                });
            }
        }

    }
}
