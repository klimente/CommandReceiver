using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace cmdsender
{
    class Program
    {

        static void Main(string[] args)
        {
            XmlDocument doc_users = new XmlDocument();
            doc_users.Load("c:\\users\\andrey\\source\\repos\\ServerMem\\ServerMem\\users.xml");
            int users_c;
            users_c = 0;
            string[] users = new string[50];
            string[] IPsadd = new string[50];
            int[] ports = new int[50];
            foreach (XmlNode node in doc_users.DocumentElement.ChildNodes)
            {
                string text = node.InnerText;
                string ipaddres = node.Attributes["IP"].Value;
                int freeport = Int32.Parse(node.Attributes["port"].Value);
                users[users_c] = text;
                IPsadd[users_c] = ipaddres;
                ports[users_c] = freeport;
                users_c++;
            }

            int arg_counter = 0;
            string[] words = new string[50];
            foreach (string word in args)
            {
                words[arg_counter] = args[arg_counter];
                arg_counter++;
                Console.WriteLine(word);
            }

            try
            {
                if (System.Array.IndexOf(users, words[0]) != -1)
                {
                    int port = ports[Array.IndexOf(users, words[0])];
                    string endipaddr = IPsadd[Array.IndexOf(users, words[0])];

                    try
                    {
                        using (StreamWriter w = File.AppendText("logcmdsender.txt"))
                        {
                            Log(words[0] + words[1], w);

                        }
                        SendMessageFromSocket(port, endipaddr, words[1], words[2]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        using (StreamWriter w = File.AppendText("logcmdsender.txt"))
                        {
                            Log(ex.ToString(), w);

                        }
                    }
                    finally
                    {
                        Console.ReadLine();
                    }

                }

                if (System.Array.IndexOf(users, words[0]) == -1)
                {
                    Console.WriteLine("Такого пользователся не существует");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void SendMessageFromSocket(int port, string endipaddr, string messagecom, string messageip)
        {
            byte[] bytes = new byte[1024];



            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(endipaddr), port);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(ipEndPoint);

            string message = messagecom + " " + messageip;

            Console.WriteLine("Сокет соединяется с {0}", sender.RemoteEndPoint.ToString());
            byte[] msg = Encoding.UTF8.GetBytes(message);

            int bytesSent = sender.Send(msg);

            int bytesRec = sender.Receive(bytes);

            Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
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
