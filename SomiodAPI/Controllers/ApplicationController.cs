using System;
using System.Collections.Generic;
using System.Web.Http;
using SomiodAPI.Models;
using SomiodAPI.Helpers;
using System.Net;
using System.Linq;

namespace SomiodAPI.Controllers
{
    [RoutePrefix("api/somiod/applications")]
    public class ApplicationController : ApiController
    {
        #region GET Methods

        /// <summary>
        /// Retrieves all applications or specific application details based on the "somiod-locate" header.
        /// </summary>
        /// <returns>List of applications or filtered application data.</returns>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllApplications()
        {
            string somiodLocateHeader = Request.Headers.Contains("somiod-locate") 
                ? Request.Headers.GetValues("somiod-locate").FirstOrDefault() 
                : null;

            if (string.IsNullOrEmpty(somiodLocateHeader))
            {
                List<Application> applications = ApplicationSQLHelper.GetApplications();

                if (applications == null || applications.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, "No applications found.");
                }

                return Ok(applications);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "application":
                    List<Application> applications = ApplicationSQLHelper.GetApplications();
                    if (applications == null || applications.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No applications found.");
                    }
                    return Ok(applications.Select(a => a.Name));
                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        /// <summary>
        /// Retrieves a specific application or related data (e.g., containers, records, notifications) based on the "somiod-locate" header.
        /// </summary>
        /// <param name="name">Name of the application.</param>
        /// <returns>Application details or related data.</returns>
        [HttpGet]
        [Route("{name}")]
        public IHttpActionResult GetApplication(string name)
        {
            string somiodLocateHeader = Request.Headers.Contains("somiod-locate") 
                ? Request.Headers.GetValues("somiod-locate").FirstOrDefault() 
                : null;

            if (string.IsNullOrEmpty(somiodLocateHeader))
            {
                Application application = ApplicationSQLHelper.GetApplication(name);
                if (application == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Application '{name}' not found.");
                }
                return Ok(application);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "application":
                    Application application = ApplicationSQLHelper.GetApplication(name);
                    if (application == null)
                    {
                        return Content(HttpStatusCode.NotFound, "Application not found.");
                    }
                    return Ok(application.Name);

                case "container":
                    List<Container> containers = ContainerSQLHelper.GetContainersLocateApplication(name);
                    if (containers == null || containers.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No containers found.");
                    }
                    return Ok(containers.Select(c => c.Name));

                case "record":
                    List<Record> records = RecordSQLHelper.GetRecordsLocateApplication(name);
                    if (records == null || records.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No records found.");
                    }
                    return Ok(records.Select(r => r.Name));

                case "notification":
                    List<Notification> notifications = NotificationSQLHelper.GetNotificationsLocateApplication(name);
                    if (notifications == null || notifications.Count == 0)
                    {
                        return NotFound();
                    }
                    return Ok(notifications.Select(n => n.Name));

                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        #endregion

        #region POST Methods

        /// <summary>
        /// Creates a new application.
        /// </summary>
        /// <param name="application">Application data to be created.</param>
        /// <returns>Created application or an error response.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult PostApplication([FromBody] Application application)
        {
            if (application == null || string.IsNullOrEmpty(application.Name))
            {
                return BadRequest("Application data is invalid.");
            }

            try
            {
                Application createdApplication = ApplicationSQLHelper.CreateApplication(application);
                if (createdApplication == null)
                {
                    return Conflict();
                }

                return Created(Request.RequestUri + "/" + createdApplication.Name, createdApplication);
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
        /// Updates an existing application.
        /// </summary>
        /// <param name="name">Name of the application to update.</param>
        /// <param name="application">Updated application data.</param>
        /// <returns>Updated application or an error response.</returns>
        [HttpPut]
        [Route("{name}")]
        public IHttpActionResult PutApplication(string name, [FromBody] Application application)
        {
            if (application == null)
            {
                return BadRequest("Application data is invalid.");
            }

            try
            {
                Application updatedApplication = ApplicationSQLHelper.UpdateApplication(name, application);

                if (updatedApplication == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Application '{name}' not found.");
                }

                return Ok(updatedApplication);
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
        /// Deletes an application by name.
        /// </summary>
        /// <param name="name">Name of the application to delete.</param>
        /// <returns>Deleted application or an error response.</returns>
        [HttpDelete]
        [Route("{name}")]
        public IHttpActionResult DeleteApplication(string name)
        {
            try
            {
                Application deletedApplication = ApplicationSQLHelper.DeleteApplication(name);
            
                if (deletedApplication == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Application '{name}' not found.");
                }

                return Ok(deletedApplication);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion
    }
}
