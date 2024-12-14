using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SomiodAPI.Models;

namespace SomiodAPI.Helpers
{
    /// <summary>
    /// Provides data access methods for managing Application entities in the database.
    /// </summary>
    public class ApplicationSQLHelper
    {
        // Database connection string from application settings
        private static readonly string ConnectionString = Properties.Settings.Default.connString;

        /// <summary>
        /// Retrieves all applications from the database.
        /// </summary>
        /// <returns>A list of Application objects</returns>
        public static List<Application> GetApplications()
        {
            var applications = new List<Application>();
            const string selectQuery = "SELECT * FROM Application";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(selectQuery, sqlConnection))
                {
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            applications.Add(LoadApplication(reader));
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log the original exception for debugging
                throw new Exception("Database error occurred while retrieving applications.", sqlEx);
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors
                throw new Exception("An unexpected error occurred while retrieving applications.", ex);
            }

            return applications;
        }

        /// <summary>
        /// Retrieves a single application by its name.
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <returns>An Application object or null if not found</returns>
        public static Application GetApplication(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            const string query = "SELECT Id, Name, Creation_date FROM Application WHERE Name = @Name";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? LoadApplication(reader) : null;
                }
            }
        }

        /// <summary>
        /// Gets the highest application ID in the database.
        /// </summary>
        /// <returns>The last (maximum) application ID or 0 if no applications exist</returns>
        public static int GetLastId()
        {
            const string selectQuery = "SELECT ISNULL(MAX(Id), 0) FROM Application";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(selectQuery, sqlConnection))
                {
                    sqlConnection.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while retrieving the last application ID.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the last application ID.", ex);
            }
        }

        /// <summary>
        /// Creates a new application in the database.
        /// </summary>
        /// <param name="application">The application to create</param>
        /// <returns>The created Application object or null if creation failed</returns>
        public static Application CreateApplication(Application application)
        {
            // If no name is provided, generate a default name
            if (string.IsNullOrWhiteSpace(application.Name))
            {
                application.Name = $"app{GetLastId() + 1}";
            }

            const string insertQuery = "INSERT INTO Application (Name, Creation_date) VALUES (@Name, @Creation_date)";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(insertQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", application.Name);
                    cmd.Parameters.AddWithValue("@Creation_date", application.Creation_date);
                    sqlConnection.Open();

                    return cmd.ExecuteNonQuery() > 0 ? GetApplication(application.Name) : null;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while creating the application.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while creating the application.", ex);
            }
        }

        /// <summary>
        /// Updates an existing application's name.
        /// </summary>
        /// <param name="oldName">The current name of the application</param>
        /// <param name="application">The application with the new name</param>
        /// <returns>The updated Application object or null if update failed</returns>
        public static Application UpdateApplication(string oldName, Application application)
        {
            const string updateQuery = "UPDATE Application SET Name = @Name WHERE Name = @OldName";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(updateQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", application.Name);
                    cmd.Parameters.AddWithValue("@OldName", oldName);
                    sqlConnection.Open();

                    return cmd.ExecuteNonQuery() > 0 ? GetApplication(application.Name) : null;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while updating the application.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the application.", ex);
            }
        }

        /// <summary>
        /// Deletes an application from the database.
        /// </summary>
        /// <param name="name">The name of the application to delete</param>
        /// <returns>The deleted Application object or null if deletion failed</returns>
        public static Application DeleteApplication(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            const string deleteQuery = "DELETE FROM Application WHERE Name = @name";

            try
            {
                // First, retrieve the application to be deleted
                var application = GetApplication(name);
                if (application == null)
                    return null;

                using (var connection = new SqlConnection(ConnectionString))
                using (var command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    connection.Open();

                    return command.ExecuteNonQuery() > 0 ? application : null;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while deleting the application.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while deleting the application.", ex);
            }
        }

        /// <summary>
        /// Converts a SqlDataReader row to an Application object.
        /// </summary>
        /// <param name="reader">The SqlDataReader containing application data</param>
        /// <returns>An Application object</returns>
        private static Application LoadApplication(SqlDataReader reader)
        {
            return new Application
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Creation_date = reader.GetDateTime(reader.GetOrdinal("Creation_date")).ToString("yyyy/MM/dd HH:mm:ss"),
            };
        }
    }
}