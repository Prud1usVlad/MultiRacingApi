using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiRacingApi.Controllers.ViewModels;
using MultiRacingApi.Models;
using MultiRacingApi.Services;
using System.Xml.Linq;

namespace MultiRacingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UserController(UsersService usersService)
        {
            _usersService = usersService;
        }

        // GET: api/<UserController>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> Get()
        {
            Console.WriteLine("Get all users");

            try
            {
                return Ok((await _usersService.GetAll()).Select((u) => FromUserToModel(u)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserViewModel>> Get(string id)
        {
            Console.WriteLine("Get user id: " + id);

            try
            {
                return Ok(FromUserToModel(await _usersService.GetById(id)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<UserController>/5
        [HttpGet("name/{name}")]
        public async Task<ActionResult<User>> GetByName(string name)
        {
            Console.WriteLine("Get user: " + name);

            try
            {
                return Ok(FromUserToModel(await _usersService.GetByName(name)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //// PUT api/<UserController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<UserController>/5

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> Delete(string id)
        {
            Console.WriteLine("Delete user: " + id);

            try
            {
                await _usersService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/results")]
        [Authorize]
        public async Task<ActionResult> UpdateResults([FromBody] UpdateResultsModel model)
        {
            Console.WriteLine("Update results: " + model.Id);

            try
            {
                var user = await _usersService.GetById(model.Id);
                user.Results = model.Results
                    .OrderBy(r => r)
                    .Take(10)
                    .ToList();

                await _usersService.Update(user.Id, user);
                return Ok("Results updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private UserViewModel FromUserToModel(User user, bool keepId = true)
        {
            return new UserViewModel()
            {
                Id = keepId ? user.Id : "",
                Name = user.Name,
                Results = user.Results,
            };
        }
    }
}
