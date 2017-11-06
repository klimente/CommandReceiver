using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Xml;
using System.IO;

namespace CommandReceiver
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text);

        static string availableIP = System.Configuration.ConfigurationManager.AppSettings["IP"];
        static string availablePort = System.Configuration.ConfigurationManager.AppSettings["port"];
        Thread t = null;
        XmlDocument doc = new XmlDocument();

        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(availableIP), int.Parse(availablePort));
        Socket sListner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();

            try
            {
                doc.Load("utility.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Файл utility не найден или находится не в корневой директории");
                Application.Exit();
            }
            t = new Thread(DoWork);
            t.Start();
        }


        public void DoWork()
        {
            string[] util = new string[50];
            int counter;
            counter = 0;
            string availiblecommand = "";
            try
            {
                sListner.Bind(ipEndPoint);
                sListner.Listen(10);

                while (true)
                {
                    this.SetText("Ожидаем соединение через порт ");
                    Socket handler = sListner.Accept();
                    string remoteip = ((IPEndPoint)(handler.RemoteEndPoint)).Address.ToString();
                    this.SetText(remoteip);
                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        string text = node.InnerText;
                        this.SetText(text);
                        util[counter] = text;
                        counter++;
                        availiblecommand += "\n" + text;
                    }
                    //byte[] avcommands = Encoding.UTF8.GetBytes(availiblecommand);
                    //handler.Send(avcommands);
                    string data = null;

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    using (StreamWriter w = File.AppendText("logcmdrec.txt"))
                    {
                        Log(data, w);

                    }

                    string[] command = data.Split(new char[] { ' ' });

                    string verifUtil = command[0];
                    this.SetText("Полученный текст:" + data + "\n\n");
                    if (data != "" & (System.Array.IndexOf(util, verifUtil) != -1))
                    {

                        this.SetText(data);
                        System.Diagnostics.ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo();
                        proc.FileName = "CMD.exe";
                        proc.Arguments = "/c " + data;
                        System.Diagnostics.Process.Start(proc);
                        string reply = "Команда выполнена";
                        //outputfile.WriteLine(reply);
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }

                    if (System.Array.IndexOf(util, verifUtil) == -1)
                    {
                        string reply = "Такой команды нет в списке ";

                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }


                    if (data.IndexOf("<TheEnd>") > -1)
                    {
                        this.SetText("Сервер завершил соединение с клиентом");
                        break;

                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
            }
            catch (Exception ex)
            {
                this.SetText(ex.ToString());
                using (StreamWriter w = File.AppendText("logcmdrec.txt"))
                {
                    Log(ex.ToString(), w);

                }
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private void SetText(string text)
        {

            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text + "   " });
            }
            else
            {
                this.textBox1.Text = this.textBox1.Text + text + "   ";
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            this.Show();
        }



        private void Form1_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.ShowBalloonTip(1000, "Сервер работает", " ", ToolTipIcon.Info);
            }
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry :");
            w.WriteLine("{0} {1}", DateTime.Now.Year + " " + DateTime.Now.Month + " " + DateTime.Now.Day + " ", DateTime.Now.ToLongTimeString());

            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------");
        }
    }
}
