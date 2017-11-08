using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplicationMD
{
    public enum Dir { TL=0,SL=1,TS=2,TR=3,SR=4};
    public struct configInfo
    {
        public String ipV4_Cof;
        public Dir dirFlag;
        public String laneNo_Cof;
        public String port_Cof;
        public String Channel_Cof;
        public String arrivalTime;
        public String throughTime;
        public String sendTime;
    };



    class JsonPack
    {
        public String time ;/*= DateTime.Now.ToLocalTime().AddSeconds(11).ToString();*/
        public String ipV4;
        public String LaneVehicleDir;
        public String arrivalStopLineTime ;/*= DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss");*/
        public String throughStopLineTime ;/*= DateTime.Now.ToLocalTime().AddSeconds(1).ToString(); */
        public String sendSnapDataTime ;/* = DateTime.Now.ToLocalTime().AddSeconds(11).ToString(); */
        public String laneNo;
        public String port;
        public String Channel;

        public JsonPack() { }
        public JsonPack(configInfo conLane)
        {
            arrivalStopLineTime = conLane.arrivalTime;
            throughStopLineTime = conLane.throughTime;
            sendSnapDataTime = conLane.sendTime;
            time = sendSnapDataTime;
            laneNo = conLane.laneNo_Cof;
            Channel = conLane.Channel_Cof; 
            port = conLane.port_Cof;
            ipV4 = conLane.ipV4_Cof;
            switch (conLane.dirFlag)
            {
                case Dir.TL:
                    LaneVehicleDir = "Left";
                    break;

                case Dir.SL:
                    if (MainWindow.randomNm(0, 2) == 0)
                    {
                        LaneVehicleDir = "Straight";
                    }
                    else
                    {
                        LaneVehicleDir = "Left";
                    }
                    break;

                case Dir.TS:
                    LaneVehicleDir = "Straight";
                    break;

                case Dir.TR:
                    LaneVehicleDir = "Right";
                    break;

                case Dir.SR:

                    if (MainWindow.randomNm(0, 2) == 0)
                    {
                        LaneVehicleDir = "Straight";
                    }
                    else
                    {
                        LaneVehicleDir = "Right";
                    }
                    break;
            }
        }

        public string ClassToJson()
        {

            var sb = new StringBuilder();
            sb.Append("{");
            if (this != null)
            {
 //                   sb.AppendFormat("\"time\":\"{0}\",\"ipV4\":\"{1}\",", this.time, this.ipV4);
                    sb.Append("\"time\":\"" + this.time + ":696Z\",\"ipV4\":\"" + this.ipV4 + "\",");
                    sb.Append("\"ipV6\":\"::\",\"port\":"+this.port+",\"macAddress\":\"54:C4:15:45:43:3A\",\"channel\":1,\"Target\":");
                    sb.Append("[");
                        sb.Append("{");
                            sb.Append("\"recognitionType\":\"vehicle\",\"TargetInfo\":{\"recognition\":\"plate\",\"Region\":\"[]\",");
                            sb.Append("\"Property\":[{\"description\":\"plate\",\"value\":\"蓝浙CA0186\"},");
                            sb.Append("{\"description\":\"confidence\",\"value\":80},");
                            sb.Append("{\"description\":\"characterConfidence\",\"value\":\"71 95 79 99 73 88 54\"}]}");
                        sb.Append("},");
                        sb.Append("{");
                        sb.Append("\"recognitionType\":\"vehicle\",\"TargetInfo\":{\"recognition\":\"vehicle\",\"Region\":\"[]\",");
                        sb.Append("\"Property\":[");
                        sb.Append("{\"description\":\"LaneVehicleDir\",\"value\":\"" + this.LaneVehicleDir + "\"},");
//                      sb.AppendFormat("{\"description\":\"LaneVehicleDir\",\"value\":\"{0}\"},", this.LaneVehicleDir);
                        sb.Append("{\"description\":\"vehicle_category\",\"value\":\"vehicle\"},");
                        sb.Append("{\"description\":\"arrivalStopLineTime\",\"value\":\"" + this.arrivalStopLineTime + ":335Z\"},");
                        sb.Append("{\"description\":\"throughStopLineTime\",\"value\":\"" + this.throughStopLineTime + ":435Z\"},");
                        sb.Append("{\"description\":\"sendSnapDataTime\",\"value\":\"" + this.sendSnapDataTime + ":696Z\"},");

         //               sb.AppendFormat("{\"description\":\"arrivalStopLineTime\",\"value\":\"{0}\"},", this.arrivalStopLineTime);
         //               sb.AppendFormat("{\"description\":\"throughStopLineTime\",\"value\":\"{0}\"},", this.throughStopLineTime);
        //                sb.AppendFormat("{\"description\":\"sendSnapDataTime\",\"value\":\"{0}\"},", this.sendSnapDataTime);
                        sb.Append("{\"description\":\"monitorInfo\",\"value\":\"\"},");
                        sb.Append("{\"description\":\"laneNo\",\"value\":" + this.laneNo + "}");
       //                 sb.AppendFormat("{\"description\":\"laneNo\",\"value\":\"{0}\"}", this.laneNo);
                    sb.Append("]}}]");
//                    sb.Append("]");

 //               if (sb.Length > 1)
 //                   sb.Remove(sb.Length - 1, 1);

            }
            sb.Append("}");
            sb.Replace("\\","");
            string ha = sb.ToString();
           return ha;
        }
    }
    
    /// <summary> 
    /// 循环队列 
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public class MyCircleQueue<T>
    {
        /// <summary> 
        /// 队列数组 
        /// </summary> 
        private T[] _queue;
        /// <summary> 
        /// 队首索引 
        /// </summary> 
        private int _front;
        /// <summary> 
        /// 队尾索引 
        /// </summary> 
        private int _rear;

        /// <summary> 
        /// 队列的内存大小，但实际可用大小为_capacity-1 
        /// </summary> 
        private int _capacity;

        public MyCircleQueue(int queueSize)
        {
            if (queueSize < 1)
                throw new IndexOutOfRangeException("传入的队列长度不能小于1。");

            //设置队列容量 
            _capacity = queueSize;

            //创建队列数组 
            _queue = new T[queueSize];

            //初始化队首和队尾索引 
            _front = _rear = 0;
        }

        /// <summary> 
        /// 添加一个元素 
        /// </summary> 
        /// <param name="item"></param> 
        public void Push(T item)
        {
            //队列已满 
            if (GetNextRearIndex() == _front)
            {
                //扩大数组 
                T[] newQueue = new T[2 * _capacity];

                if (newQueue == null)
                    throw new ArgumentOutOfRangeException("数据容量过大，超出系统内存大小。");
                //队列索引尚未回绕 
                if (_front == 0)
                {
                    //将旧队列数组数据转移到新队列数组中 
                    Array.Copy(_queue, newQueue, _capacity);
                }
                else
                {
                    //如果队列回绕，刚需拷贝再次， 
                    //第一次将队首至旧队列数组最大长度的数据拷贝到新队列数组中 
                    Array.Copy(_queue, _front, newQueue, _front, _capacity - _rear - 1);
                    //第二次将旧队列数组起始位置至队尾的数据拷贝到新队列数组中 
                    Array.Copy(_queue, 0, newQueue, _capacity, _rear + 1);
                    //将队尾索引改为新队列数组的索引 
                    _rear = _capacity + 1;
                }

                _queue = newQueue;
                _capacity *= 2;
            }

            //累加队尾索引，并添加当前项 
            _rear = GetNextRearIndex();
            _queue[_rear] = item;
        }

        /// <summary> 
        /// 获取队首元素 
        /// </summary> 
        /// <returns></returns> 
        public T FrontItem()
        {
            if (IsEmpty())
                throw new ArgumentOutOfRangeException("队列为空。");

            return _queue[GetNextFrontIndex()];
        }

        /// <summary> 
        /// 获取队尾元素 
        /// </summary> 
        /// <returns></returns> 
        public T RearItem()
        {
            if (IsEmpty())
                throw new ArgumentOutOfRangeException("队列为空。");

            return _queue[_rear];
        }

        /// <summary> 
        /// 弹出一个元素 
        /// </summary> 
        /// <returns></returns> 
        public T Pop()
        {
            if (IsEmpty())
                throw new ArgumentOutOfRangeException("队列为空。");

            _front = GetNextFrontIndex();
            return _queue[_front];
        }

        /// <summary> 
        /// 队列是否为空 
        /// </summary> 
        /// <returns></returns> 
        public bool IsEmpty()
        {
            return _front == _rear;
        }
        /// <summary> 
        /// 获取下一个索引 
        /// </summary> 
        /// <returns></returns> 
        private int GetNextRearIndex()
        {
            if (_rear + 1 == _capacity)
            {
                return 0;
            }
            return _rear + 1;
        }

        /// <summary> 
        /// 获取下一个索引 
        /// </summary> 
        /// <returns></returns> 
        private int GetNextFrontIndex()
        {
            if (_front + 1 == _capacity)
            {
                return 0;
            }
            return _front + 1;
        }
    } 
}

