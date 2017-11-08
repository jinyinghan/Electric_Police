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
using System.Diagnostics;



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
        public IPAddress tmpSCip;
        public String tmpSCipSTR;
        public String tmpEPip;
        public int tmpSCport;
        public String tmpEPport;
        public IPAddress tmpPLip;
        public int tmpPLport;
        public Stopwatch stopwatch;

        IPAddress ips = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First();
        public MainWindow()
        {
            InitializeComponent();

            LodData();
            String must = "10.21.48.135";
            SCip.Text = must;
            EPip.Text = ips.ToString();
//            SCip.Text = ips.ToString();
 /*           PLip.Text = ips.ToString();*/
//            tmpEPport = EPport.Text;
            tmpPLip = ips;
            tmpPLport = 21338;
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
        Thread tx;
        Thread packjson;
        Thread toSC;

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //1.校验数据正确性        
            /***************************************************************/
            //发送间隔 0<a<b<10s

            if ((string.IsNullOrWhiteSpace(sendA.Text)) && (string.IsNullOrWhiteSpace(sendB.Text)))
            {
//                MessageBox.Show("发送间隔时间不得为空");
                ShowMessageforView(lstbxMessageView2, "发送间隔时间不得为空");

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
 //                   MessageBox.Show("请输入(0,10) 大于 0 小于 10 的值!");
                    ShowMessageforView(lstbxMessageView2, "请输入(0,10) 大于 0 小于 10 的值!");

                    sendA.Background = Brushes.Coral;
                    sendB.Background = Brushes.Coral;
                    sendA.Focus();
                }
                else if(sb <= sa)
                {
    //                MessageBox.Show("请确保第一个值小于第二个值");
                    ShowMessageforView(lstbxMessageView2, "请确保第一个值小于第二个值");
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
 //                   MessageBox.Show("发送间隔随机", intervalTime.ToString());
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
 //                   MessageBox.Show("发送间隔不随机", intervalTime.ToString());
                    ShowMessageforView(lstbxMessageView2, "发送间隔不随机" + intervalTime.ToString());
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
   //                 MessageBox.Show("发送间隔不随机", intervalTime.ToString());
                    ShowMessageforView(lstbxMessageView2, "发送间隔不随机" + intervalTime.ToString());

                }
            }
            //发送间隔 0<a<b<10s
            /***************************************************************/

            /***************************************************************/
            //ip正确性校验,port校验
            if (string.IsNullOrWhiteSpace(EPip.Text))
            {
   //             MessageBox.Show("必须填入电警IP");
                ShowMessageforView(lstbxMessageView2, "必须填入电警IP");
                EPip.Background = Brushes.Coral;
            }
            else
            {
                EPip.Background = Brushes.White;
            }

            if (string.IsNullOrWhiteSpace(SCip.Text))
            {
   //              MessageBox.Show("必须填入信号机IP");
                 ShowMessageforView(lstbxMessageView2, "必须填入信号机IP");
                 SCip.Background = Brushes.Coral;
            }
            else
            {
                SCip.Background = Brushes.White;
            }

            //ip正确性校验,port校验
            /***************************************************************/
//            commit_bt.IsEnabled = false;
//            add_bt.IsEnabled = false;
//            delete_bt.IsEnabled = false;
            tmpSCip = IPAddress.Parse(SCip.Text);
            tmpSCipSTR = SCip.Text;
            tmpSCport = int.Parse(SCport.Text);
            tmpEPport = EPport.Text;
            tmpEPip = EPip.Text;
            
            //请求 信号机状态线程
             tx = new Thread(new ThreadStart(ThreadReqStatus));
            tx.Start();

            //组包 线程
             packjson = new Thread(new ThreadStart(ThreadPackJsonToQueue));
            packjson.Start();

            //发送 json给平台线程
