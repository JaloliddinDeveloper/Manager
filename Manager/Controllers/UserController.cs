using Manager.Brokers.Storages;
using Manager.Models.Foundations.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTFulSense.Controllers;

namespace Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : RESTFulController
    {
        private readonly IStorageBroker storageBroker;
        private readonly IWebHostEnvironment webHostEnvironment;

        public UserController(
            IStorageBroker storageBroker,
            IWebHostEnvironment webHostEnvironment)
        {
            this.storageBroker = storageBroker;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async ValueTask<ActionResult<IEnumerable<User>>> GetAllUsersAsync()
        {
            try
            {
                var users = await this.storageBroker.SelectAllUsersAsync();
                return Ok(new { success = true, data = users });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving users." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromForm] User user, IFormFile picture)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.Name))
                    return BadRequest(new { success = false, message = "User name is required." });

                if (user.Age <= 0)
                    return BadRequest(new { success = false, message = "Valid age is required." });

                if (picture != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(picture.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { success = false, message = "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed." });

                    if (picture.Length > 5 * 1024 * 1024)
                        return BadRequest(new { success = false, message = "File size exceeds the 5 MB limit." });

                    var uploadsFolder = Path.Combine(this.webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}_{picture.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await picture.CopyToAsync(stream);
                    }

                    user.UserPicture = $"/images/{fileName}"; // Save path
                }

                await this.storageBroker.InsertUserAsync(user);

                return Ok(new { success = true, message = "User added successfully.", data = user });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding the user." });
            }
        }
    }
}
