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
        private UdpClient sendUdpClient;
        private UdpClient receiveUpdClient;
        private BackgroundWorker bgWorker = new BackgroundWorker();
        private int i = 0;
        public  String tmpSCip;
        public String tmpSCport;

        public MainWindow()
        {
            InitializeComponent();
            IPAddress ips = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First();



            //支持报告进度更新
            bgWorker.WorkerReportsProgress = true;
            //支持异步取消
            bgWorker.WorkerSupportsCancellation = true;
            //将DoWork_Handler绑定在RunWorkerAsync()
            bgWorker.DoWork += DoWork_Handler;
            //将ProgressChanged_Handler绑定在ReportProgress()
            bgWorker.ProgressChanged += ProgressChanged_Handler;
            //退出时发生
            bgWorker.RunWorkerCompleted += RunWorkerCompleted_Handler;
/*
            btnSkin.Click += (s, e) => skinUI.IsOpen = true;
            skinPanel.AddHandler(Button.ClickEvent, new RoutedEventHandler(ChangeSkin));
            InitSkins();
*/        }
/// <summary>  
/// 初始化所有皮肤控件  
/// </summary>  
private void InitSkins()
{
    var accents = ThemeManager.Accents;
    Style btnStyle = App.Current.FindResource("btnSkinStyle") as Style;
    foreach (var accent in accents)
    {
        //新建换肤按钮  
        Button btnskin = new Button();
        btnskin.Style = btnStyle;
        btnskin.Name = accent.Name;
        SolidColorBrush scb = accent.Resources["AccentColorBrush"] as SolidColorBrush;
        btnskin.Background = scb;
        skinPanel.Children.Add(btnskin);
    }
}
/// <summary>  
/// 实现换肤  
/// </summary>  
private void ChangeSkin(object obj, RoutedEventArgs e)
{
    if (e.OriginalSource is Button)
    {
        Accent accent = ThemeManager.GetAccent((e.OriginalSource as Button).Name);
        App.Current.Resources.MergedDictionaries.Last().Source = accent.Resources.Source;
    }
}  


        int judge = 0;   //0表示编辑状态，1为添加状态。因为后面的增加和编辑都在同一个事件中，所以建一个变量来区分操作  
        //     TB_Information tbInfo = new TB_Information();    //这个类可以供我调用里面的方法来进行增删改查的操作  
    /*
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            judge = 1;  //现在为添加状态       
            dataGrid.CanUserAddRows = true;    //点击添加后  将CanUserAddRows重新设置为True，这样DataGrid就会自动生成新行，我们就能在新行中输入数据了。  
        }
      */  
        //现在我们可以添加新记录了，我们接下来要做的就是获取这些新添加的记录  

        //先声明一个存储新建记录集的List<T>      这里的Information是我的数据表实体类  里面包含FID ，车道号,方向,通道号  

        List<Information> lstInformation = new List<Information>();

        //我们通过 RowEditEnding来获取新增的记录，就是每次编辑完行后，行失去焦点激发该事件。   更新记录也是执行该事件  
        public class Information
        {
            public int laneNub;
            public string direction;
            public int channelNub;
            public Information() { }

        };
        private void dataGrid1_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Information info = new Information();   //我自己的数据表实例类  
            info = e.Row.Item as Information;        //获取该行的记录  
            if (judge == 1)                                          //如果是添加状态就保存该行的值到lstInformation中  这样我们就完成了新行值的获取  
            {
                lstInformation.Add(info);
            }
            else
            {
                //              tbInfo.UpdInformation(info);            //如果是编辑状态就执行更新操作  更新操作最简单，因为你直接可以在DataGrid里面进行编辑，编辑完成后执行这个事件就完成更新操作了  
            }
        }
        int intervalTime = 1;
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
                int sa = int.Parse(sendA.Text);
                int sb = int.Parse(sendB.Text);
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

            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync();
            }
/* 
            //IPv6
            IPAddress[] ips = Dns.GetHostAddresses("");
            EPip.Text = ips[3].ToString();
            SCip.Text = ips[3].ToString();
*/
            //IPv4
            IPAddress ips = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First();
