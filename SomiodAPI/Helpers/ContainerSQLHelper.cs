using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SomiodAPI.Models;


namespace SomiodAPI.Helpers
{
    /// <summary>
    /// Provides data access methods for managing Container entities in the database.
    /// </summary>
    public class ContainerSQLHelper
    {
        // Database connection string from application settings
        private static readonly string ConnectionString = Properties.Settings.Default.connString;

        /// <summary>
        /// Creates a new container in the database.
        /// </summary>
        /// <param name="container">The container to create</param>
        /// <param name="applicationName">The name of the parent application</param>
        /// <returns>The created Container object or null if creation failed</returns>
        public static Container CreateContainer(Container container, string applicationName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(applicationName))
                return null;

            // Generate default name if not provided
            if (string.IsNullOrWhiteSpace(container.Name))
            {
                container.Name = $"container{GetLastId() + 1}";
            }

            // Get parent application ID
            int parentId = GetContainerParent(applicationName);
            if (parentId == 0)
                return null;

            const string insertQuery = "INSERT INTO Container (Name, Creation_date, Parent) VALUES (@Name, @Creation, @Parent)";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(insertQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", container.Name);
                    cmd.Parameters.AddWithValue("@Creation", container.Creation_date);
                    cmd.Parameters.AddWithValue("@Parent", parentId);

                    sqlConnection.Open();
                    return cmd.ExecuteNonQuery() > 0 ? GetContainer(container.Name) : null;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while creating the container.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while creating the container.", ex);
            }
        }

        /// <summary>
        /// Gets the highest container ID in the database.
        /// </summary>
        /// <returns>The last (maximum) container ID or 0 if no containers exist</returns>
        public static int GetLastId()
        {
            const string selectQuery = "SELECT ISNULL(MAX(Id), 0) FROM Container";

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
                throw new Exception("Database error occurred while retrieving the last container ID.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the last container ID.", ex);
            }
        }

        /// <summary>
        /// Retrieves the parent application ID for a given application name.
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
        /// <returns>The application ID or 0 if not found</returns>
        public static int GetContainerParent(string applicationName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
                return 0;

            const string selectQuery = "SELECT ISNULL(Id, 0) FROM Application WHERE Name = @ApplicationName";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(selectQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@ApplicationName", applicationName);
                    sqlConnection.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while retrieving the parent application.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the parent application.", ex);
            }
        }

        /// <summary>
        /// Retrieves a container by its ID.
        /// </summary>
        /// <param name="id">The container ID</param>
        /// <returns>A Container object or null if not found</returns>
        public static Container GetContainer(int id)
        {
            const string selectQuery = "SELECT * FROM Container WHERE Id = @Id";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(selectQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? LoadContainer(reader) : null;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while retrieving the container.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the container.", ex);
            }
        }

        /// <summary>
        /// Retrieves a container by its name.
        /// </summary>
        /// <param name="containerName">The name of the container</param>
        /// <returns>A Container object or null if not found</returns>
        public static Container GetContainer(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                return null;

            const string selectQuery = "SELECT * FROM Container WHERE Name = @Name";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(selectQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", containerName);
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? LoadContainer(reader) : null;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while retrieving the container.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the container.", ex);
            }
        }

        /// <summary>
        /// Retrieves all containers from the database.
        /// </summary>
        /// <returns>A list of Container objects</returns>
        public static List<Container> GetContainers()
        {
            var containers = new List<Container>();
            const string selectQuery = "SELECT * FROM Container";

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
                            containers.Add(LoadContainer(reader));
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while retrieving containers.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving containers.", ex);
            }

            return containers;
        }

        /// <summary>
        /// Retrieves containers belonging to a specific parent application.
        /// </summary>
        /// <param name="name">The name of the parent application</param>
        /// <returns>A list of Container objects or null if parent not found</returns>
        public static List<Container> GetContainersFromParent(string name)
        {
            int parent = GetContainerParent(name);
            if (parent == 0)
                return null;

            var containers = new List<Container>();
            const string selectQuery = "SELECT * FROM Container WHERE Parent = @parent";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(selectQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@parent", parent);
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            containers.Add(LoadContainer(reader));
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while retrieving containers.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving containers.", ex);
            }

            return containers;
        }

        /// <summary>
        /// Updates a container's name.
        /// </summary>
        /// <param name="container">The container with the new name</param>
        /// <param name="oldName">The current name of the container</param>
        /// <returns>The updated Container object or null if update failed</returns>
        public static Container UpdateContainer(Container container, string oldName)
        {
            if (container == null || string.IsNullOrWhiteSpace(oldName))
                return null;

            const string updateQuery = "UPDATE Container SET Name = @Name WHERE Name = @OldName";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", container.Name);
                    cmd.Parameters.AddWithValue("@OldName", oldName);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0 ? GetContainer(container.Name) : null;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while updating the container.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the container.", ex);
            }
        }

        /// <summary>
        /// Deletes a container by its name.
        /// </summary>
        /// <param name="name">The name of the container to delete</param>
        /// <returns>The deleted Container object or null if deletion failed</returns>
        public static Container DeleteContainer(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            const string deleteQuery = "DELETE FROM Container WHERE Name = @Name";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    var container = GetContainer(name);
                    if (container == null)
                        return null;

                    using (var cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        return cmd.ExecuteNonQuery() > 0 ? container : null;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while deleting the container.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while deleting the container.", ex);
            }
        }

        /// <summary>
        /// Retrieves containers located in a specific application.
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <returns>A list of Container objects</returns>
        public static List<Container> GetContainersLocateApplication(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<Container>();

            var containers = new List<Container>();
            const string selectQuery = "SELECT * FROM Container WHERE Parent = @id";

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(selectQuery, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@id", GetContainerParent(name));
                    sqlConnection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            containers.Add(LoadContainer(reader));
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while retrieving containers.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving containers.", ex);
            }

            return containers;
        }

        /// <summary>
        /// Converts a SqlDataReader row to a Container object.
        /// </summary>
        /// <param name="reader">The SqlDataReader containing container data</param>
        /// <returns>A Container object</returns>
        private static Container LoadContainer(SqlDataReader reader)
        {
            return new Container
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Creation_date = reader.GetDateTime(reader.GetOrdinal("Creation_date")).ToString("yyyy/MM/dd HH:mm:ss"),
                Parent = reader.GetInt32(reader.GetOrdinal("Parent"))
            };
        }
    }
}