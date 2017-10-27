using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using Microsoft.VisualBasic;
using System.Text;
using System.Text.RegularExpressions;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfApplicationMD
{

        

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

public enum Direction { 左转直行,左转,直行,右转,右转直行 };
public partial class MainWindow//:MetroWindow 
    {
        private UdpClient sendJsonUdpClient;
        private UdpClient sendUdpClient;
        private UdpClient receiveUpdClient;
        private int i = 0;
        public IPAddress tmpSCip;
        public IPAddress tmpPLip;
        public String tmpEPip;
        public int tmpSCport;
        public int tmpPLport;
        public int tmpEPport;
        IPAddress ips = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First();
        public MainWindow()
        {
            InitializeComponent();
            
            EPip.Text = ips.ToString();
            SCip.Text = ips.ToString();
            PLip.Text = ips.ToString();
            tmpEPport = int.Parse(EPport.Text);

        }


    private int  randomNm(int n,int m)
{
    int iSeed = 10;
    Random ro = new Random(iSeed);
    long tick = DateTime.Now.Ticks;
    Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

    int iResult;
    int iUp = m;
    int iDown = n;
    iResult = ro.Next(iDown, iUp);
    return iResult;       
}

        public class Information
        {
            public int laneNub;
            public string direction;
            public int channelNub;
            public Information() { }

        };

        int intervalTime = 1;
        byte[] ChannelConfig = new Byte[5];
        int sa;
        int sb;
        bool randflag = false;
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //1.校验数据正确性        
            /***************************************************************/
            //发送间隔 0<a<b<10s

            if ((string.IsNullOrWhiteSpace(sendA.Text)) && (string.IsNullOrWhiteSpace(sendB.Text)))
            {
                MessageBox.Show("发送间隔时间不得为空");
                sendA.Background = Brushes.Coral;
                sendB.Background = Brushes.Coral;
                sendA.Focus();
            }
            if ((!string.IsNullOrWhiteSpace(sendA.Text)) && (!string.IsNullOrWhiteSpace(sendB.Text)))
            {
                sa = int.Parse(sendA.Text);
                sb = int.Parse(sendB.Text);
                if(( sa <= 0 )|| ( sb >= 10))
                {
                    MessageBox.Show("请输入(0,10) 大于 0 小于 10 的值!");
                    sendA.Background = Brushes.Coral;
                    sendB.Background = Brushes.Coral;
                    sendA.Focus();
                }
                else if(sb <= sa)
                {
                    MessageBox.Show("请确保第一个值小于第二个值");
                    sendA.Background = Brushes.White;
                    sendB.Background = Brushes.Coral;
                    sendB.Focus();
                }
                else
                {
                    sendA.Background = Brushes.White;
                    sendB.Background = Brushes.White;
                    //只在配置的时候随机一次---------可能需要改到组json包处
                    randflag = true;
                    intervalTime = randomNm(sa, sb);
                    MessageBox.Show("发送间隔随机", intervalTime.ToString());
                }
            }
            if ((string.IsNullOrWhiteSpace(sendA.Text)) && (!string.IsNullOrWhiteSpace(sendB.Text)))
            {
                int sb = int.Parse(sendB.Text);
                if ((0 < sb)&&(sb < 10))
                {
                    intervalTime = int.Parse(sendB.Text);
                    sendA.Background = Brushes.White;
                    sendB.Background = Brushes.LightBlue;
                    MessageBox.Show("发送间隔不随机", intervalTime.ToString());
                }
            }
            if ((!string.IsNullOrWhiteSpace(sendA.Text)) && (string.IsNullOrWhiteSpace(sendB.Text)))
            {
                int sa = int.Parse(sendA.Text);
                if ((0 < sa) && (sa < 10))
                {
                    intervalTime = int.Parse(sendA.Text);
                    sendA.Background = Brushes.LightBlue;
                    sendB.Background = Brushes.White;
                    MessageBox.Show("发送间隔不随机", intervalTime.ToString());
                }
            }
            //发送间隔 0<a<b<10s
            /***************************************************************/

            /***************************************************************/
            //ip正确性校验,port校验
            if (string.IsNullOrWhiteSpace(EPip.Text))
            {
                MessageBox.Show("必须填入电警IP");
                EPip.Background = Brushes.Coral;
            }
            else
            {
                EPip.Background = Brushes.White;
            }

            if (string.IsNullOrWhiteSpace(SCip.Text))
            {
                 MessageBox.Show("必须填入信号机IP");
                 SCip.Background = Brushes.Coral;
            }
            else
            {
                SCip.Background = Brushes.White;
            }

            if (string.IsNullOrWhiteSpace(PLip.Text))
            {
                MessageBox.Show("必须填入平台IP");
                PLip.Background = Brushes.Coral;
            }
            else
            {
                PLip.Background = Brushes.White;
            }
            if (string.IsNullOrWhiteSpace(PLport.Text))
            {
                MessageBox.Show("必须填入平台port");
                PLport.Background = Brushes.Coral;
            }
            else
            {
                PLport.Background = Brushes.White;
            }

            //ip正确性校验,port校验
            /***************************************************************/
            tmpSCip = IPAddress.Parse(SCip.Text);
            tmpSCport = int.Parse(SCport.Text);
            tmpPLip = IPAddress.Parse(PLip.Text);
            tmpPLport = int.Parse(PLport.Text);
            tmpEPip = EPip.Text;

            

            Thread tx = new Thread(new ThreadStart(ThreadReqStatus));
            tx.Start();

            Thread rx = new Thread(new ThreadStart(ThreadRcvStatus));
            rx.Start();

            Thread toPL = new Thread(new ThreadStart(ThreadUpJson));
            toPL.Start();

        }
        bool IsUdpcRecvStart = false;//开关:在监听udp报文阶段为true,否则为false

        byte[] ChannelStatus = new Byte[32];

        // 接收消息方法
        private void ReceiveMessage()
        {
           tmpSCport = 31662;
//          IPAddress remoteIp = ips;
           IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpSCip, tmpSCport);
           //IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, tmpSCport);
            while (true)
            {
                try
               {
                    // 关闭receiveUdpClient时此时会产生异常
                    byte[] receiveBytes = receiveUpdClient.Receive(ref remoteIpEndPoint);
                    // 显示消息内容
                     if(receiveBytes.Length < 320)
                    {
                        for(int i = 0;i<receiveBytes.Length;i++)
                        {
                            ShowMessageforView(lstbxMessageView, string.Format("{0}[{1}]:{2}", remoteIpEndPoint,receiveBytes[i],i));                           
                        }   
                    }
                    else 
                    {
                        for (int i = 288; i < 320; i++)
                        {
                            ChannelStatus[i - 288] = receiveBytes[i];
                            ShowMessageforView(lstbxMessageView, string.Format("{0}:[{1}]_{2}", remoteIpEndPoint,(i-288),ChannelStatus[i-288]));
                           
                        }
                    }
               }
               catch
              {
                    MessageBox.Show("异常退出recv");
                    break;
               }
            }
        }
        // 利用委托回调机制实现界面上消息内容显示
        delegate void ShowMessageforViewCallBack(ListBox listbox, string text);
        private void ShowMessageforView(ListBox listbox, string text)
        {
            if (!listbox.Dispatcher.CheckAccess())
            {
                ShowMessageforViewCallBack showMessageforViewCallback = ShowMessageforView;
                listbox.Dispatcher.Invoke(showMessageforViewCallback, new object[] { listbox, text });
            }
            else
            {
                lstbxMessageView.Items.Add(text);
                lstbxMessageView.SelectedIndex = lstbxMessageView.Items.Count - 1;
//                lstbxMessageView.ClearSelected();
            }
        }
        // 通过委托回调机制显示消息内容
        delegate void ShowMessageCallBack(ListBox listbox, string text);
        private void ShowMessage(ListBox listbox, string text)
        {
            if (listbox.Dispatcher.CheckAccess())
            {
                ShowMessageCallBack showmessageCallback = ShowMessage;
                listbox.Dispatcher.Invoke(showmessageCallback, new object[] { listbox, text });
            }
            else
            {
                listbox.Items.Add(text);
                listbox.SelectedIndex = listbox.Items.Count - 1;
                //               listbox.ClearSelected();
            }
        }
        Thread sendThread;

        public void ThreadReqStatus()
        {
 //           this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,(ThreadStart)delegate()
   //         {
                //线程action代码...待添加
       //         MessageBox.Show("我帅不帅?.! send 线程开始", "消息");
                //匿名模式
                sendUdpClient = new UdpClient(0);
                // 实名模式(套接字绑定到本地指定的端口)
/*                IPAddress localIp = IPAddress.Parse(EPip.Text);
                IPEndPoint localIpEndPoint = new IPEndPoint(localIp, int.Parse(EPport.Text));
                sendUdpClient = new UdpClient(localIpEndPoint);
*/
                sendThread = new Thread(SendMessage);
                sendThread.Start();
     //       });
        }
    Thread sendJsonThread;
        public void ThreadUpJson()
    {
        
        sendJsonUdpClient = new UdpClient(0);
        sendJsonThread = new Thread(SendJsonMessage);
        sendJsonThread.Start();

    }
        Thread receiveThread;
        public void ThreadRcvStatus()
        {
            if(!IsUdpcRecvStart)//未监听的情况,开始监听
            {
                IPEndPoint localIpep = new IPEndPoint(ips, tmpEPport);

                receiveUpdClient = new UdpClient(localIpep);
                receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();
                IsUdpcRecvStart = true;
                MessageBox.Show("recv 监听器已成功启动", "消息");
            }
            else//正在监听的情况,终止监听
            {
                receiveThread.Abort();//必须先关闭这个线程,否则会异常
                receiveUpdClient.Close();

                IsUdpcRecvStart = false;

                MessageBox.Show("UDP监听器已成功启动");

            }

        }

        private void SendJsonMessage(object obj)
        {
            DateTime dt = DateTime.Now.ToLocalTime() ;
            String nowTime = dt.ToString("yyyy-MM-dd hh:mm:ss");
            String arrivalTime = dt.AddSeconds(1).ToString();   //加n秒
            String throughTime = dt.AddSeconds(11).ToString();
            string Lane1Json = "\"time\":\"" + nowTime + "ipv4" + tmpEPip + "arrivalStopLineTime" + arrivalTime + "throughStopLineTime" + throughTime;
            ShowMessageforView(lstbxMessageView, Lane1Json);
            byte[] sendbytes = System.Text.Encoding.Default.GetBytes(Lane1Json);

            IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpPLip, tmpPLport);
            MyCircleQueue<JsonPack> jsonPackQ = new MyCircleQueue<JsonPack>(4);
            JsonPack Lane1JP = new JsonPack();
            Lane1JP.time = dt.ToString("yyyy-MM-dd hh:mm:ss");
            Lane1JP.ipV4 = tmpEPip;
            Lane1JP.LaneVehicleDir = "left";//straight,right,unknown
            Lane1JP.arrivalStopLineTime = dt.AddSeconds(1).ToString();  
            Lane1JP.throughStopLineTime = dt.AddSeconds(11).ToString();
            Lane1JP.sendSnapDataTime = Lane1JP.time;
            Lane1JP.laneNo = "1";
            jsonPackQ.Push(Lane1JP);
            jsonPackQ.Push(Lane1JP);
            jsonPackQ.Push(Lane1JP);
            jsonPackQ.Push(Lane1JP);


            int sleepMillsSec;
            if(randflag == true)
            {
                intervalTime = randomNm(sa, sb);                
            }
            sleepMillsSec = intervalTime * 1000;
            if(sleepMillsSec <= 0 )
            {
                //报错;
            }


            while (true)
            {
                System.Threading.Thread.Sleep(sleepMillsSec);
                sendUdpClient.Send(sendbytes, sendbytes.Length, remoteIpEndPoint);
            }
        }
        // 发送消息方法
        private void SendMessage(object obj)
        {
            byte[] sendbytes = new byte[]{0x6e,0x6e,0x0,0x0,0x9e,0x0,0x0,0x0};

 //        IPEndPoint remoteIpEndPoint = new IPEndPoint(remoteIp, int.Parse(tmpSCport));
            IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpSCip, 31662);
          
            while (true)
            {
                System.Threading.Thread.Sleep(5000);
                sendUdpClient.Send(sendbytes, sendbytes.Length, remoteIpEndPoint);             
            }
        }
  

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
 //           this.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
            sendThread.Abort();
            sendUdpClient.Close();
            
          //正在监听的情况,终止监听
                receiveThread.Abort();//必须先关闭这个线程,否则会异常
                receiveUpdClient.Close();
                IsUdpcRecvStart = false;
                MessageBox.Show("UDP监听器已成功启动");

                randflag = false;

        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            if (this.radiobutton1.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(textbox1.Text))
                {
                    MessageBox.Show("不得为空");
                    textbox1.Background = Brushes.Coral;
                    textbox1.Focus();
                }
                else
                {
                    string str = textbox1.Text;
                    string[] sArray = Regex.Split(str, ",", RegexOptions.IgnoreCase);
                    foreach (string i in sArray)
                    {
                        if (int.Parse(i.ToString()) > 32)
                        {
                            MessageBox.Show("通道号不得大于32");
                            textbox1.Background = Brushes.Coral;
                            textbox1.Focus();
                        }
                        else
                        {
                            textbox1.Background = Brushes.White;
                            ChannelConfig[0] = Byte.Parse(textbox1.Text);
                        }
                    }

                }
            }
            else
            {
                MessageBox.Show("选择相对的记录操作");
            }
            if (grid12.Visibility == Visibility.Visible)
            {
                if (this.radiobutton2.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textbox2.Text))
                    {
                        MessageBox.Show("不得为空");
                        textbox2.Background = Brushes.Coral;
                        textbox2.Focus();
                    }
                    else
                    {
                        string str = textbox2.Text;
                        string[] sArray = Regex.Split(str, ",", RegexOptions.IgnoreCase);
                        foreach (string i in sArray)
                        {
                            if (int.Parse(i.ToString()) > 32)
                            {
                                MessageBox.Show("通道号不得大于32");
                                textbox2.Background = Brushes.Coral;
                                textbox2.Focus();
                            }
                            else
                            {
                                textbox2.Background = Brushes.White;
                                ChannelConfig[1] = Byte.Parse(textbox2.Text);
                            }
                        }
                        
                    }
                }
                else
                {
                    MessageBox.Show("选择相对的记录操作");
                }
            }
            if (grid13.Visibility == Visibility.Visible)
            {
                if (this.radiobutton3.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textbox3.Text))
                    {
                        MessageBox.Show("不得为空");
                        textbox3.Background = Brushes.Coral;
                        textbox3.Focus();
                    }
                    else
                    {
                        string str = textbox3.Text;
                        string[] sArray = Regex.Split(str, ",", RegexOptions.IgnoreCase);
                        foreach (string i in sArray)
                        {
                            if (int.Parse(i.ToString()) > 32)
                            {
                                MessageBox.Show("通道号不得大于32");
                                textbox3.Background = Brushes.Coral;
                                textbox3.Focus();
                            }
                            else
                            {
                                textbox3.Background = Brushes.White;
                                ChannelConfig[2] = Byte.Parse(textbox3.Text);
                            }
                        }

                    }
                }
                else
                {
                    MessageBox.Show("选择相对的记录操作13");
                }
            }
            if (grid14.Visibility == Visibility.Visible)
            {
                if (this.radiobutton4.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textbox4.Text))
                    {
                        MessageBox.Show("不得为空");
                        textbox4.Background = Brushes.Coral;
                        textbox4.Focus();
                    }
                    else
                    {
                        string str = textbox4.Text;
                        string[] sArray = Regex.Split(str, ",", RegexOptions.IgnoreCase);
                        foreach (string i in sArray)
                        {
                            if (int.Parse(i.ToString()) > 32)
                            {
                                MessageBox.Show("通道号不得大于32");
                                textbox4.Background = Brushes.Coral;
                                textbox4.Focus();
                            }
                            else
                            {
                                textbox4.Background = Brushes.White;
                                ChannelConfig[3] = Byte.Parse(textbox4.Text);
                            }
                        }

                    }
                }
                else
                {
                    MessageBox.Show("选择相对的记录操作14");
                }
            }
            if (grid15.Visibility == Visibility.Visible)
            {
                if (this.radiobutton5.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textbox5.Text))
                    {
                        MessageBox.Show("不得为空");
                        textbox5.Background = Brushes.Coral;
                        textbox5.Focus();
                    }
                    else
                    {
                        string str = textbox5.Text;
                        string[] sArray = Regex.Split(str, ",", RegexOptions.IgnoreCase);
                        foreach (string i in sArray)
                        {
                            if (int.Parse(i.ToString()) > 32)
                            {
                                MessageBox.Show("通道号不得大于32");
                                textbox5.Background = Brushes.Coral;
                                textbox5.Focus();
                            }
                            else
                            {
                                textbox5.Background = Brushes.White;
                                ChannelConfig[4] = Byte.Parse(textbox5.Text);
                            }
                        }

                    }
                }
                else
                {
                    MessageBox.Show("选择相对的记录操作");
                }
            } 

            
        }

        private void textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textbox1.Text))
            {
                MessageBox.Show("不得为空");
                textbox1.Focus();
            }
        }

        int p = 1;
        private void btnAddd_Click(object sender, RoutedEventArgs e)
        {
            
            if (p > 6)
            {
               p = 1;
            }
            p++;

            if (p == 2)
            { grid12.Visibility = Visibility.Visible; }
            if (p == 3)
            { grid13.Visibility = Visibility.Visible; }
            if (p == 4)
            { grid14.Visibility = Visibility.Visible; }
            if (p == 5)
            { grid15.Visibility = Visibility.Visible; }


        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
              if(radiobutton1.IsChecked == true)
              {
                  MessageBox.Show("至少配置1项");

              }
            if(radiobutton2.IsChecked == true)
            {
                grid12.Visibility = Visibility.Hidden;
                grid13.Visibility = Visibility.Hidden;
                grid14.Visibility = Visibility.Hidden;
                grid15.Visibility = Visibility.Hidden;
                radiobutton2.IsChecked = false;
                radiobutton3.IsChecked = false;
                radiobutton4.IsChecked = false;
                radiobutton5.IsChecked = false;
                textbox2.Text = null;
                textbox3.Text = null;
                textbox4.Text = null;
                textbox5.Text = null;

            }
            if (radiobutton3.IsChecked == true)
            {
                grid13.Visibility = Visibility.Hidden;
                grid14.Visibility = Visibility.Hidden;
                grid15.Visibility = Visibility.Hidden;
                radiobutton3.IsChecked = false;
                radiobutton4.IsChecked = false;
                radiobutton5.IsChecked = false;
                textbox3.Text = null;
                textbox4.Text = null;
                textbox5.Text = null;
            }
            if (radiobutton4.IsChecked == true)
            {
                grid14.Visibility = Visibility.Hidden;
                grid15.Visibility = Visibility.Hidden;
                radiobutton4.IsChecked = false;
                radiobutton5.IsChecked = false;
                textbox4.Text = null;
                textbox5.Text = null;
            }
            if (radiobutton5.IsChecked == true)
            {
                grid15.Visibility = Visibility.Hidden;
                radiobutton5.IsChecked = false;
                textbox5.Text = null;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lstbxMessageView.Items.Clear();
        }  
    }
}


