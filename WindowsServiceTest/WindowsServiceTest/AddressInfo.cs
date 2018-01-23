using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsServiceTest
{
    public class AddressInfo
    {
        public string SERIAL_NO { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string addres { get; set; }

        /// <summary>
        /// 接报时间
        /// </summary>
        public DateTime jiebao_time { get; set; }
        /// <summary>
        /// 反应类别
        /// </summary>
        public string fy_lb { get; set; }

        /// <summary>
        /// X轴
        /// </summary>
        public string GISX { get; set; }
        /// <summary>
        /// Y轴
        /// </summary>
        public string GISY { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string AState { get; set; }

      
    }
}
