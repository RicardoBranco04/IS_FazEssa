using System;
using System.Windows.Forms;
using RestSharp;
using FullApplication.Models;
using Application = FullApplication.Models.Application;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Web;


namespace FullApplication
{
    public partial class Form1 : Form
    {
        private readonly string baseURI = @"https://localhost:44316/api/somiod"; // Has to be the same as in ApplicationA and API
        RestClient client = null;

        public Form1()
        {
            InitializeComponent();
            client = new RestClient(baseURI);
            comboBoxPOSTNotificationEvent.Items.Add("Creation");
            comboBoxPOSTNotificationEvent.Items.Add("Deletion");
        }

        private void buttonGETALLApplication_Click(object sender, EventArgs e)
        {
            RestRequest request = new RestRequest("applications/", Method.Get) {
                RequestFormat = DataFormat.Xml
            };
            if (!string.IsNullOrEmpty(textBoxLocateApplication.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateApplication.Text);
            }
            RestResponse response = client.Execute<Application>(request);
            if (response.Content != "")
            {
                textBoxResponseGETApplication.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETApplication.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private void buttonGETApplication_Click(object sender, EventArgs e)
        {

            if(string.IsNullOrEmpty(textBoxGetApplication.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            RestRequest request = new RestRequest("applications/{name}", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddParameter("name", textBoxGetApplication.Text);
            if (!string.IsNullOrEmpty(textBoxLocateApplication.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateApplication.Text);
            }
            RestResponse response = client.Execute<Application>(request);
            if (response.Content != "")
            {
                textBoxResponseGETApplication.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETApplication.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private void buttonPOSTApplication_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPOSTApplication.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            Application application = new Application
            {
                Name = textBoxPOSTApplication.Text
            };
            RestRequest request = new RestRequest("applications/", Method.Post)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddObject(application);
            textBoxRequestPOSTApplication.Text = SerializeToXml(application);
            RestResponse response = client.Execute<Application>(request);
            textBoxResponsePOSTApplication.Text = response.Content;
        }

        private void buttonPUTApplication_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPUTApplication.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            Application application = new Application
            {
                Name = textBoxNewNamePUTApplication.Text
            };


            RestRequest request = new RestRequest("applications/{appName}", Method.Put)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("appName", textBoxPUTApplication.Text);
            request.AddObject(application);
            textBoxRequestPUTApplication.Text = SerializeToXml(application);
            RestResponse response = client.Execute<Application>(request);
            textBoxResponsePUTApplication.Text = response.Content;
        }

        private void buttonDELETEApplication_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBoxDELETEApplication.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            RestRequest request = new RestRequest("applications/{name}", Method.Delete)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("name", textBoxDELETEApplication.Text);
            RestResponse response = client.Execute<Application>(request);
            textBoxResponseDELETEApplication.Text = response.Content;
        }

        private void buttonGETALLContainer_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBoxGETApplicationContainer.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            RestRequest request = new RestRequest("applications/{applicationName}/containers", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("applicationName", textBoxGETApplicationContainer.Text);
            if (!string.IsNullOrEmpty(textBoxLocateContainer.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateContainer.Text);
            }
            RestResponse response = client.Execute<Container>(request);
            if (response.Content != "")
            {
                textBoxResponseGETContainer.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETContainer.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private void buttonGETContainer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxGETApplicationContainer.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxGetContainer.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            RestRequest request = new RestRequest("applications/{applicationName}/containers/{containerName}", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("applicationName", textBoxGETApplicationContainer.Text);
            request.AddUrlSegment("containerName", textBoxGetContainer.Text);
            if (!string.IsNullOrEmpty(textBoxLocateContainer.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateContainer.Text);
            }
            RestResponse response = client.Execute<Container>(request);
            if (response.Content != "")
            {
                textBoxResponseGETContainer.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETContainer.Text = "ERROR CODE: " + response.StatusCode;
            }

        }

        private void buttonPOSTContainer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPOSTApplicationContainer.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPOSTContainer.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            Container container = new Container
            {
                Name = textBoxPOSTContainer.Text
            };

            RestRequest request = new RestRequest("applications/{appname}/containers", Method.Post)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("appname", textBoxPOSTApplicationContainer.Text);
            request.AddObject(container);
            textBoxRequestPOSTContainer.Text = SerializeToXml(container);
            RestResponse response = client.Execute<Container>(request);
            textBoxResponsePOSTContainer.Text = response.Content;
        }

        private void buttonPUTContainer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPUTApplicationContainer.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPUTContainer.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxNewNamePUTContainer.Text))
            {
                MessageBox.Show("Please enter a new container name");
                return;
            }

            Container container = new Container
            {
                Name = textBoxNewNamePUTContainer.Text
            };

            RestRequest request = new RestRequest("applications/{appname}/containers/{containerName}", Method.Put)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("appname", textBoxPUTApplicationContainer.Text);
            request.AddUrlSegment("containerName", textBoxPUTContainer.Text);
            request.AddObject(container);
            textBoxRequestPOSTContainer.Text = SerializeToXml(container);
            RestResponse response = client.Execute<Container>(request);
            textBoxResponsePUTContainer.Text = response.Content;
        }

        private void buttonDELETEContainer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxDELETEApplicationContainer.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxDELETEContainer.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            RestRequest request = new RestRequest("applications/{name}/containers/{containerName}", Method.Delete)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("name", textBoxDELETEApplicationContainer.Text);
            request.AddUrlSegment("containerName", textBoxDELETEContainer.Text);
            RestResponse response = client.Execute<Container>(request);
            textBoxResponseDELETEContainer.Text = response.Content;
        }

        private void buttonGETALLNotification_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxGETApplicationContainerNotification.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxGETContainerNotification.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }


