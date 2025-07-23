using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Area.Models;

using Microsoft.EntityFrameworkCore;
using Area.DTOs;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly AreaContext _context;
    private readonly FirebaseService _firebaseService;

    public NotificationsController(AreaContext context, FirebaseService firebaseService)
    {
        _context = context;
        _firebaseService = firebaseService;
    }

    #region Send Notification
    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] DevicesToken model)
    {
        try
        {
            await _firebaseService.SendNotificationAsync(
                deviceToken: model.DeviceToken,
                title: "Hello " + model.UserName,
                body: "New Notifi"
            );
            return Ok(new 
            {
                Message = "Notification sent successfully",
                Message2 = "priority: high"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
    #endregion

    #region Get Notifications
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationsDTO>>> GetAllDeviceTokens()
    {
        var Notifications = await _context.Notifications
           .Select(N => new NotificationsDTO
           {
               Title = N.Title,
               Body = N.Body,
               CreatedAt = N.CreatedAt,
               UserName = N.UserName,
               Sender = N.sender,
               ImageUrl = _context.Persons
                .Where(p => p.UserName == N.sender)
                .Select(p => p.ImagePath)
                .FirstOrDefault()
           }
           ).ToListAsync();
        return Ok(Notifications);

    }
    #endregion

}
