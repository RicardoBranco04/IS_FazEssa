using System;
using System.Collections.Generic;
using System.Web.Http;
using SomiodAPI.Models;
using SomiodAPI.Helpers;
using System.Net;
using System.Linq;

namespace SomiodAPI.Controllers
{
    [RoutePrefix("api/somiod/applications/{applicationName}/containers")]
    public class ContainerController : ApiController
    {
        #region GET Methods

        /// <summary>
        /// Retrieves all containers for a specific application or filtered data based on the "somiod-locate" header.
        /// </summary>
        /// <param name="applicationName">Name of the parent application.</param>
        /// <returns>List of containers or filtered data.</returns>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllContainers(string applicationName)
        {
            string somiodLocateHeader = Request.Headers.Contains("somiod-locate")
                ? Request.Headers.GetValues("somiod-locate").FirstOrDefault()
                : null;

            if (string.IsNullOrEmpty(somiodLocateHeader))
            {
                List<Container> containers = ContainerSQLHelper.GetContainersFromParent(applicationName);

                if (containers == null || containers.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, "No containers found.");
                }
                return Ok(containers);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "container":
                    List<Container> containersList = ContainerSQLHelper.GetContainersFromParent(applicationName);
                    if (containersList == null || containersList.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No containers found.");
                    }
                    return Ok(containersList.Select(c => c.Name));

                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        /// <summary>
        /// Retrieves a specific container or related data (e.g., records, notifications) based on the "somiod-locate" header.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>Container details or related data.</returns>
        [HttpGet]
        [Route("{containerName}")]
        public IHttpActionResult GetContainer(string containerName)
        {
            string somiodLocateHeader = Request.Headers.Contains("somiod-locate")
                ? Request.Headers.GetValues("somiod-locate").FirstOrDefault()
                : null;

            if (string.IsNullOrEmpty(somiodLocateHeader))
            {
                Container container = ContainerSQLHelper.GetContainer(containerName);

                if (container == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Container '{containerName}' not found.");
                }
                return Ok(container);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "container":
                    Container containerLocate = ContainerSQLHelper.GetContainer(containerName);
                    if (containerLocate == null)
                    {
                        return Content(HttpStatusCode.NotFound, $"Container '{containerName}' not found.");
                    }
                    return Ok(containerLocate.Name);

                case "record":
                    List<Record> recordsList = RecordSQLHelper.GetRecordsLocateContainer(containerName);
                    if (recordsList == null || recordsList.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No records found.");
                    }
                    return Ok(recordsList.Select(c => c.Name));

                case "notification":
                    List<Notification> notificationsList = NotificationSQLHelper.GetNotificationsLocateContainer(containerName);
                    if (notificationsList == null || notificationsList.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No notifications found.");
                    }
                    return Ok(notificationsList.Select(c => c.Name));

                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        #endregion

        #region POST Methods

        /// <summary>
        /// Creates a new container under a specific application.
        /// </summary>
        /// <param name="applicationName">Name of the parent application.</param>
        /// <param name="value">Container data to create.</param>
        /// <returns>Created container or an error response.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult PostContainer(string applicationName, [FromBody] Container value)
        {
            if (value == null)
            {
                return BadRequest("Container data is required.");
            }

            try
            {
                Container containerCreated = ContainerSQLHelper.CreateContainer(value, applicationName);

                if (containerCreated == null)
                {
                    return Conflict();
                }

                return Created($"api/somiod/applications/{applicationName}/containers/{containerCreated.Name}", containerCreated);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE"))
                {
                    return BadRequest("Name must be unique.");
                }

                return InternalServerError(ex);
            }
        }

        #endregion

        #region PUT Methods

        /// <summary>
        /// Updates an existing container.
        /// </summary>
        /// <param name="containerName">Name of the container to update.</param>
        /// <param name="value">Updated container data.</param>
        /// <returns>Updated container or an error response.</returns>
        [HttpPut]
        [Route("{containerName}")]
        public IHttpActionResult PutContainer(string containerName, [FromBody] Container value)
        {
            if (value == null)
            {
                return BadRequest("Container data is required.");
            }

            try
            {
                Container container = ContainerSQLHelper.UpdateContainer(value, containerName);

                if (container == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Container '{containerName}' not found.");
                }
                return Ok(container);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE"))
                {
                    return BadRequest("Name must be unique.");
                }

                return InternalServerError(ex);
            }
        }

        #endregion

        #region DELETE Methods

        /// <summary>
        /// Deletes a container by name.
        /// </summary>
        /// <param name="containerName">Name of the container to delete.</param>
        /// <returns>Deleted container or an error response.</returns>
        [HttpDelete]
        [Route("{containerName}")]
        public IHttpActionResult DeleteContainer(string containerName)
        {
            try
            {
                Container container = ContainerSQLHelper.DeleteContainer(containerName);

                if (container == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Container '{containerName}' not found.");
                }
                return Ok(container);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion
    }
}
