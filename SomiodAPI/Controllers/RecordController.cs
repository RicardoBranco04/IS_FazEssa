using System;
using System.Collections.Generic;
using System.Web.Http;
using SomiodAPI.Models;
using SomiodAPI.Helpers;
using System.Net;
using System.Linq;

namespace SomiodAPI.Controllers
{
    [RoutePrefix("api/somiod/applications/{applicationName}/containers/{containerName}/records")]
    public class RecordController : ApiController
    {
        #region GET Methods

        /// <summary>
        /// Retrieves all records for a specific container based on the "somiod-locate" header.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>List of records or filtered data.</returns>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllRecords(string containerName)
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
                List<Record> records = RecordSQLHelper.GetRecords(containerName);

                if (records == null || records.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, "No records found.");
                }
                return Ok(records);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "record":
                    List<Record> records = RecordSQLHelper.GetRecords(containerName);
                    if (records == null || records.Count == 0)
                    {
                        return Content(HttpStatusCode.NotFound, "No records found.");
                    }
                    return Ok(records.Select(n => n.Name));

                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        /// <summary>
        /// Retrieves a specific record or its name based on the "somiod-locate" header.
        /// </summary>
        /// <param name="name">Name of the record.</param>
        /// <returns>Record details or its name.</returns>
        [HttpGet]
        [Route("{name}")]
        public IHttpActionResult GetRecord(string name)
        {
            string somiodLocateHeader = Request.Headers.Contains("somiod-locate")
                ? Request.Headers.GetValues("somiod-locate").FirstOrDefault()
                : null;

            if (string.IsNullOrEmpty(somiodLocateHeader))
            {
                Record record = RecordSQLHelper.GetRecord(name);

                if (record == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Record '{name}' not found.");
                }
                return Ok(record);
            }

            switch (somiodLocateHeader.ToLower())
            {
                case "record":
                    Record record = RecordSQLHelper.GetRecord(name);
                    if (record == null)
                    {
                        return Content(HttpStatusCode.NotFound, $"Record '{name}' not found.");
                    }
                    return Ok(record.Name);

                default:
                    return BadRequest("Invalid somiod-locate header value.");
            }
        }

        /// <summary>
        /// Retrieves the last record in the DB
        /// </summary>
        /// <returns>Last record details.</returns>
        [HttpGet]
        [Route("last")]
        public IHttpActionResult GetLastRecord()
        {
                Record record = RecordSQLHelper.GetLastRecord();

                if (record == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Record doesnt exist");
                }
                return Ok(record);
        }



        #endregion

        #region POST Methods

        /// <summary>
        /// Creates a new record under a specific container.
        /// </summary>
        /// <param name="applicationName">Name of the parent application.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="value">Record data to create.</param>
        /// <returns>Created record or an error response.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult PostRecord(string applicationName, string containerName, [FromBody] Record value)
        {
            if (value == null)
            {
                return BadRequest("Record data is required.");
            }

            try
            {

                Record record = RecordSQLHelper.CreateRecord(value,containerName);
                Notification notification = NotificationSQLHelper.GetNotificationFromContainer(containerName,1);
                if (notification != null)
                {
                    MosquittoHelper.PublishData(notification.Endpoint, containerName , record);
                }
                return Created($"api/somiod/applications/{applicationName}/containers/{containerName}/records/{record.Name}", record);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region DELETE Methods

        /// <summary>
        /// Deletes a record by name.
        /// </summary>
        /// <param name="applicationName">Name of the parent application.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="name">Name of the record to delete.</param>
        /// <returns>Deleted record or an error response.</returns>
        [HttpDelete]
        [Route("{name}")]
        public IHttpActionResult DeleteRecord(string applicationName, string containerName, string name)
        {
            try
            {
                Record recordDeleted = RecordSQLHelper.DeleteRecord(name);

                if (recordDeleted == null)
                {
                    return Content(HttpStatusCode.NotFound, $"Record '{name}' not found.");
                }
                Notification notification = NotificationSQLHelper.GetNotificationFromContainer(containerName, 2);
                if (notification != null)
                {
                    MosquittoHelper.PublishData(notification.Endpoint, containerName, recordDeleted);
                }
                    return Ok(recordDeleted);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion
    }
}
