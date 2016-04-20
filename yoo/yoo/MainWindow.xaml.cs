using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace yoo
{
    public partial class MainForm : Form
    {
        MqttClient clientSub;
        delegate void SetTextCallback(string text);

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                IPAddress HostIP;
                HostIP = IPAddress.Parse(TextBox1.Text);
                clientSub = new MqttClient(HostIP);
                clientSub.MqttMsgPublishReceived += new MqttClient.MqttMsgPublishEventHandler(EventPublished);
            }
            catch (InvalidCastException ex)
            {
            }
        }

        private void EventPublished(Object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {
                SetText("*** Received Message");
                SetText("*** Topic: " + e.Topic);
                SetText("*** Message: " + System.Text.UTF8Encoding.UTF8.GetString(e.Message));
                SetText("");
            }
            catch (InvalidCastException ex)
            {
            }
        }

        private void SetText(string text)
        {
            if (this.ListBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.ListBox1.Items.Add(text);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                clientSub.Connect(TextBox2.Text + "_sub");
                ListBox1.Items.Add("* Client connected");
                clientSub.Subscribe(new string[] { TextBox3.Text }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                ListBox1.Items.Add("** Subscribing to: " + TextBox3.Text);
            }
            catch (InvalidCastException ex)
            {
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                clientSub.Publish(TextBox3.Text, Encoding.UTF8.GetBytes(TextBox4.Text), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE);
                ListBox1.Items.Add("*** Publishing on: " + TextBox3.Text);
            }
            catch (InvalidCastException ex)
            {
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                clientSub.Disconnect();
                ListBox1.Items.Add("* Client disconnected");
            }
            catch (InvalidCastException ex)
            {
            }
        }
    }
}