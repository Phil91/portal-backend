﻿using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Notification.Service.BusinessLogic;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Notification.Service.Controllers;

/// <summary>
/// Controller providing actions for creating, displaying and updating notifications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="NotificationController"/>
    /// </summary>
    /// <param name="logic">The business logic for the notifications</param>
    public NotificationController(INotificationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Creates a new notification for the given user.
    /// </summary>
    /// <param name="companyUserId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the user to create the notification for.</param>
    /// <param name="data">Contains the information needed to create the notification.</param>
    /// <remarks>Example: POST: /api/notification/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="201">Notification was successfully created.</response>
    /// <response code="400">UserId not found or the NotificationType or NotificationStatus don't exist.</response>
    [HttpPost]
    [Route("{companyUserId}")]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(NotificationDetailData), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationDetailData>> CreateNotification([FromRoute] Guid companyUserId, [FromBody] NotificationCreationData data)
    {
        var notificationDetailData = await _logic.CreateNotification(data, companyUserId).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetNotification), new { notificationId = notificationDetailData.Id }, notificationDetailData);
    }

    /// <summary>
    /// Gets all notifications for the logged in user
    /// </summary>
    /// <remarks>Example: Get: /api/notification/</remarks>
    /// <response code="200">Collection of the unread notifications for the user.</response>
    /// <response code="400">UserId not found.</response>
    [HttpGet]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(ICollection<NotificationDetailData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<ICollection<NotificationDetailData>> GetNotifications() =>
        this.WithIamUserId(userId => _logic.GetNotifications(userId));

    /// <summary>
    /// Gets all notifications for the logged in user
    /// </summary>
    /// <param name="notificationId" example="ad5b64ee-98fc-41c4-982a-f32610ad01b8"></param>
    /// <remarks>Example: Get: /api/notification/ad5b64ee-98fc-41c4-982a-f32610ad01b8</remarks>
    /// <response code="200">Collection of the unread notifications for the user.</response>
    /// <response code="400">UserId not found.</response>
    /// <response code="404">Notification does not exist.</response>
    [HttpGet]
    [Route("{notificationId}", Name = nameof(GetNotification))]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(NotificationDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<NotificationDetailData> GetNotification([FromRoute] Guid notificationId) =>
        this.WithIamUserId(userId => _logic.GetNotification(notificationId, userId));
}