            RestRequest request = new RestRequest("applications/{applicationName}/containers/{containerName}/notifications", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            if (!string.IsNullOrEmpty(textBoxLocateNotification.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateNotification.Text);
            }
            request.AddUrlSegment("applicationName", textBoxGETApplicationContainerNotification.Text);
            request.AddUrlSegment("containerName", textBoxGETContainerNotification.Text);
            RestResponse response = client.Execute<Notification>(request);
            if (response.Content != "")
            {
                textBoxResponseGETNotification.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETNotification.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private void buttonGETNotification_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxGETApplicationContainerNotification.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxGETContainerNotification.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxGETNotification.Text))
            {
                MessageBox.Show("Please enter an notification name");
                return;
            }


            RestRequest request = new RestRequest("applications/{applicationName}/containers/{containerName}/notifications/{notificationName}", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            if (!string.IsNullOrEmpty(textBoxLocateNotification.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateNotification.Text);
            }
            request.AddUrlSegment("applicationName", textBoxGETApplicationContainerNotification.Text);
            request.AddUrlSegment("containerName", textBoxGETContainerNotification.Text);
            request.AddUrlSegment("notificationName", textBoxGETNotification.Text);
            RestResponse response = client.Execute<Notification>(request);
            if (response.Content != "")
            {
                textBoxResponseGETNotification.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETNotification.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private void buttonPOSTNotification_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPOSTApplicationContainerNotification.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPOSTContainerNotification.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPOSTNotificationName.Text))
            {
                MessageBox.Show("Please enter an notification name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPOSTNotificationEndpoint.Text))
            {
                MessageBox.Show("Please enter an notification endpoint");
                return;
            }

            if(comboBoxPOSTNotificationEvent.Text == "")
            {
                MessageBox.Show("Please select an event");
                return;
            }

            int comboBox = 1;

            if(comboBoxPOSTNotificationEvent.Text == "Deletion")
            {
                comboBox = 2;
            }

            Notification notification = new Notification
            {
                Name = textBoxPOSTNotificationName.Text,
                Endpoint = textBoxPOSTNotificationEndpoint.Text,
                Event = comboBox,
                Enabled = true
            };

            RestRequest request = new RestRequest("applications/{appname}/containers/{containername}/notifications", Method.Post)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("appname", textBoxPOSTApplicationContainerNotification.Text);
            request.AddUrlSegment("containername", textBoxPOSTContainerNotification.Text);
            request.AddObject(notification);
            textBoxRequestPOSTNotification.Text = SerializeToXml(notification);
            RestResponse response = client.Execute<Notification>(request);
            textBoxResponsePOSTNotification.Text = response.Content;

        }

        private void buttonDELETENotification_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxDELETEApplicationContainerNotification.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxDELETEContainerNotification.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxDELETENotification.Text))
            {
                MessageBox.Show("Please enter an notification name");
                return;
            }

