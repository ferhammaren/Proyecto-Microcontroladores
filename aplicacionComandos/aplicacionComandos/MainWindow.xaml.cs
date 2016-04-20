using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

using System.Windows;

using System.Windows.Controls;

using System.Windows.Data;

using System.Windows.Documents;

using System.Windows.Input;

using System.Windows.Media;

using System.Windows.Media.Imaging;

using System.Windows.Navigation;

using System.Windows.Shapes;
using System.Drawing;

using Microsoft.Kinect;

using Microsoft.Kinect.Toolkit;

using Microsoft.Speech.AudioFormat;

using Microsoft.Speech.Recognition;
using System.Net;
using uPLibrary.Networking.M2Mqtt;

namespace aplicacionComandos
{

    /// <summary>

    /// Interaction logic for MainWindow.xaml

    /// </summary>

    public partial class MainWindow : Window
    {

        KinectSensor _sensor;
        SpeechRecognitionEngine speechengine;
      //  Class1 sendText;
        MqttClient clientSub;
        IPAddress HostIP;
        public MainWindow()
        {

            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

          //  sendText= new Class1(boxTopic.Text, boxIP.Text);
            // KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);

          //  conectaActiva();
          //  sendText.setMessage("HOLA DESDE KINECT");
            //sendText.sendMessage();
            //sendText.setMessage(boxMensaje.Text);
            //sendText.sendMessage();
            HostIP = IPAddress.Parse(boxIP.Text);       
            try
            {      
               // clientSub = new MqttClient(HostIP);
                clientSub = new MqttClient(HostIP);
                clientSub.Connect("MQTTPUBLISHER" + "_sub");
                //topic = "KinectControl";
                while (true)
                {
                    clientSub.Publish(boxTopic.Text, Encoding.UTF8.GetBytes(boxMensaje.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);
                }
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine("\nERROR DE CASTING "+ex.Message);
            }
        
        }

        void conectaActiva()
        {

            //Nos aseguramos que la cuenta de sensores conectados sea de al menos 1

            if (KinectSensor.KinectSensors.Count > 0)
            {

                //Checamos que la variable _sensor sea nula

                if (this._sensor == null)
                {
                    //Asignamos el primer sensor Kinect a nuestra variable
                    this._sensor = KinectSensor.KinectSensors[0];
                    if (this._sensor != null)
                    {
                        try
                        {
                            //Iniciamos el dispositivo Kinect
                            this._sensor.Start();
                            //Esto es opcional pero ayuda a colocar el dispositivo Kinect a un cierto angulo de inclinacion, desde -27 a 27
                            _sensor.ElevationAngle = 3;
                            //Informamos que se ha conectado e inicializado correctamente el dispositivo Kinect
                            //El source nuestro control imagen mandara llamar a la imagen 1.jpg , lo mismo se hace para las demas opciones

                        }
                        catch (Exception ex)
                        {
                            //Si hay algun error lo mandamos en el TextBlock statusK
                        }

                        //Creamos esta variable ri que tratara de encontrar un language pack valido haciendo uso del metodo obtenerLP
                        RecognizerInfo ri = obtenerLP();
                        //Si se encontro el language pack requerido lo asignaremos a nuestra variable speechengine
if (ri != null)
                        {

                            this.speechengine = new SpeechRecognitionEngine(ri.Id);

                            //Creamos esta variable opciones la cual almacenara las opciones de palabras o frases que podran ser reconocidas por el dispositivo

                            var opciones = new Choices();

                            //Comenzamos a agregar las opciones comenzando por el valor de opcion que tratamos reconocer y una llave que identificara a ese
                            //valor

                            //Por ejemplo en esta linea "uno" es el valor de opcion y "UNO" es la llave

                            opciones.Add("encendido", "ENCENDIDO");

                            //En esta linea "unidad" es el valor de opcion y "UNO" es la llave

                            opciones.Add("apagado", "APAGAR");

                            //En esta linea "dos" es el valor de opcion y "DOS" es la llave

                            opciones.Add("adelante", "ADELANTE");

                            //En esta linea "windows ocho" es el valor de opcion y "TRES" es la llave y asi sucesivamente

                            opciones.Add("atrás", "BACK");

                            opciones.Add("subir", "SUBIR");
                            opciones.Add("bajar", "BAJAR");
                            opciones.Add(new SemanticResultValue("prende", "ENCENDIDO"));
                            opciones.Add(new SemanticResultValue("apaga", "APAGADO"));
                            opciones.Add(new SemanticResultValue("sube", "SUBIR"));
                            opciones.Add(new SemanticResultValue("baja", "BAJAR"));

                            //Esta variable creará todo el conjunto de frases y palabras en base a nuestro lenguaje elegido en la variable ri

                            var grammarb = new GrammarBuilder { Culture = ri.Culture };

                            //Agregamos las opciones de palabras y frases a grammarb

                            grammarb.Append(opciones);

                            //Creamos una variable de tipo Grammar utilizando como parametro a grammarb

                            var grammar = new Grammar(grammarb);

                            //Le decimos a nuestra variable speechengine que cargue a grammar

                            this.speechengine.LoadGrammar(grammar);

                            //mandamos llamar al evento SpeechRecognized el cual se ejecutara cada vez que una palabra sea detectada

                            speechengine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(speechengine_SpeechRecognized);

                            //speechengine inicia la entrada de datos de tipo audio

                            speechengine.SetInputToAudioStream(_sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1,
                                32000, 2, null));

                            speechengine.RecognizeAsync(RecognizeMode.Multiple);

                        }

                    }
                    else
                    {

                        // statusK1.Content = "Conectado";
                    }
                }

            }

        }

