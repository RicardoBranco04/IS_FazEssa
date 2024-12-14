using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SomiodAPI.Models;

namespace SomiodAPI.Helpers
{
    /// <summary>
    /// Provides SQL database operations for managing records in the Somiod application.
    /// Handles creation, retrieval, and deletion of records across different containers and applications.
    /// </summary>
    public class RecordSQLHelper
    {
        // Connection string retrieved from application settings
        private readonly static string ConnectionString = Properties.Settings.Default.connString;

        #region POST'S

        /// <summary>
        /// Creates a new record in the database.
        /// </summary>
        /// <param name="record">The record to create</param>
        /// <param name="containerName">Name of the container</param>
        /// <returns>Created record or null if creation fails</returns>
        public static Record CreateRecord(Record record, string containerName)
        {
            if (string.IsNullOrEmpty(record.Name))
            {
                record.Name = $"record{GetLastId() + 1}";
            }

            int parentId = NotificationSQLHelper.GetParent(containerName);
            if (parentId == 0)
            {
                return null;
            }

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "INSERT INTO Record (Name, Content, Creation_date, Parent) " +
                    "VALUES (@Name, @Content, @Creation, @Parent)",
                    sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", record.Name);
                    cmd.Parameters.AddWithValue("@Content", record.Content);
                    cmd.Parameters.AddWithValue("@Creation", record.Creation_date);
                    cmd.Parameters.AddWithValue("@Parent", parentId);

                    sqlConnection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0 ? GetRecord(record.Name) : null;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error creating record.", e);
            }
        }

        #endregion

        #region GET'S

        /// <summary>
        /// Retrieves the maximum existing record ID.
        /// </summary>
        /// <returns>Last record ID or 0 if no records exist</returns>
        public static int GetLastId()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 Id FROM Record ORDER BY Id DESC",
                    sqlConnection))
                {
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read()
                            ? reader.GetInt32(reader.GetOrdinal("Id"))
                            : 0;
                    }
                }
            }
            catch
            {
                // In a production environment, log this exception
                return 0;
            }
        }

  
        /// <summary>
        /// Retrieves a record by its name.
        /// </summary>
        public static Record GetRecord(string name)
        {
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT * FROM Record WHERE Name = @Name",
                    sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? LoadRecord(reader) : null;
                    }
                }
            }
            catch
            {
                // In a production environment, log the exception
                return null;
            }
        }

        /// <summary>
        /// Retrieves all records for a given container name.
        /// </summary>
        public static List<Record> GetRecords(string containerName)
        {
            // Get parent container ID
            int parent = GetParent(containerName);
            if (parent == 0)
            {
                return null;
            }

            var records = new List<Record>();

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT * FROM Record WHERE Parent = @Parent",
                    sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Parent", parent);
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(LoadRecord(reader));
                        }
                    }
                }
            }
            catch
            {
                // In a production environment, log the exception
                return null;
            }

            return records;
        }

        /// <summary>
        /// Gets the parent container ID for a given container name.
        /// </summary>
        public static int GetParent(string containerName)
        {
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT Id FROM Container WHERE Name = @Name",
                    sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", containerName);
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read()
                            ? reader.GetInt32(reader.GetOrdinal("Id"))
                            : 0;
                    }
                }
            }
            catch
            {
                // In a production environment, log the exception
                return 0;
            }
        }

        /// <summary>
        /// Retrieves all records for a given application.
        /// </summary>
        public static List<Record> GetRecordsLocateApplication(string applicationName)
        {
            var records = new List<Record>();

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT * FROM Record WHERE Parent IN (SELECT id FROM container WHERE Parent = @id)",
                    sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@id", ApplicationSQLHelper.GetApplication(applicationName).Id);
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(LoadRecord(reader));
                        }
                    }
                }
                return records;
            }
            catch (Exception e)
            {
                throw new Exception("Error retrieving application records.", e);
            }
        }

        /// <summary>
        /// Retrieves all records for a given container.
        /// </summary>
        public static List<Record> GetRecordsLocateContainer(string containerName)
        {
            var records = new List<Record>();

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT * FROM Record WHERE Parent = @parent",
                    sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@parent", GetParent(containerName));
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(LoadRecord(reader));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error retrieving container records.", e);
            }

            return records;
        }

        /// <summary>
        /// Retrieves the last created record.
        /// </summary>
        public static Record GetLastRecord()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 * FROM Record ORDER BY Id DESC",
                    sqlConnection))
                {
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? LoadRecord(reader) : null;
                    }
                }
            }
            catch
            {
                // In a production environment, log the exception
                return null;
            }
        }
        #endregion

        #region DELETE's

        /// <summary>
        /// Deletes a record by its name.
        /// </summary>
        /// <returns>The deleted record or null if deletion failed</returns>
        public static Record DeleteRecord(string name)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    var record = GetRecord(name);

                    using (var cmd = new SqlCommand(
                        "DELETE FROM Record WHERE Name = @Name",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        return cmd.ExecuteNonQuery() > 0 ? record : null;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error deleting record.", e);
            }
        }


        #endregion

        #region AUXILIARY METHODS

        /// <summary>
        /// Loads a Record object from a SqlDataReader.
        /// </summary>
        private static Record LoadRecord(SqlDataReader reader)
        {
            return new Record
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                Creation_date = reader.GetDateTime(reader.GetOrdinal("Creation_date")).ToString("yyyy-MM-dd HH:mm:ss"),
                Parent = reader.GetInt32(reader.GetOrdinal("Parent"))
            };
        }

        #endregion
    }
}