using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SomiodAPI.Helpers;
using SomiodAPI.Models;

namespace SomiodAPI.Helpers
{
    public class NotificationSQLHelper
    {

        readonly static string connectionString = Properties.Settings.Default.connString;

        #region GET


        /// <summary>
        /// Retrieves all notifications.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns> The found notifications or null if they don't exist </returns>

        public static List<Notification> GetNotifications(string containerName)
        {
            List<Notification> notifications = new List<Notification>();
            int parent = GetParent(containerName);
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notification WHERE Parent = @Parent", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Parent", parent);
                    sqlConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Notification notification = LoadNotification(reader);
                            notifications.Add(notification);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return notifications;
        }

        /// <summary>
        /// Retrieves a notification by its name.
        /// </summary>
        /// <param name="name">The name of the notification.</param>
        /// <returns>The found notification or null if it does not exist.</returns>
        public static Notification GetNotification(string name)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notification WHERE Name = @Name", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    sqlConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return LoadNotification(reader);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Retrieves notifications located in a specific container by its name.
        /// </summary>
        /// <param name="name">The name of the container.</param>
        /// <returns>A list of notifications found or null if none exist.</returns>
        public static List<Notification> GetNotificationsLocateContainer(string name)
        {
            List<Notification> notifications = new List<Notification>();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notification WHERE Parent = @parent", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@parent", ContainerSQLHelper.GetContainer(name).Id);
                    sqlConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notifications.Add(LoadNotification(reader));
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return notifications;
        }

        /// <summary>
        /// Retrieves notifications located in a specific application by its name.
        /// </summary>
        /// <param name="appName">The name of the application.</param>
        /// <returns>A list of notifications found or null if none exist.</returns>
        public static List<Notification> GetNotificationsLocateApplication(string appName)
        {
            List<Notification> notifications = new List<Notification>();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notification WHERE Parent IN (SELECT id FROM Container WHERE Parent = @id)", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@id", ApplicationSQLHelper.GetApplication(appName).Id);
                    sqlConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notifications.Add(LoadNotification(reader));
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return notifications;
        }

        /// <summary>
        /// Retrieves a notification from a container based on application name, container name, and event type.
        /// </summary>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="type">The event type.</param>
        /// <returns>The found notification or null if it does not exist.</returns>
        public static Notification GetNotificationFromContainer(string containerName, int type)
        {
            int parentId = GetParent(containerName);

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notification WHERE Parent = @parent AND Event = @type AND Enabled = 1", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@parent", parentId);
                    cmd.Parameters.AddWithValue("@type", type);
                    sqlConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return LoadNotification(reader);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        #endregion

        #region POST

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="notification">The notification to create.</param>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="moduleName">The name of the module.</param>
        /// <returns>The created notification or null if creation failed.</returns>
        public static Notification CreateNotification(Notification notification, string moduleName)
        {
            if (notification.Name.Equals(null))
            {
                notification.Name = "notification" + (GetLastId() + 1);
            }

            int parentId = GetParent(moduleName);

            if (parentId == 0 || (notification.Event != 1 && notification.Event != 2))
            {
                return null;
            }

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notification WHERE Parent = @Parent AND Event = @Event", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Parent", parentId);
                    cmd.Parameters.AddWithValue("@Event", notification.Event);

                    sqlConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return LoadNotification(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while creating the notification.", e);
            }

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Notification (Name, Creation_date, Parent, Event, EndPoint, Enabled) VALUES (@Name, @Creation, @Parent, @Event, @Endpoint, @Enabled)", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", notification.Name);
                    cmd.Parameters.AddWithValue("@Creation", notification.Creation_date);
                    cmd.Parameters.AddWithValue("@Parent", parentId);
                    cmd.Parameters.AddWithValue("@Event", notification.Event);
                    cmd.Parameters.AddWithValue("@Endpoint", notification.Endpoint);
                    cmd.Parameters.AddWithValue("@Enabled", notification.Enabled);

                    sqlConnection.Open();
                    int numRows = cmd.ExecuteNonQuery();

                    if (numRows > 0)
                    {
                        return GetNotification(notification.Name);
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while creating the notification.", e);
            }
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Deletes a notification by its name.
        /// </summary>
        /// <param name="name">The name of the notification to delete.</param>
        /// <returns>The deleted notification or null if it does not exist.</returns>
        public static Notification DeleteNotification(string name)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    Notification notification = GetNotification(name);

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Notification WHERE Name = @Name", sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        int numRows = cmd.ExecuteNonQuery();

                        if (numRows > 0)
                        {
                            return notification;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while deleting the notification.", e);
            }

            return null;
        }

        #endregion

        #region Helper Methods

        private static Notification LoadNotification(SqlDataReader reader)
        {
            Notification notification = new Notification
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Creation_date = reader.GetDateTime(reader.GetOrdinal("Creation_date")).ToString("yyyy-MM-dd HH:mm:ss"),
                Parent = reader.GetInt32(reader.GetOrdinal("Parent")),
                Event = reader.GetInt32(reader.GetOrdinal("Event")),
                Endpoint = reader.GetString(reader.GetOrdinal("EndPoint")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("Enabled")),
            };

            return notification;
        }

        public static int GetLastId()
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT MAX(Id) FROM Notification", sqlConnection))
                {
                    sqlConnection.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch
            {
                return 0;
            }
        }

        public static int GetParent(string containerName)
        {
            int parentId = 0;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT Id FROM Container WHERE Name = @Name", sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@Name", containerName);
                    sqlConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            parentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        }
                    }
                }
            }
            catch
            {
                return 0;
            }

            return parentId;
        }

        #endregion
    }
}
