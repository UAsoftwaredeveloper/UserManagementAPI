using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using System.Collections.Concurrent;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static readonly ConcurrentDictionary<int, User> users = new();
        private static int nextId = 1;

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(users.Values.ToList().AsReadOnly());
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            return users.TryGetValue(id, out var user) ? Ok(user) : NotFound();
        }

        [HttpPost]
        public IActionResult Create([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = System.Threading.Interlocked.Increment(ref nextId);
            user.Id = id;
            users.TryAdd(id, user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!users.ContainsKey(id))
                return NotFound();

            updatedUser.Id = id;
            users[id] = updatedUser;
            return Ok(updatedUser);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            return users.TryRemove(id, out _) ? NoContent() : NotFound();
        }
    }
}
