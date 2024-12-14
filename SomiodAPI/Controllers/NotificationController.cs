using System;
using System.Collections.Generic;
using System.Web.Http;
using SomiodAPI.Models;
using SomiodAPI.Helpers;
using System.Net;
using System.Linq;

namespace SomiodAPI.Controllers
{
    [RoutePrefix("api/somiod/applications/{applicationName}/containers/{containerName}/notifications")]
    public class NotificationController : ApiController
    {
        #region GET Methods

        /// <summary>
        /// Retrieves all notifications for a specific container or filtered data based on the "somiod-locate" header.
        /// </summary>
        /// <param name="applicationName">Name of the parent application.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>List of notifications or filtered data.</returns>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllNotifications(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                return BadRequest("Container name is required.");
            }

            string somiodLocateHeader = Request.Headers.Contains("somiod-locate")
                ? Request.Headers.GetValues("somiod-locate").FirstOrDefault()
                : null;

            if (string.IsNullOrEmpty(somiodLocateHeader))
            {
                List<Notification> notifications = NotificationSQLHelper.GetNotifications(containerName);

                if (notifications == null || notifications.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, "No notifications found.");
                }
                return Ok(notifications);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "notification":
                    List<Notification> notificationsList = NotificationSQLHelper.GetNotifications(containerName);
                    if (notificationsList == null || notificationsList.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No notifications found.");
                    }
                    return Ok(notificationsList.Select(n => n.Name));

                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        /// <summary>
        /// Retrieves a specific notification or its name based on the "somiod-locate" header.
        /// </summary>
        /// <param name="applicationName">Name of the parent application.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="name">Name of the notification.</param>
        /// <returns>Notification details or its name.</returns>
        [HttpGet]
        [Route("{name}")]
        public IHttpActionResult GetNotification(string name)
        {
            string somiodLocateHeader = Request.Headers.Contains("somiod-locate")
                ? Request.Headers.GetValues("somiod-locate").FirstOrDefault()
                : null;

            if (string.IsNullOrEmpty(somiodLocateHeader))
            {
                Notification notification = NotificationSQLHelper.GetNotification(name);

                if (notification == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Notification '{name}' not found.");
                }
                return Ok(notification);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "notification":
                    Notification notificationLocate = NotificationSQLHelper.GetNotification(name);
                    if (notificationLocate == null)
                    {
                        return Content(HttpStatusCode.NotFound, $"Notification '{name}' not found.");
                    }
                    return Ok(notificationLocate.Name);

                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        #endregion

        #region POST Methods

        /// <summary>
        /// Creates a new notification under a specific container.
        /// </summary>
        /// <param name="applicationName">Name of the parent application.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="value">Notification data to create.</param>
        /// <returns>Created notification or an error response.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult PostNotification(string applicationName, string containerName, [FromBody] Notification value)
        {
            if (value == null)
            {
                return BadRequest("Notification data is required.");
            }

            try
            {
                Notification notificationCreated = NotificationSQLHelper.CreateNotification(value, containerName);

                if (notificationCreated == null)
                {
                    return BadRequest("Notification already exists.");
                }

                return Created($"api/somiod/applications/{applicationName}/containers/{containerName}/notifications/{notificationCreated.Name}", notificationCreated);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region DELETE Methods

        /// <summary>
        /// Deletes a notification by name.
        /// </summary>
        /// <param name="name">Name of the notification to delete.</param>
        /// <returns>Deleted notification or an error response.</returns>
        [HttpDelete]
        [Route("{name}")]
        public IHttpActionResult DeleteNotification(string name)
        {
            try
            {
                Notification notificationDeleted = NotificationSQLHelper.DeleteNotification(name);

                if (notificationDeleted == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Notification '{name}' not found.");
                }
                return Ok(notificationDeleted);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion
    }
}
