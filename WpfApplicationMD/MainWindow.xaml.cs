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
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace WpfApplicationMD
{
    public class comboDir
    {
        public Dir ID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Desc { get; set; }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

public enum Direction { 左转直行,左转,直行,右转,右转直行 };
public partial class MainWindow//:MetroWindow 
    {
        private UdpClient sendJsonUdpClient;

        private UdpClient sendUdpClient;
    
        private UdpClient receiveUpdClient;
              
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

            LodData();
            
            EPip.Text = ips.ToString();
            SCip.Text = ips.ToString();
            PLip.Text = ips.ToString();
            tmpEPport = int.Parse(EPport.Text);
        }

        private void LodData()
        {
            IList<comboDir> dirList = new List<comboDir>();
            //项目文件中新建一个images文件夹，并上传了001.png，002.png,003.png
            dirList.Add(new comboDir() { ID = Dir.TL, Name = "左转", Image = "/resource/dir_img/1.png", Desc = "Left" });
            dirList.Add(new comboDir() { ID = Dir.SL, Name = "直左", Image = "/resource/dir_img/2.png", Desc = "Straight & Left" });
            dirList.Add(new comboDir() { ID = Dir.TS, Name = "直行", Image = "/resource/dir_img/3.png", Desc = "Straight" });
            dirList.Add(new comboDir() { ID = Dir.TR, Name = "右转", Image = "/resource/dir_img/4.png", Desc = "Right" });
            dirList.Add(new comboDir() { ID = Dir.SR, Name = "直右", Image = "/resource/dir_img/5.png", Desc = "Straight & Right" });

            this.lane1Combox.ItemsSource = dirList;//数据源绑定
            this.lane1Combox.SelectedValue = dirList[0];//默认选择项
            this.lane2Combox.ItemsSource = dirList;//数据源绑定
            this.lane2Combox.SelectedValue = dirList[1];//默认选择项
            this.lane3Combox.ItemsSource = dirList;//数据源绑定
            this.lane3Combox.SelectedValue = dirList[2];//默认选择项
            this.lane4Combox.ItemsSource = dirList;//数据源绑定
            this.lane4Combox.SelectedValue = dirList[3];//默认选择项
        }


    public static int  randomNm(int n,int m)
{
    Random ran = new Random();

    int iResult = ran.Next(n, m);
    return iResult;
 }

        int intervalTime = 1;
        byte[] ChannelConfig = new Byte[5];
        string[] dirConfig = new string[5];
        int[] dir_C = new int[5];
        int sa;
        int sb;

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
 //                   randflag = true;
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
            
            //请求 信号机状态线程
            Thread tx = new Thread(new ThreadStart(ThreadReqStatus));
            tx.Start();
            //接收 信号机状态线程
            Thread rx = new Thread(new ThreadStart(ThreadRcvStatus));
            rx.Start();

            //组包 线程
            Thread packjson = new Thread(new ThreadStart(ThreadPackJsonToQueue));
            packjson.Start();

            //发送 json给平台线程
            Thread toPL = new Thread(new ThreadStart(ThreadUpJson));
            toPL.Start();
        }

        bool IsUdpcRecvStart = false;//开关:在监听udp报文阶段为true,否则为false

        byte[] ChannelStatus = new Byte[32];

        // 接收消息方法
        private void ReceiveMessage()
        {
                       tmpSCport = 31662;
            //           IPAddress remoteIp = ips;
            IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpSCip, tmpSCport);
            //         IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, tmpSCport);
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
                    MessageBox.Show("信号机监听器异常退出");
                    break;
               }
//                System.Threading.Thread.Sleep(1000);
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
//                lstbxMessageView.Items.Add(text);
  //              lstbxMessageView.SelectedIndex = lstbxMessageView.Items.Count - 1;
//              lstbxMessageView.ClearSelected();
                listbox.Items.Add(text);
                listbox.SelectedIndex = lstbxMessageView.Items.Count - 1;
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
//              listbox.ClearSelected();
            }
        }
        Thread sendThread;

        public void ThreadReqStatus()
        {
                sendUdpClient = new UdpClient();
                sendThread = new Thread(SendMessage);
                sendThread.Start();
        }
    Thread sendJsonThread;
        public void ThreadUpJson()
    {
        
        sendJsonUdpClient = new UdpClient();
        sendJsonThread = new Thread(SendJsonMessage);
        sendJsonThread.Start();

    }
        string jsonhh;
        MyCircleQueue<JsonPack> jsonPackQ = new MyCircleQueue<JsonPack>(20);
        public void ThreadPackJsonToQueue()
        {
            int sleepMillsSec;
            while (true)
            {
                configInfo config = new configInfo();
                config.arrivalTime = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss");
                config.throughTime = DateTime.Now.ToLocalTime().AddSeconds(1).ToString();
                config.sendTime = DateTime.Now.ToLocalTime().AddSeconds(11).ToString(); for (int i = 0; i < 4; i++)

 //               if (randflag == true)
 //               {
                    intervalTime = randomNm(sa, sb);
 //               }
                sleepMillsSec = intervalTime * 1000;
                if (sleepMillsSec <= 0)
                {
                    //报错;
                }
                for (int k = 0; ChannelConfig[k] != 0 && k < 4; k++)
                    {
                        if (ChannelConfig[k] != 0)
                        {
                            config.Channel_Cof = ChannelConfig[k].ToString();
                            config.ipV4_Cof = tmpEPip;
                            //           config.dirFlag = (Dir)Enum.Parse(typeof(Dir), dirConfig[i], false);
                            config.dirFlag = (Dir)dir_C[k];
                            config.laneNo_Cof = (k + 1).ToString();
                            JsonPack jsClass = new JsonPack(config);
                            jsonhh = jsClass.ClassToJson();
                            //                    ShowMessageforView(lstbxMessageView, jsonhh);
                            jsonPackQ.Push(jsClass);
                        }
                    }
                System.Threading.Thread.Sleep(sleepMillsSec);

            }
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
                MessageBox.Show("信号机监听器已成功启动", "消息");

            }
            else//正在监听的情况,终止监听
            {
                receiveThread.Abort();//必须先关闭这个线程,否则会异常
                receiveUpdClient.Close();

                IsUdpcRecvStart = false;

                MessageBox.Show("信号机监听器已关闭", "消息");

            }

        }
    
        private void SendJsonMessage(object obj)
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpPLip, tmpPLport);
            DateTime dt = DateTime.Now.ToLocalTime() ;
            String nowTime = dt.ToString("yyyy-MM-dd hh:mm:ss");
            String arrivalTime = dt.AddSeconds(1).ToString();   //加n秒
            String throughTime = dt.AddSeconds(11).ToString();
            string sendjsStr;
            byte[] sendbytes;

