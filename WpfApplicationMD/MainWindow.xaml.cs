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


namespace WpfApplicationMD
{

        

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

public enum Direction { 左转直行,左转,直行,右转,右转直行 };
    public partial class MainWindow : Window
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
            /* 
            //IPv6
            IPAddress[] ips = Dns.GetHostAddresses("");
            EPip.Text = ips[3].ToString();
            SCip.Text = ips[3].ToString();
            */ 
            //IPv4
             IPAddress ips = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First();
             EPip.Text = ips.ToString();
             SCip.Text = ips.ToString();
             PLip.Text = ips.ToString();

            tmpSCip = SCip.Text;
            tmpSCport = SCport.Text;
            //tmpSCport = "41883";
            int port = 61883;
             PLport.Text = port.ToString();

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
        }


        int judge = 0;   //0表示编辑状态，1为添加状态。因为后面的增加和编辑都在同一个事件中，所以建一个变量来区分操作  
        //     TB_Information tbInfo = new TB_Information();    //这个类可以供我调用里面的方法来进行增删改查的操作  
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            judge = 1;  //现在为添加状态       
            dataGrid.CanUserAddRows = true;    //点击添加后  将CanUserAddRows重新设置为True，这样DataGrid就会自动生成新行，我们就能在新行中输入数据了。  
        }
        
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

        //获取到记录后，单击保存按钮就可以保存lstInformation中的每一条记录  
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            foreach (Information info in lstInformation)
            {
                //              tbInfo.InsInformation(info);      //执行插入方法，将记录保存到数据库  
            }
            judge = 0;          //重新回到编辑状态  
            lstInformation.Clear();
            dataGrid.CanUserAddRows = false;     //因为完成了添加操作 所以设置DataGrid不能自动生成新行了  
                                                 //          Binding(Num, 1);
        }


        //由ChecBox的Click事件来记录被选中行的FID  

        List<int> selectFID = new List<int>();  //保存选中要删除行的FID值  

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox dg = sender as CheckBox;
            int FID = int.Parse(dg.Tag.ToString());   //获取该行的FID  
            var bl = dg.IsChecked;
            if (bl == true)
            {
                selectFID.Add(FID);         //如果选中就保存FID  
            }
            else
            {
                selectFID.Remove(FID);  //如果选中取消就删除里面的FID  
            }
        }



        //已经获取到里面的值了，接下来就只要完成删除操作就可以了  删除事件如下  

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (int FID in selectFID)
            {
                //              tbInfo.DelInformation(FID);   //循环遍历删除里面的记录  
            }
            //Binding(Num, 1);       //这个是我绑定的一个方法，作用是删除记录后重新给DataGrid赋新的数据源  
        }
        private void dataGrid1_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count == 0)
                return;
            var currentCell = e.AddedCells[0];
            if (currentCell.Column == dataGrid.Columns[3])   //Columns[]从0开始  我这的ComboBox在第四列  所以为3  
            {
                dataGrid.BeginEdit();    //  进入编辑模式  这样单击一次就可以选择ComboBox里面的值了  
            }
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {




            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync();
            }


            // 创建接收套接字
            IPAddress localIp = IPAddress.Parse(PLip.Text);
            IPEndPoint localIpEndPoint = new IPEndPoint(localIp, int.Parse(PLport.Text));
            receiveUpdClient = new UdpClient(localIpEndPoint);

            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();


            Thread t = new Thread(new ThreadStart(ThreadSearch));
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

                    string message = System.Text.Encoding.Unicode.GetString(receiveBytes);

                    // 显示消息内容
                    ShowMessageforView(lstbxMessageView, string.Format("{0}[{1}]", remoteIpEndPoint, message));
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


        public void ThreadSearch()
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,(ThreadStart)delegate()
            {
                //线程action代码...待添加
                MessageBox.Show("我帅不帅?.!", "消息");
                //匿名模式
                //sendUdpClient = new UdpClient(0);
                // 实名模式(套接字绑定到本地指定的端口)
                IPAddress localIp = IPAddress.Parse(EPip.Text);
                IPEndPoint localIpEndPoint = new IPEndPoint(localIp, int.Parse(EPport.Text));
                sendUdpClient = new UdpClient(localIpEndPoint);

                Thread sendThread = new Thread(SendMessage);
                sendThread.Start(tbxMessageSend.Text);
            });
        }

        //十六进制转换为字符串
        public string Data_Hex_Asc(ref string Data)
        {


            string Data1 = "";


            string sData = "";


            while (Data.Length > 0)


            //first take two hex value using substring.


            //then convert Hex value into ascii.


            //then convert ascii value into character.
            {

                Data1 = System.Convert.ToChar(System.Convert.ToUInt32(Data.Substring(0, 2), 16)).ToString();


                sData = sData + Data1;


                Data = Data.Substring(2, Data.Length - 2);
            }
            return sData;
        }

        public string Data_Asc_Hex(ref string Data)
        {

            //first take each charcter using substring.

            //then convert character into ascii.

            //then convert ascii value into Hex Format

            string sValue;

            string sHex = "";

            while (Data.Length > 0)
            {

                sValue = Microsoft.VisualBasic.Conversion.Hex(Strings.Asc(Data.Substring(0, 1).ToString()));

                Data = Data.Substring(1, Data.Length - 1);

                sHex = sHex + sValue;

            }

            return sHex;
        }
        // 发送消息方法
        private void SendMessage(object obj)
        {
            string message = (string)obj;
            message = Data_Hex_Asc(ref message);

            byte[] sendbytes = System.Text.Encoding.Unicode.GetBytes(message);
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


    }
}