/*
            EPip.Text = ips.ToString();
            SCip.Text = ips.ToString();
            PLip.Text = ips.ToString();
*/


            // 创建接收套接字
            IPAddress localIp = IPAddress.Parse(ips.ToString());
 //           IPEndPoint localIpEndPoint = new IPEndPoint(localIp, int.Parse(PLport.Text));
            IPEndPoint localIpEndPoint = new IPEndPoint(localIp,0);
            receiveUpdClient = new UdpClient(localIpEndPoint);

            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();


            Thread t = new Thread(new ThreadStart(ThreadGetStatus));
            t.Start();

        }
        // 接收消息方法
        private void ReceiveMessage()
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    // 关闭receiveUdpClient时此时会产生异常
                    byte[] receiveBytes = receiveUpdClient.Receive(ref remoteIpEndPoint);

                    //                   string message = System.Text.Encoding.Unicode.GetString(receiveBytes);

                    // 显示消息内容
                    //                   ShowMessageforView(lstbxMessageView, string.Format("{0}[{1}]", remoteIpEndPoint, message));
                    for (int i = 288; i < 320; i++)
                    {
                        ShowMessageforView(lstbxMessageView, string.Format("{0}[{1}]", remoteIpEndPoint, receiveBytes[i]));

                    }
                }
                catch
                {
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


        public void ThreadGetStatus()
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,(ThreadStart)delegate()
            {
                //线程action代码...待添加
                MessageBox.Show("我帅不帅?.!", "消息");
                //匿名模式
                sendUdpClient = new UdpClient(0);
                // 实名模式(套接字绑定到本地指定的端口)
/*                IPAddress localIp = IPAddress.Parse(EPip.Text);
                IPEndPoint localIpEndPoint = new IPEndPoint(localIp, int.Parse(EPport.Text));
                sendUdpClient = new UdpClient(localIpEndPoint);
*/
                Thread sendThread = new Thread(SendMessage);
                sendThread.Start();
            });
        }

        // 发送消息方法
        private void SendMessage(object obj)
        {
//           string message = (string)obj;
//           message = Data_Hex_Asc(ref message);
//          byte[] sendbytes = System.Text.Encoding.Unicode.GetBytes(message);
            byte[] sendbytes = new byte[]{0x6e,0x6e,0x0,0x0,0x9e,0x0,0x0,0x0};

            tmpSCip = SCip.ToString();
            tmpSCport = SCport.ToString();

            IPAddress remoteIp = IPAddress.Parse(tmpSCip);
            IPEndPoint remoteIpEndPoint = new IPEndPoint(remoteIp, int.Parse(tmpSCport));
            int i = 0;
            while (i<5)
            {
                System.Threading.Thread.Sleep(1);
                sendUdpClient.Send(sendbytes, sendbytes.Length, remoteIpEndPoint);
                i++;
            }
            //sendUdpClient.Close();

            // 清空发送消息框
 //           ResetMessageText(tbxMessageSend);
        }
  

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            bgWorker.CancelAsync();
 //           this.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
            sendUdpClient.Close();
            receiveUpdClient.Close();

        }


        /// <summary>
        /// 进度更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ProgressChanged_Handler(object sender, ProgressChangedEventArgs args)
        {
 //           lbDisplay.Items.Add("第" + i + "次测试");
        }

        /// <summary>
        /// 后台线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DoWork_Handler(object sender, DoWorkEventArgs args)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker.CancellationPending)
                {
                    args.Cancel = true;
                    break;
                }
                else
                {
                    i++;
                    bgWorker.ReportProgress(0);
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 执行完成或正常退出后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void RunWorkerCompleted_Handler(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Cancelled)
            {
                MessageBox.Show("后台任务已经被取消。", "消息");
            }
            else
            {
                MessageBox.Show("后台任务正常结束。", "消息");
            }
        }


        private void TextBox_SCIPChanged(object sender, TextChangedEventArgs e)
        {
//            tbxSendtoIp.Text =
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
                            }
                        }

                    }
                }
                else
                {
                    MessageBox.Show("选择相对的记录操作");
                }
            } 
            if(radiobutton1.IsChecked == true)
            {
                Information info = new Information();   //我自己的数据表实例类  
                info.channelNub = 1;
           //     info.direction = CB11.


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

    
    }
}


