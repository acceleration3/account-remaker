using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace AccountRemaker
{
    public class ChatangoAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }

        private string requestURL;
        private bool run = false;
        private int retries = 0;
        private BackgroundWorker statusWorker;
        private bool confirmed = false;

        public ChatangoAccount(string username, string password, string email)
        {
            this.Username = username;
            this.Password = password;
            this.Email = email;
            this.Status = "Ready";
            this.requestURL = "http://ust.chatango.com/profileimg/" + this.Username[0] + "/" + this.Username[1] + "/" + this.Username + "/mod1.xml";
        }

        public void startChecking(Main mainForm)
        {
            run = true;
            retries = 0;

            statusWorker = new BackgroundWorker();
            statusWorker.DoWork += StatusWorker_DoWork;
            statusWorker.RunWorkerAsync(mainForm);
        }

        public void stopChecking()
        {
            run = false;
        }

        private void StatusWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while(run)
            {
                HttpWebRequest httpRequest = WebRequest.CreateHttp(requestURL);
                HttpWebResponse webResponse = (HttpWebResponse)httpRequest.GetResponse();

                if(webResponse.StatusCode != HttpStatusCode.OK)
                {
                    this.Status = "Error";
                    continue;
                }

                string xmlData = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();

                if(!xmlData.Contains("mod") && confirmed == false)
                {
                    this.Status = "Doesn't exist";
                    this.run = false;
                }
                else if(!xmlData.Contains("mod") && confirmed == true)
                {
                    TcpClient socket = new TcpClient();

                    string remakePostData = "email=" + this.Email + "&login=" + this.Username + "&password=" + this.Password + "&password_confirm=" + this.Password + "&storecookie=on&signupsubmit=Sign+up&checkerrors=yes";

                    socket.Connect("chatango.com", 80);

                    StreamWriter writer = new StreamWriter(socket.GetStream());
                    writer.Write("POST /signupdir HTTP/1.1\r\n");
                    writer.Write("Host: chatango.com\r\n");
                    writer.Write("Connection: keep-alive\r\n");
                    writer.Write("Content-Length: " + remakePostData.Length + "\r\n");
                    writer.Write("Cache-Control: max-age=0\r\n");
                    writer.Write("Origin: http://chatango.com\r\n");
                    writer.Write("Upgrade-Insecure-Requests: 1\r\n");
                    writer.Write("User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36\r\n");
                    writer.Write("Content-Type: application/x-www-form-urlencoded\r\n");
                    writer.Write("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8\r\n");
                    writer.Write("Referer: http://chatango.com/signupdir\r\n");
                    writer.Write("Accept-Encoding: gzip, deflate\r\n");
                    writer.Write("Accept-Language: en-US,en;q=0.8\r\n");
                    writer.Write("Cookie: sm_dapi_session=true; H1:b9deec981aa9dbc478b09607f5da7=1; cookies_enabled.chatango.com=yes; fph.chatango.com=http\r\n\r\n");
                    writer.Write(remakePostData);

                    writer.Flush();

                    StreamReader reader = new StreamReader(socket.GetStream());

                    string htmlResult = reader.ReadToEnd();

                    if (htmlResult.Contains("Download Message Catcher"))
                    {
                        this.Status = "Success!";
                        this.run = false;
                    }
                    else
                    {
                        this.Status = "Failed to remake?";
                        this.run = false;
                    }

                    writer.Close();
                    socket = null;
                }
                else if(xmlData.Contains("mod") && confirmed == false)
                {
                    this.confirmed = true;
                    this.retries++;
                    this.Status = "Failed(1)";
                }
                else if(xmlData.Contains("mod") && confirmed == true)
                {
                    this.retries++;
                    this.Status = "Failed(" + this.retries + ")";
                }

                ((Main)e.Argument).UpdateList();

                Thread.Sleep(2);
            }
        }
    }
}