//          String Lane1Json = "\"time\":\"" + nowTime + "ipv4" + tmpEPip + "arrivalStopLineTime" + arrivalTime + "throughStopLineTime" + throughTime;
//          ShowMessageforView(lstbxMessageView, Lane1Json);
            while (true)
            {
                if (!jsonPackQ.IsEmpty())
                {
                    for (int i = 0; ChannelConfig[i] != 0 && i < 5; i++)
                    {
                        JsonPack sendjs = jsonPackQ.FrontItem();
                        int tmpChannel = int.Parse(sendjs.Channel);
                        DateTime t1 = Convert.ToDateTime(sendjs.sendSnapDataTime);
                        DateTime t2 = DateTime.Now.ToLocalTime();
//                        if (DateTime.Compare(t1, t2) == 0)
 //                       {
                            if ((ChannelStatus[tmpChannel-1] == 1) ||( ChannelStatus[tmpChannel-1] == 4))
                            {

                                sendjsStr = sendjs.ClassToJson();
                                sendbytes = System.Text.Encoding.Default.GetBytes(sendjsStr);
                                sendUdpClient.Send(sendbytes, sendbytes.Length, remoteIpEndPoint);
 //                               lstbxMessageView.Items.Clear();

                                ShowMessageforView(lstbxMessageView2, sendjsStr);                          
             //                   jsonPackQ.Pop();
                            }
                            jsonPackQ.Pop();
                       }
                  }
                
                System.Threading.Thread.Sleep(11000);
            }
        }

        // 发送消息方法
        private void SendMessage(object obj)
        {
           byte[] sendbytes = new byte[]{0x6e,0x6e,0x0,0x0,0x9e,0x0,0x0,0x0};
           tmpSCport = 31662;
         IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpSCip, tmpSCport);
/*
          String str = "0x6e,0x6e,0x0,0x0,0x9e,0x0,0x0,0x0";
          String[] str1 = str.Replace(" ", "").Split(',');
          byte[] b = new byte[str1.Length];
          for (int i = 0; i < str1.Length; i++)
          {
              b[i] = Convert.ToByte(Convert.ToInt32(str1[i], 16));
          }
*/
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                sendUdpClient.Send(sendbytes, sendbytes.Length, remoteIpEndPoint);
 //               sendUdpClient.Send(b, b.Length, remoteIpEndPoint);             
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

            sendJsonThread.Abort();
            sendJsonUdpClient.Close();
            //tx.Abort();
            //rx.Abort();
            //packjson.Abort();
            //toPL.Abort();

                IsUdpcRecvStart = false;
                MessageBox.Show("模拟电警已停止");

//                randflag = false;

        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
//            MessageBox.Show((this.lane1Combox.SelectedItem as comboDir).ID.ToString());
 
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
                            dirConfig[0] = (this.lane1Combox.SelectedItem as comboDir).ID.ToString();
                            dir_C[0] = (int)(this.lane1Combox.SelectedItem as comboDir).ID;
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
                                dirConfig[1] = (this.lane2Combox.SelectedItem as comboDir).ID.ToString();
                                dir_C[1] = (int)(this.lane2Combox.SelectedItem as comboDir).ID;
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
                                dirConfig[2] = (this.lane3Combox.SelectedItem as comboDir).ID.ToString();
                                dir_C[2] = (int)(this.lane3Combox.SelectedItem as comboDir).ID;
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
                                dirConfig[3] = (this.lane4Combox.SelectedItem as comboDir).ID.ToString();
                                dir_C[3] = (int)(this.lane4Combox.SelectedItem as comboDir).ID;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("选择相对的记录操作14");
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
                radiobutton2.IsChecked = false;
                radiobutton3.IsChecked = false;
                radiobutton4.IsChecked = false;
                textbox2.Text = null;
                textbox3.Text = null;
                textbox4.Text = null;
            }
            if (radiobutton3.IsChecked == true)
            {
                grid13.Visibility = Visibility.Hidden;
                grid14.Visibility = Visibility.Hidden;
                radiobutton3.IsChecked = false;
                radiobutton4.IsChecked = false;
                textbox3.Text = null;
                textbox4.Text = null;
            }
            if (radiobutton4.IsChecked == true)
            {
                grid14.Visibility = Visibility.Hidden;
                radiobutton4.IsChecked = false;
                textbox4.Text = null;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lstbxMessageView.Items.Clear();
        }



    }
}


