using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using uPLibrary.Networking.M2Mqtt;
using SomiodAPI.Models;
namespace SomiodAPI.Helpers
{
    /// <summary>
    /// Helper class for interacting with the Mosquitto MQTT broker.
    /// </summary>
    public class MosquittoHelper
    {
        /// <summary>
        /// Publishes a record to the specified MQTT broker and channel.
        /// </summary>
        /// <param name="endpoint">The MQTT broker endpoint (URL).</param>
        /// <param name="channelName">The name of the MQTT channel (topic) to publish to.</param>
        /// <param name="record">The record object to be published.</param>
        /// <returns>0 if successful, throws an exception otherwise.</returns>
        public static int PublishData(string endpoint, string channelName, Record record)
        {
            MqttClient mClient;
            try
            {
                mClient = new MqttClient(endpoint);
                mClient.Connect(Guid.NewGuid().ToString());
                if (!mClient.IsConnected)
                {
                    throw new Exception("Error connecting to message broker...");
                }
                Byte[] data = Encoding.UTF8.GetBytes(SerializeObjectToXML(record));
                mClient.Publish(channelName, data);
                return 0;
            }
            catch (Exception)
            {
                throw new Exception("Error connecting to message broker...");
            }
        }
        private static string SerializeObjectToXML(object obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                MemoryStream ms = new MemoryStream();
                serializer.Serialize(ms, obj);
                ms.Position = 0;
                using (StreamReader r = new StreamReader(ms))
                {
                    return r.ReadToEnd();
                };
            }
            catch (Exception)
            {
                throw new Exception("Error serializing object to XML...");
            }
        }
    }
}