        private RecognizerInfo obtenerLP()
        {

            //Comienza a checar todos los languagepack que tengamos instalados

            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {

                string value;

                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);

                //Aqui es donde elegimos el lenguaje, si se dan cuenta hay una parte donde dice "es-MX" para cambiar el lenguaje a ingles de EU basta con
                //cambiar el valor a "en-US"

                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "es-MX".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {

                    //Si se encontro el language pack solicitado se retorna a recognizer

                    return recognizer;

                }

            }

            //En caso de que no se encuentre ningun languaje pack se retorna un valor nulo

            return null;

        }

        void speechengine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double igualdad = 0.1;

            if (e.Result.Confidence > igualdad)
            {

                switch (e.Result.Words[0].Text)
                {

                    //En caso de que digamos "uno" la llave "UNO" se abrira y se realizara lo siguiente

                    case "ENCENDIDO":

                        //Se mandara un mensaje alusivo a la imagen
                        
                        boxMensaje.Text = "encendido";
                   

                        break;

                    case "APAGADO":
                        boxMensaje.Text = "apagado";
                     //   sendText.setMessage(boxMensaje.Text);
                       // sendText.sendMessage();
                        break;

                    //default:

                    //En caso de que no solo contenga una palabra tambien realizaremos un switch para ver si la frase corresponde a alguna de nuestros
                    //valores de opcion

                    //switch (e.Result.Semantics.Value.ToString())

                    //{

                    case "ADELANTE":
                        boxMensaje.Text = "adelante";
                        System.Diagnostics.Debug.WriteLine("YOOOOO");
                      //  sendText.setMessage(boxMensaje.Text);
                        //sendText.sendMessage();
                        break;
                    case "BACK":
                        boxMensaje.Text = "atras";
                      //  sendText.setMessage(boxMensaje.Text);
                        //sendText.sendMessage();
                        break;
                    case "SUBE":
                        boxMensaje.Text = "subeVolumen";
                      //  sendText.setMessage(boxMensaje.Text);
                        //sendText.sendMessage();
                        break;

                    case "BAJA":
                        boxMensaje.Text = "bajaVolumen";
//sendText.setMessage(boxMensaje.Text);
  //                      sendText.sendMessage();
                        break;
                    default:

                        boxMensaje.Text = "No se reconoció el comando";

                        break;

                    //}

                    //break;

                }

            }

        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (this._sensor != null)
            {

                this._sensor.AudioSource.Stop();

                this._sensor.Stop();

                this._sensor = null;

            }

        }

    }
}