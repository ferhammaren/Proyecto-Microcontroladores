using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace aplicacionComandos
{

    //delegate void SetTextCallback(string text);
    public class Class1
    {

            String topic;
            String ip;
            String mensaje;
            MqttClient clientSub;
            IPAddress HostIP;
     
        public Class1(string topic, string ip){
            this.ip=ip;
            this.topic=topic;
            HostIP = IPAddress.Parse(ip);
        }

        public void sendMessage(){
                   try
            {      
                clientSub = new MqttClient(HostIP);
                clientSub.Connect("MQTTPUBLISHER" + "_sub");
                //topic = "KinectControl";
                clientSub.Publish(topic, Encoding.UTF8.GetBytes(mensaje)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);

            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine("\nERROR DE CASTING "+ex.Message);
            }
        }

        public void setIP(String ip)
        {
            this.ip=ip;
        }

        public void setTopic(String topic)
        {
            this.topic=topic;
        }

        public void setMessage(String mensaje)
        {
            this.mensaje=mensaje;
        }
    }

    }
