using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ConsoleApplication1
{

    delegate void SetTextCallback(string text);
    class Program
    {

        static void Main(string[] args)
        {
            String topic;
            MqttClient clientSub;
            try
            {
                IPAddress HostIP;
                HostIP = IPAddress.Parse("85.119.83.194");
                clientSub = new MqttClient(HostIP);
               // clientSub.MqttMsgPublishReceived += new MqttClient.MqttMsgPublishEventHandler(EventPublished);
                clientSub.Connect("MQTTPUBLISHER" + "_sub");
                Console.WriteLine("\n\t* Client connected");
                String mensaje = "";
                topic = "KinectControl";
            Console.WriteLine("\nEscribe el mensaje a publicar");
            mensaje=Console.ReadLine(); 
               // clientSub.p
                clientSub.Publish(topic, Encoding.UTF8.GetBytes(mensaje)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);
                Console.WriteLine("\n*** Publishing on: " + topic);
                Console.WriteLine("\nMensaje enviado");
                Console.ReadKey();
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine("\nERROR DE CASTING ");
            }
        }
        //private void EventPublished(Object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        //{
        //    try
        //    {
        //        Console.WriteLine("\n*** Received Message");
        //        Console.WriteLine("\n*** Topic: " + e.Topic);
        //        Console.WriteLine("\n*** Message: " + System.Text.UTF8Encoding.UTF8.GetString(e.Message));

        //    }
        //    catch (InvalidCastException ex)
        //    {
        //    }
        //}


        }
    }