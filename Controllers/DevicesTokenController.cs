using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesTokenController : ControllerBase
    {
        private readonly AreaContext _dbContext;

        public DevicesTokenController(AreaContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Get Tokens
        [HttpGet]
        public async Task<IActionResult> GetAllDeviceTokens()
        {
            var tokens = await _dbContext.DevicesToken.ToListAsync();
            return Ok(tokens);
        }
        #endregion

        #region Create Token
        [HttpPost]
        public async Task<IActionResult> SaveDeviceToken([FromBody] DevicesTokenDTO model)
        {
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.DeviceToken) || string.IsNullOrEmpty(model.DeviceType))
            {
                return BadRequest("UserName, DeviceToken, and DeviceType are required.");
            }

            // Validate device type
            if (model.DeviceType != "Mobile" && model.DeviceType != "Web")
            {
                return BadRequest("DeviceType must be either 'Mobile' or 'Web'.");
            }

            // Check if this token is already in the database
            var existingToken = await _dbContext.DevicesToken
                .FirstOrDefaultAsync(x => x.DeviceToken == model.DeviceToken);

            if (existingToken != null)
            {
                if (existingToken.UserName != model.UserName)
                {
                    // Token is used by another user, reassign it
                    existingToken.UserName = model.UserName;
                    existingToken.DeviceType = model.DeviceType;
                    await _dbContext.SaveChangesAsync();
                    return Ok(new { message = "Device token reassigned to user." });
                }
                else
                {
                    return BadRequest("This device token is already assigned to the user.");
                }
            }

            // Check if the user already has a token of this device type
            var existingTypeToken = await _dbContext.DevicesToken
                .FirstOrDefaultAsync(x => x.UserName == model.UserName && x.DeviceType == model.DeviceType);

            if (existingTypeToken != null)
            {
                // Replace the old token with the new one
                existingTypeToken.DeviceToken = model.DeviceToken;
            }
            else
            {
                // Add a new device token
                var newDevice = new DevicesToken
                {
                    UserName = model.UserName,
                    DeviceToken = model.DeviceToken,
                    DeviceType = model.DeviceType
                };
                _dbContext.DevicesToken.Add(newDevice);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Device token saved successfully." });
        }
        #endregion

        #region Update Token
        [HttpPut]
        public async Task<IActionResult> UpdateDeviceToken([FromBody] DevicesTokenDTO model)
        {
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.DeviceToken))
            {
                return BadRequest("UserName and DeviceToken are required.");
            }

            var existing = await _dbContext.DevicesToken
                .FirstOrDefaultAsync(x => x.UserName == model.UserName);

            if (existing != null)
            {
                existing.DeviceToken = model.DeviceToken;
                _dbContext.DevicesToken.Update(existing);
            }
            else
            {
                var device = new DevicesToken
                {
                    UserName = model.UserName,
                    DeviceToken = model.DeviceToken
                };
                _dbContext.DevicesToken.Add(device);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Device token updated successfully." });
        }
        #endregion
    }
}
