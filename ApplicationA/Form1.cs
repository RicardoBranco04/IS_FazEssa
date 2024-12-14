using RestSharp;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Drawing;
using System.Net;
using ApplicationA.Models;
using Application = ApplicationA.Models.Application;
using System.Drawing.Imaging;
namespace ApplicationA
{
    public partial class Form1 : Form
    {
        private readonly string baseURI = @"https://localhost:44316/api/somiod"; // Must be the same as the URI in the Web API project
        MqttClient mClient;
        RestClient client = null;
        private readonly string appName = "Illumination";
        private readonly string containerName = "stadium";
        private readonly string notificationName = "creation";
        private readonly string mosquittoEndpoint = "test.mosquitto.org";
        private readonly string catedralON = "assets/ON.JPG";
        private readonly string catedralOFF = "assets/OFF.JPG";
        private string[] topics = { "stadium" };

        public Form1()
        {
            InitializeComponent();
            client = new RestClient(baseURI);
            mClient = new MqttClient(mosquittoEndpoint);
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            CheckApplication();
            CheckContainer();
            CheckNotification();


            mClient.Connect(Guid.NewGuid().ToString());
            if (!mClient.IsConnected)
            {
                MessageBox.Show("Error connecting to message broker...");
                return;
            }
            mClient.MqttMsgPublishReceived += ClientMqttMsgPublishReceived;
            byte[] qos = { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE };
            mClient.Subscribe(topics, qos);
        }

        private void ClientMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                this.BeginInvoke((MethodInvoker)delegate ()
                {
                    string rawMessage = System.Text.Encoding.UTF8.GetString(e.Message);

                    XmlSerializer serializer = new XmlSerializer(typeof(Record));
                    MemoryStream ms = new MemoryStream(e.Message);

                    Record record = (Record)serializer.Deserialize(ms);
                    ms.Close();

                    float value = float.Parse(record.Content);

                    if (value >= 0 && value < 50)
                    {
                        SetImageWithOpacity(catedralOFF, value / 100);
                    }
                    else if (value >= 50 && value <= 100)
                    {
                        SetImageWithOpacity(catedralON, value / 100);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing the message: " + ex.Message);
            }
        }


        private void CheckApplication()
        {
            RestRequest request = new RestRequest("applications/{name}", Method.Get);
            request.RequestFormat = DataFormat.Xml;
            request.AddUrlSegment("name", appName);
            RestResponse response = client.Execute<Application>(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Application application = new Application
                {
                    Name = appName
                };
                request = new RestRequest("applications", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };
                request.AddObject(application);
                response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    MessageBox.Show("Error adding Application!");
                }
            }
        }

        private void CheckContainer()
        {

            RestRequest request = new RestRequest("applications/{appname}/containers/{containername}", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("appname", appName);
            request.AddUrlSegment("containername", containerName);
            RestResponse response = client.Execute<Container>(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Container container = new Container
                {
                    Name = containerName
                };
                request = new RestRequest("applications/{appname}/containers", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };
                request.AddUrlSegment("appname", appName);
                request.AddObject(container);
                response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    MessageBox.Show("Error adding Container!");
                }
            }
        }

        public void CheckNotification()
        {
            RestRequest request = new RestRequest("applications/{appname}/containers/{containername}/notifications/{name}", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("appname", appName);
            request.AddUrlSegment("containername", containerName);
            request.AddUrlSegment("name", notificationName);
            RestResponse response = client.Execute<Notification>(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Notification notification = new Notification
                {
                    Name = notificationName,
                    Endpoint = mosquittoEndpoint,
                };
                request = new RestRequest("applications/{appname}/containers/{containername}/notifications", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };
                request.AddUrlSegment("appname", appName);
                request.AddUrlSegment("containername", containerName);
                request.AddObject(notification);
                response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    MessageBox.Show("Error adding Notification!");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mClient.IsConnected)
            {
                mClient.Unsubscribe(topics);
                mClient.Disconnect();
            }

        }

        private T DeserializeXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }


        private void SetImageWithOpacity(string imagePath, float opacity)
        {
            using (Image originalImage = Image.FromFile(imagePath))
            {
                Bitmap bitmap = new Bitmap(originalImage.Width, originalImage.Height);

                using (Graphics graph = Graphics.FromImage(bitmap))
                {
                    ColorMatrix colorMatrix = new ColorMatrix
                    {
                        Matrix33 = opacity
                    };
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(colorMatrix);

                        graph.DrawImage(originalImage, new Rectangle(0, 0, bitmap.Width, bitmap.Height),0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, attributes);
                    }
                }

                pictureBox1.Image = bitmap;
            }
        }


    }
}