            RestRequest request = new RestRequest("applications/{applicationName}/containers/{containerName}/notifications/{notificationName}", Method.Delete)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("applicationName", textBoxDELETEApplicationContainerNotification.Text);
            request.AddUrlSegment("containerName", textBoxDELETEContainerNotification.Text);
            request.AddUrlSegment("notificationName", textBoxDELETENotification.Text);
            RestResponse response = client.Execute<Notification>(request);
            textBoxResponseDELETENotification.Text = response.Content;
        }

        private void buttonGETALLRecord_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxGETApplicationContainerRecord.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxGETContainerRecord.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }


            RestRequest request = new RestRequest("applications/{applicationName}/containers/{containerName}/records", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            if (!string.IsNullOrEmpty(textBoxLocateRecord.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateRecord.Text);
            }
            request.AddUrlSegment("applicationName", textBoxGETApplicationContainerRecord.Text);
            request.AddUrlSegment("containerName", textBoxGETContainerRecord.Text);
            RestResponse response = client.Execute<Record>(request);
            if (response.Content != "")
            {
                textBoxResponseGETRecord.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETRecord.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private void buttonGETRecord_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxGETApplicationContainerRecord.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxGETContainerRecord.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxGETRecord.Text))
            {
                MessageBox.Show("Please enter an record name");
                return;
            }


            RestRequest request = new RestRequest("applications/{applicationName}/containers/{containerName}/records/{recordName}", Method.Get)
            {
                RequestFormat = DataFormat.Xml
            };
            if (!string.IsNullOrEmpty(textBoxLocateRecord.Text))
            {
                request.AddHeader("somiod-locate", textBoxLocateRecord.Text);
            }
            request.AddUrlSegment("applicationName", textBoxGETApplicationContainerRecord.Text);
            request.AddUrlSegment("containerName", textBoxGETContainerRecord.Text);
            request.AddUrlSegment("recordName", textBoxGETRecord.Text);
            RestResponse response = client.Execute<Record>(request);
            if (response.Content != "")
            {
                textBoxResponseGETRecord.Text = TransformString(response.Content);
            }
            else
            {
                textBoxResponseGETRecord.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private void buttonPOSTRecord_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPOSTApplicationContainerRecord.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPOSTContainerRecord.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPOSTRecordName.Text))
            {
                MessageBox.Show("Please enter an record name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPOSTRecordContent.Text))
            {
                MessageBox.Show("Please enter content for the record");
                return;
            }

            Record record = new Record
            {
                Name = textBoxPOSTRecordName.Text,
                Content = textBoxPOSTRecordContent.Text
            };

            RestRequest request = new RestRequest("applications/{appname}/containers/{containername}/records", Method.Post)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("appname", textBoxPOSTApplicationContainerRecord.Text);
            request.AddUrlSegment("containername", textBoxPOSTContainerRecord.Text);
            request.AddObject(record);
            textBoxRequestPOSTRecord.Text = SerializeToXml(record);
            RestResponse response = client.Execute<Record>(request);
            textBoxResponsePOSTRecord.Text = response.Content;
        }

        private void buttonDELETERecord_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxDELETEApplicationContainerRecord.Text))
            {
                MessageBox.Show("Please enter an application name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxDELETEContainerRecord.Text))
            {
                MessageBox.Show("Please enter an container name");
                return;
            }

            if (string.IsNullOrEmpty(textBoxDELETERecord.Text))
            {
                MessageBox.Show("Please enter an record name");
                return;
            }


            RestRequest request = new RestRequest("applications/{applicationName}/containers/{containerName}/records/{recordName}", Method.Delete)
            {
                RequestFormat = DataFormat.Xml
            };
            request.AddUrlSegment("applicationName", textBoxDELETEApplicationContainerRecord.Text);
            request.AddUrlSegment("containerName", textBoxDELETEContainerRecord.Text);
            request.AddUrlSegment("recordName", textBoxDELETERecord.Text);
            RestResponse response = client.Execute<Record>(request);
            if (response.Content != "")
            {
                textBoxResponseDELETERecord.Text = response.Content;
            }
            else
            {
                textBoxResponseDELETERecord.Text = "ERROR CODE: " + response.StatusCode;
            }
        }

        private static string TransformString(string content)
        {
            string transformed = content.Replace(">", ">\r\n").Replace("</", "\r\n</ ");
            return transformed.TrimEnd('\r', '\n');
        }

        private string SerializeToXml<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

    }
}
