using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplicationMD
{
  /*  class JsonPack
    {
        public String time;
        public String ipV4;
        public String LaneVehicleDir;
        public String arrivalStopLineTime;
        public String throughStopLineTime;
        public String sendSnapDataTime;
        public String laneNo;

        public string ClassToJson(List<JsonPack> Class)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            if (Class != null)
            {

                foreach (var item in Class)
                {
                    sb.Append("{");
                    sb.AppendFormat("\"time\":\"{0}\",\"ipV4\":\"{1}\"", item.time, item.ipV4);
                    sb.Append("}");
                    sb.Append(",");
                }
                if (sb.Length > 1)
                    sb.Remove(sb.Length - 1, 1);

            }
            sb.Append("]");
            return sb.ToString();
        }
    }*/
}