//            Thread toPL = new Thread(new ThreadStart(ThreadUpJson));
//            toPL.Start();

            //Tcp发送 json给信号机线程
             toSC = new Thread(new ThreadStart(ThreadTCP));
             toSC.Start();
            stopwatch = new Stopwatch();
           stopwatch.Start();


        }

        bool IsUdpcRecvStart = false;//开关:在监听udp报文阶段为true,否则为false

        byte[] ChannelStatus = new Byte[32];

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
 /*   Thread sendJsonThread;
        public void ThreadUpJson()
    {
        
        sendJsonUdpClient = new UdpClient();
        sendJsonThread = new Thread(SendJsonMessage);
        sendJsonThread.Start();

    }
  * */
        private void ConnectCallback(IAsyncResult ar)
        {
            connectDone.Set();
            TcpClient t = (TcpClient)ar.AsyncState;
            try
            {
                if (t.Connected)
                {
   //                 MessageBox.Show("信号机连接成功");
                    ShowMessageforView(lstbxMessageView2, "信号机连接成功");
                    t.EndConnect(ar);
//                    MessageBox.Show("连接线程完成");
                }
                else
                {
   //                 MessageBox.Show("信号机连接失败");
                    ShowMessageforView(lstbxMessageView2, "信号机连接失败");
                    t.EndConnect(ar);
                }

            }
            catch //(SocketException se)
            {
 //               MessageBox.Show("连接发生错误ConnCallBack.......:" + se.Message);
//                MessageBox.Show("信号机不支持电警数据转发功能", "警告");
                ShowMessageforView(lstbxMessageView2, "信号机不支持电警数据转发功能");
            }
        }
        private void DisConnect()
        {
            if ((tcpClient != null) && (tcpClient.Connected))
            {
                ns.Close();
                tcpClient.Close();
            }
        }
        /// <summary>

        /// 异步连接

        /// </summary>

        private void Connect()
        {
            if ((tcpClient == null) || (!tcpClient.Connected))
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.ReceiveTimeout = 10;


                    connectDone.Reset();

//                    MessageBox.Show("尝试数据上传到信号机: " + tmpSCipSTR);

                    tcpClient.BeginConnect(tmpSCip, 7200,
                        new AsyncCallback(ConnectCallback), tcpClient);

                    connectDone.WaitOne();

                    if ((tcpClient != null) && (tcpClient.Connected))
                    {
                        ns = tcpClient.GetStream();

//                        MessageBox.Show("Connection established");

                        //                        asyncread(tcpClient);
                    }
                }
                catch (Exception se)
                {
 //                   MessageBox.Show(se.Message + " Conn......." + Environment.NewLine);
                    ShowMessageforView(lstbxMessageView2, se.Message + " Conn......." + Environment.NewLine);
                }
            }
        }
        TcpClient tcpClient = null;
        NetworkStream ns = null;
    static string nowT = DateTime.Now.ToLocalTime().ToString("HH_mm_ss");
        static string filename = "C:/jsonlog"+ nowT +".txt";
    System.IO.StreamWriter sw = new System.IO.StreamWriter(filename);
       
                               
                                
        public ManualResetEvent connectDone = new ManualResetEvent(false);
    public void ThreadTCP()
        {
            if ((tcpClient != null) && (tcpClient.Connected))
            {
                DisConnect();
            }
            else
            {
                Connect();
            }
            string sendjsStr;
            byte[] sendbytes;
            System.Threading.Thread.Sleep(11000);
                while (true)
                {
                    if (!jsonPackQ.IsEmpty())
                    {
                        for (int i = 0; ChannelConfig[i] != 0 && i < 4; i++)
                        {
                            JsonPack sendjs = jsonPackQ.FrontItem();
                            int tmpChannel = int.Parse(sendjs.Channel);
                            DateTime t1 = Convert.ToDateTime(sendjs.sendSnapDataTime);
                            DateTime t2 = DateTime.Now.ToLocalTime();
                            //                        if (DateTime.Compare(t1, t2) == 0)
                            //                       {
                            if ((ChannelStatus[tmpChannel - 1] == 1) || (ChannelStatus[tmpChannel - 1] == 4))
                            {
                                sendjsStr = sendjs.ClassToJson();
                                sendbytes = System.Text.Encoding.Default.GetBytes(sendjsStr);
                                byte[] addHead = new Byte[132];
                                Int32 dwLength = addHead.Length + sendbytes.Length;

                                addHead[0] = (byte)(dwLength & 0xFF);
                                addHead[1] = (byte)((dwLength & 0xFF00) >> 8);
                                addHead[2] = (byte)((dwLength & 0xFF0000) >> 16);
                                addHead[3] = (byte)((dwLength >> 24) & 0xFF);
                                addHead[6] = 0x98;

                                byte[] data3 = new byte[addHead.Length + sendbytes.Length];
                                System.Array.Copy(addHead, 0, data3, 0, addHead.Length);
                                System.Array.Copy(sendbytes, 0, data3, addHead.Length, sendbytes.Length);

                                try
                                {
                                    if ((ns.CanWrite)&&(ns !=null))
                                    {
                                      ns.Write(data3, 0, data3.Length);
                                      ShowMessageforView(lstbxMessageView2, sendjsStr);
//                                      string str = System.Text.Encoding.Default.GetString(data3);
//                                      ShowMessageforView(lstbxMessageView2, str);
                                      sw.Write(sendjsStr);
                                      sw.Write("\r\n");
                                     }
                                     else
                                    {
                                        ShowMessageforView(lstbxMessageView2, "不能写入数据流,请重启信号机");
 //                                        MessageBox.Show("不能写入数据流", "终止");
                                    }
                                 }
                                 catch (Exception se)
                                 {
 //                                       MessageBox.Show(se.Message + Environment.NewLine);
 //                                    MessageBox.Show("信号机不支持电警数据转发功能","警告");
                                     ShowMessageforView(lstbxMessageView2, "信号机不支持电警数据转发功能");

                                  }
                                   
                            }
                            jsonPackQ.Pop();
                          }                           
                        }
                    System.Threading.Thread.Sleep(sleepMillsSec);
                }
        }
        string jsonhh;
        int sleepMillsSec;
        MyCircleQueue<JsonPack> jsonPackQ = new MyCircleQueue<JsonPack>(25);
        public void ThreadPackJsonToQueue()
        {

            while (true)
            {
                configInfo config = new configInfo();
                config.arrivalTime = DateTime.Now.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss");
                config.throughTime = DateTime.Now.ToLocalTime().AddSeconds(1).ToString("yyyy-MM-ddTHH:mm:ss");
                config.sendTime = DateTime.Now.ToLocalTime().AddSeconds(11).ToString("yyyy-MM-ddTHH:mm:ss"); 

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
                            config.port_Cof = tmpEPport;
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
      

 /*   
        private void SendJsonMessage(object obj)
        {

            IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpPLip, tmpPLport);
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
                
//                System.Threading.Thread.Sleep(11000);
            }
        }
*/
        // 发送消息方法
        private void SendMessage(object obj)
        {
           byte[] sendbytesQ = new byte[8]{0x6e,0x6e,0,0,0x9e,0,0,0};
//           tmpSCport = 31662;
         IPEndPoint remoteIpEndPoint = new IPEndPoint(tmpSCip, tmpSCport);

            while (true)
            {
                try
                {
                System.Threading.Thread.Sleep(1000);
                sendUdpClient.Send(sendbytesQ, sendbytesQ.Length, remoteIpEndPoint);
 //               sendUdpClient.Send(sendbytes, sendbytes.Length, "AlternateHostMachineName", 11000);

                    // 关闭receiveUdpClient时此时会产生异常
                    byte[] receiveBytes = sendUdpClient.Receive(ref remoteIpEndPoint);

                    // 显示消息内容
                    if (receiveBytes.Length != 388)
                    {
                        for (int i = 0; i < receiveBytes.Length; i++)
                        {
                            ShowMessageforView(lstbxMessageView, string.Format("{0}[0x{1:X2}]:{2}", remoteIpEndPoint, receiveBytes[i], i));

                        }
                    }
                    else
                    {
                        for (int i = 288; i < 320; i++)
                        {
                            ChannelStatus[i - 288] = receiveBytes[i];
                            ShowMessageforView(lstbxMessageView, string.Format("{0}:[{1}]_{2}", remoteIpEndPoint, (i - 287), ChannelStatus[i - 288]));

                        }
                    }
                }
                catch (Exception se)
                {
//                    MessageBox.Show(se.Message + " Conn......." + Environment.NewLine);
                    sendUdpClient.Close();
                    sendUdpClient = new UdpClient();
                    ShowMessageforView(lstbxMessageView2, "已自动重连信号机");
//                    break;
                }
            }
        }
  

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
 //           this.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
            if (sendThread != null)
            { sendThread.Abort(); }
            if (sendUdpClient != null)
            { sendUdpClient.Close(); }
//            if (sendJsonThread != null)
//            { sendJsonThread.Abort(); }
            if (sendJsonUdpClient != null)
            { sendJsonUdpClient.Close(); }
            if ((tcpClient != null) && (tcpClient.Connected))
            {
                tcpClient = null;
                DisConnect();
                stopwatch.Stop();
//                ShowMessageforView(lstbxMessageView, string.Format("[ ^^^^^^^^^^^ 本次运行时间: {1} ^^^^^^^^^^^ ]", stopwatch.Elapsed.TotalMinutes));
//                MessageBox.Show("[本次运行时间: " + stopwatch.Elapsed.TotalMinutes + "mins]");
                ShowMessageforView(lstbxMessageView2, "[本次运行时间: " + stopwatch.Elapsed.TotalMinutes + "mins]");
            }
           

            try
            {
                tx.Abort();
                packjson.Abort();
                toSC.Abort();
            }
            catch (Exception se)
            {
//                MessageBox.Show(se.Message+ Environment.NewLine);
                ShowMessageforView(lstbxMessageView2, se.Message + Environment.NewLine);
            }


                IsUdpcRecvStart = false;
//               MessageBox.Show("模拟电警已停止");
                ShowMessageforView(lstbxMessageView2, "模拟电警已停止");

                Start.IsEnabled = false;

        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
//            MessageBox.Show((this.lane1Combox.SelectedItem as comboDir).ID.ToString());
 
            if (this.radiobutton1.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(textbox1.Text))
                {
   //                 MessageBox.Show("不得为空");
                    ShowMessageforView(lstbxMessageView2, "不得为空");
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
  //                          MessageBox.Show("通道号不得大于32");
                            ShowMessageforView(lstbxMessageView2, "通道号不得大于32"); 
                            textbox1.Background = Brushes.Coral;
                            textbox1.Focus();
                        }
                        else
                        {
                            textbox1.Background = Brushes.White;
                            ChannelConfig[0] = Byte.Parse(textbox1.Text);
                            dirConfig[0] = (this.lane1Combox.SelectedItem as comboDir).ID.ToString();
                            dir_C[0] = (int)(this.lane1Combox.SelectedItem as comboDir).ID;
                            Start.IsEnabled = true;
                        }
                    }
                }
            }
            else
            {
  //              MessageBox.Show("选择相对的记录操作");
                ShowMessageforView(lstbxMessageView2, "选择相对的记录操作"); 
            }
            if (grid12.Visibility == Visibility.Visible)
            {
                if (this.radiobutton2.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textbox2.Text))
                    {
                      //  MessageBox.Show("不得为空");
                        ShowMessageforView(lstbxMessageView2, "不得为空"); 
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
                      //          MessageBox.Show("通道号不得大于32");
                                ShowMessageforView(lstbxMessageView2, "通道号不得大于32");
                                textbox2.Background = Brushes.Coral;
                                textbox2.Focus();
                            }
                            else
                            {
                                textbox2.Background = Brushes.White;
                                ChannelConfig[1] = Byte.Parse(textbox2.Text);
                                dirConfig[1] = (this.lane2Combox.SelectedItem as comboDir).ID.ToString();
                                dir_C[1] = (int)(this.lane2Combox.SelectedItem as comboDir).ID;
                                Start.IsEnabled = true;
                            }
                        }                       
                    }
                }
                else
                {
//                    MessageBox.Show("选择相对的记录操作");
                }
            }
            if (grid13.Visibility == Visibility.Visible)
            {
                if (this.radiobutton3.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textbox3.Text))
                    {
          //              MessageBox.Show("不得为空");
                        ShowMessageforView(lstbxMessageView2, "不得为空");
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
          //                      MessageBox.Show("通道号不得大于32");
                                ShowMessageforView(lstbxMessageView2, "通道号不得大于32");
                                textbox3.Background = Brushes.Coral;
                                textbox3.Focus();
                            }
                            else
                            {
                                textbox3.Background = Brushes.White;
                                ChannelConfig[2] = Byte.Parse(textbox3.Text);
                                dirConfig[2] = (this.lane3Combox.SelectedItem as comboDir).ID.ToString();
                                dir_C[2] = (int)(this.lane3Combox.SelectedItem as comboDir).ID;
                                Start.IsEnabled = true;
                            }
                        }
                    }
                }
                else
                {
//                    MessageBox.Show("选择相对的记录操作13");
                }
            }
            if (grid14.Visibility == Visibility.Visible)
            {
                if (this.radiobutton4.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textbox4.Text))
                    {
                 //       MessageBox.Show("不得为空");
                        ShowMessageforView(lstbxMessageView2, "不得为空");
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
          //                      MessageBox.Show("通道号不得大于32");
                                ShowMessageforView(lstbxMessageView2, "通道号不得大于32");
                                textbox4.Background = Brushes.Coral;
                                textbox4.Focus();
                            }
                            else
                            {
                                textbox4.Background = Brushes.White;
                                ChannelConfig[3] = Byte.Parse(textbox4.Text);
                                dirConfig[3] = (this.lane4Combox.SelectedItem as comboDir).ID.ToString();
                                dir_C[3] = (int)(this.lane4Combox.SelectedItem as comboDir).ID;
                                Start.IsEnabled = true;
                            }
                        }
                    }
                }
                else
                {
//                    MessageBox.Show("选择相对的记录操作14");
                }
            }           
        }

        private void textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textbox1.Text))
            {
      //          MessageBox.Show("不得为空");
                ShowMessageforView(lstbxMessageView2, "不得为空");
                textbox1.Focus();
            }
        }

        int p = 1;
        private void btnAddd_Click(object sender, RoutedEventArgs e)
        {
            this.radiobutton1.IsChecked = true;
            if (p > 6)
            {
               p = 1;
            }
            p++;

            if (p == 2)
            { 
                grid12.Visibility = Visibility.Visible;
                this.radiobutton2.IsChecked = true;
            }
            if (p == 3)
            { grid13.Visibility = Visibility.Visible;
            this.radiobutton3.IsChecked = true;
            }
            if (p == 4)
            { grid14.Visibility = Visibility.Visible;
            this.radiobutton4.IsChecked = true;
            }

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
              if(radiobutton1.IsChecked == true)
              {
   //               MessageBox.Show("至少配置1项");
                  ShowMessageforView(lstbxMessageView2, "至少配置1项");

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
            Array.Clear(ChannelConfig, 0, ChannelConfig.Length);
            Array.Clear(dirConfig, 0, dirConfig.Length);
            Array.Clear(dir_C, 0, dir_C.Length);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lstbxMessageView.Items.Clear();
        }

            protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

            private void Button_Click(object sender, RoutedEventArgs e)
            {
                lstbxMessageView2.Items.Clear();
            }

            private void Button_Click_1(object sender, RoutedEventArgs e)
            {
                string CopyText = "";
                for (int i = 0; i < lstbxMessageView2.SelectedItems.Count; i++)
                {

                    CopyText = CopyText + Environment.NewLine + lstbxMessageView2.SelectedItems[i].ToString();
                }
                try
                { Clipboard.SetText(CopyText); }
                catch (Exception se)
                {
                    //MessageBox.Show(se.Message  + Environment.NewLine);
                    ShowMessageforView(lstbxMessageView2, se.Message + Environment.NewLine);

                }

            }



    }
}


