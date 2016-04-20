using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using uPLibrary.Networking.M2Mqtt;

namespace sendMessages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KinectSensorChooser sensorChooser;
        KinectSensor _sensor;
        SpeechRecognitionEngine speechengine;
        MqttClient clientSub;
        IPAddress HostIP;
        public MainWindow()
        {
            InitializeComponent();
            HostIP = IPAddress.Parse("85.119.83.194");
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
            try
            {
                clientSub = new MqttClient(HostIP);
              //  clientSub = new MqttClient(brokerBox.Text);
                clientSub.Connect("MQTTPUBLISHER" + "_sub");
                //topic = "KinectControl";
                
               // do
                //{
                   // System.Diagnostics.Debug.WriteLine("FJQOFWQOOJFQ");
                   
                    //clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);
                 //   conectaActiva();
                    //messageBox.Text = " ";
                   // System.Threading.Thread.Sleep(60);
               // } while (!messageBox.Text.Equals("apagado"));
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine("\nERROR DE CASTING " + ex.Message);
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
                           // opciones.Add("prende", "ENCENDIDO");


                            opciones.Add("prende", "ENCENDIDO");
                            opciones.Add("apagado", "APAGAR");
                            opciones.Add("apaga", "APAGAR");
                            opciones.Add("adelante", "ADELANTE");
                            opciones.Add("atras", "BACK");
                            opciones.Add("sube", "SUBIR");
                            opciones.Add("baja", "BAJAR");
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
            clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);
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

                        messageBox.Text = "encendido";
                        clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);

                        break;

                    case "APAGADO":
                        messageBox.Text = "apagado";
                        //   sendText.setMessage(boxMensaje.Text);
                        // sendText.sendMessage();
                        clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);

                        break;

                    //default:

                    //En caso de que no solo contenga una palabra tambien realizaremos un switch para ver si la frase corresponde a alguna de nuestros
                    //valores de opcion

                    //switch (e.Result.Semantics.Value.ToString())

                    //{

                    case "ADELANTE":
                        messageBox.Text = "adelante";
                        clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);
                        System.Diagnostics.Debug.WriteLine("YOOOOO");
                        //  sendText.setMessage(boxMensaje.Text);
                        //sendText.sendMessage();
                        break;
                    case "BACK":
                        messageBox.Text = "atras";
                        clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);

                        //  sendText.setMessage(boxMensaje.Text);
                        //sendText.sendMessage();
                        break;
                    case "SUBE":
                        messageBox.Text = "subeVolumen";
                        clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);

                        //  sendText.setMessage(boxMensaje.Text);
                        //sendText.sendMessage();
                        break;

                    case "BAJA":
                        messageBox.Text = "bajaVolumen";
                        clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);

                        //sendText.setMessage(boxMensaje.Text);
                        //                      sendText.sendMessage();
                        break;
                    default:

                        messageBox.Text = "No se reconoció el comando";
                        clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);

                        break;

                    //}

                    //break;

                }

            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //while(true){
           
            conectaActiva();
           // clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);
            //}
           // System.Diagnostics.Debug.WriteLine("YOOO");
        }



        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        private void messageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }   
        private void Window_Closed(object sender, EventArgs e)
        {
            if (this._sensor != null)
            {

                this._sensor.AudioSource.Stop();

                this._sensor.Stop();

                this._sensor = null;

            }

        }


        private void KinectTileButton_Click_1(object sender, RoutedEventArgs e)
        {
            clientSub.Publish(topicBox.Text, Encoding.UTF8.GetBytes(messageBox.Text)/*, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE*/);
        }
    
    
    }
}
