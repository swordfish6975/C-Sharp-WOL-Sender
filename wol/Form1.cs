using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System.Diagnostics;

namespace wol
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
            BGW.RunWorkerAsync();
        }

        private void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            IPAddress ipAddress;

            try
            {
                IPAddress[] temp = Dns.GetHostAddresses(tbxIP.Text);
                ipAddress = temp[0];

                if (temp.Count() > 1)
                    BGW.ReportProgress(15, "Multiple IP for DNS (using first one)");
            }
            catch
            {
                BGW.ReportProgress(10, "Failed to get IP from DNS");
                return;
            }
            var response = WakeFunction(tbxMAC.Text, ipAddress, int.Parse(txbPort.Text));


            if (response[0] == response[1])
                BGW.ReportProgress(5, "Sent Magic Packet");
            else
                BGW.ReportProgress(10, "Only" + response[0] + " bytes of " + response[1] + "bytes sent");
        }



        private List<int> WakeFunction(string MAC_ADDRESS, IPAddress ipAddress, int port)
        {

            MAC_ADDRESS = MAC_ADDRESS.Replace(":","");
            MAC_ADDRESS = MAC_ADDRESS.Replace("-", "");
            System.Threading.Thread.Sleep(100);
            BGW.ReportProgress(5, "Mac Address:" + MAC_ADDRESS);

            if (MAC_ADDRESS.Length != 12)
            {
                BGW.ReportProgress(10, "Mac Address Incorrect Length (should be 12)");
                return new List<int> { 0, 0 };
            }
            
            System.Threading.Thread.Sleep(100);
            BGW.ReportProgress(5, "Create Packet...");
            int counter = 0;
            //buffer to be send
            byte[] bytes = new byte[1024];   // more than enough :-)
            //first 6 bytes should be 0xFF
            for (int y = 0; y < 6; y++)
                bytes[counter++] = 0xFF;
            //now repeate MAC 16 times
            for (int y = 0; y < 16; y++)
            {
                int i = 0;
                for (int z = 0; z < 6; z++)
                {
                    bytes[counter++] =
                        byte.Parse(MAC_ADDRESS.Substring(i, 2),
                        NumberStyles.HexNumber);
                    i += 2;
                }
            }

            //now send wake up packet
            System.Threading.Thread.Sleep(100);
            BGW.ReportProgress(5, "Packet Made...");
            System.Threading.Thread.Sleep(100);
            BGW.ReportProgress(5, "About to send packet...");
            System.Threading.Thread.Sleep(100);
            return sendUDPMessage(ipAddress.ToString(), port, port, bytes);
        }

        public  List<int> sendUDPMessage(string ipAddress, int fromPort, int toPort, byte[] send_buffer)
        {

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), fromPort);

            BGW.ReportProgress(5, "Remote Endpoint: " + endPoint.ToString());

            UdpClient client = new UdpClient(toPort);
            var sentBytes = client.Send(send_buffer, send_buffer.Length, endPoint);
            client.Close();

            System.Threading.Thread.Sleep(100);
            return (new List<int> { sentBytes, send_buffer.Length });

        }


        private void BGW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 5)
            {
                rtbOutput.SelectionColor = Color.Green;
              
            }
            else if (e.ProgressPercentage == 10) //red
            {
                rtbOutput.SelectionColor = Color.Red;
            }
            else if (e.ProgressPercentage == 15) //orange
            {
                rtbOutput.SelectionColor = Color.Orange;
            }

            if (rtbOutput.Text == "")
                rtbOutput.AppendText(e.UserState.ToString());
            else
                rtbOutput.AppendText(Environment.NewLine + e.UserState.ToString());
        }

        private void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            rtbOutput.SelectionColor = Color.Green;
            rtbOutput.AppendText(Environment.NewLine + "Done");

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbxMAC.Text = Properties.Settings.Default.MAC;
            tbxIP.Text =  Properties.Settings.Default.IP;
            txbPort.Text = Properties.Settings.Default.Port;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.MAC = tbxMAC.Text;
            Properties.Settings.Default.IP = tbxIP.Text;
            Properties.Settings.Default.Port = txbPort.Text ;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("These instructions are writen for a particular router firmware, they may give you an idea how to configure your own. (step 3 is not required)");
            Process.Start("http://www.dd-wrt.com/wiki/index.php/WOL#Remote_Wake_On_LAN_via_Port_Forwarding");
        }

    }

  
 
}
