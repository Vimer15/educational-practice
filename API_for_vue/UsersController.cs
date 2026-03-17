using APIDemkaForVue.DTO;
using APIDemkaForVue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIDemkaForVue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpPost("Authorization")]
        public ActionResult Auth(DTO_auth authorization_user) { 
            var user = ShoeContext.Context.UserImports.FirstOrDefault(u => u.LoginUserImport == authorization_user.LoginUserImport &&
            u.PasswordUserImport == authorization_user.PasswordUserImport);
            if (user == null) return BadRequest("Вы ввели не коректные данные");
            return Ok(user);
        }
        [HttpGet("GetAllUsers")]
        public ActionResult GetUsers() { 
            var users = ShoeContext.Context.UserImports.ToList();
            return Ok(users);
        }
    }
}
