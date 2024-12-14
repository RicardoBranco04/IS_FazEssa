using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationB.Models;
using Application = ApplicationB.Models.Application;
using Container = ApplicationB.Models.Container;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationB
{
    public partial class Form1 : Form
    {
        private readonly string baseURI = @"https://localhost:44316/api/somiod"; // Has to be the same as in ApplicationA and API
        private readonly string appName = "Switch";
        private readonly string stadiumApp = "Illumination";
        private readonly string containerName = "stadium";

        RestClient client = null;
        public Form1()
        {

            InitializeComponent();
            client = new RestClient(baseURI);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckApplication();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            string value = trackBar1.Value.ToString();
            labelValue.Text = value;

        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            string value = trackBar1.Value.ToString();
            PostData(value);
        }


        private void PostData(string state)
        {
            RestRequest request = new RestRequest("applications/{appname}/containers/{containername}/records/last", Method.Get);
            request.AddUrlSegment("appname", stadiumApp);
            request.AddUrlSegment("containername", containerName);
            request.RequestFormat = DataFormat.Xml;
            RestResponse response = client.Execute(request);

            Record record = null;

            if (response.Content != "")
            {
                record = DeserializeXml<Record>(response.Content);
            }

            Record newRecord = new Record
            {
                Name = record == null ? "record_1" : "record_" + (record.Id + 1).ToString(),
                Content = state,
            };

            request = new RestRequest("applications/{appname}/containers/{containername}/records", Method.Post);
            request.RequestFormat = DataFormat.Xml;
            request.AddObject(newRecord);
            request.AddUrlSegment("appname", stadiumApp);
            request.AddUrlSegment("containername", containerName);
            response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                MessageBox.Show("Error adding Data!");
            }
        }

        private void CheckApplication()
        {
            RestRequest request = new RestRequest("applications/{name}", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("name", appName);
            RestResponse response = client.Execute<Application>(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Application application = new Application
                {
                    Name = "Switch"
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

    public T DeserializeXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }


    }
}
