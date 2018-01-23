using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using Quartz;

namespace WindowsServiceTest
{
    public class GISConvert : IJob
    {
        //日志
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //sql连接字符串
        private static readonly string connString = ConfigurationManager.ConnectionStrings["conn_gis"].ConnectionString;
        private int unreturn = 0;    //用于输出到日志，转换失败条数 
        private string unNO = "";    //用于输出到日志，转换失败的ID
        public void Execute(IJobExecutionContext context)
        {
            string sName = context.Trigger.Key.Name;
            logger.Debug("开始GIS坐标转换");
            UpdateGISXY();
            logger.Debug("GIS坐标转换结束");
        }


        public void UpdateGISXY()
        {
            List<AddressInfo> addressList = new List<AddressInfo>();
            //AddressInfo addressinfo;
            //查询语句，一次获取前一百条数据。
            string sql = "select top 100 * from GisConvert where State = 0";     //根据自己需求设置设计数据表，我是用State=0表示未转换的
            DataTable addressDt = SqlHelper.ExecuteDataTable(connString, CommandType.Text, sql);
            string sn = "";  //   转换成功的ID
            int kfnum = 0;
            for (int i = 0; i < addressDt.Rows.Count; i++)
            {
                sn = sn + "'" + addressDt.Rows[i]["ID"].ToString() + "',";         //主键为ID
                kfnum = i + 1;
                try
                {
                    string XY, X;
                    //ak是百度地图appkey，需要自己申请
                    XY = Post("http://api.map.baidu.com/geocoder/v2/?ak=eIxDStjzbtH0WtU50gqdXYCz&output=json",  addressDt.Rows[i]["Address"].ToString());
                    X = XY.Split(new char[] { '{', '}', ':', '"' }, StringSplitOptions.RemoveEmptyEntries)[5];

                    //更新表的state和经纬度
                    string SQLString = "update GisConvert set State=@State ,Longitude=@Longitude,Latitude=@Latitude where ID =@ID";
                    SqlParameter[] parameters = {
                    new SqlParameter("@ID", SqlDbType.Int),
                    new SqlParameter("@State", SqlDbType.Int,4),         
                    new SqlParameter("@Longitude", SqlDbType.VarChar,50),
                    new SqlParameter("@Latitude", SqlDbType.VarChar,50)
                                                };
                    parameters[0].Value = addressDt.Rows[i]["ID"].ToString();
                    if (X.Substring(0, X.Length - 1) == "")   //当数据库的地址不规范，无法在地图中找到该坐标时state=999
                    {
                        parameters[1].Value = 999;
                        unreturn++;
                        unNO = unNO + addressDt.Rows[i]["ID"].ToString() + ",";
                    }
                    else
                    {
                        parameters[1].Value = 1;             //转换成功后state=1
                    }
                    parameters[2].Value = X.Substring(0, X.Length - 1);
                    parameters[3].Value = XY.Split(new char[] { '{', '}', ':', '"' }, StringSplitOptions.RemoveEmptyEntries)[7];
                    using (SqlConnection connection = new SqlConnection(connString))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            PrepareCommand(cmd, connection, null, SQLString, parameters);
                            object obj = cmd.ExecuteScalar();
                            cmd.Parameters.Clear();
                            logger.Info("GIS坐标转换成功，ID：" + addressDt.Rows[i]["ID"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    unreturn++;
                    unNO = unNO + addressDt.Rows[i]["ID"].ToString() + ",";
                    logger.Debug("GIS坐标转换失败，ID：" + addressDt.Rows[i]["ID"].ToString() + "，错误信息：" + ex.Message);
                    
                }
            }
            //坐标转换
            logger.Debug("已转换ID（" + sn + ")，共计" + kfnum + "条...");
            if (unreturn>0)
            { 
            logger.Debug("因地址无法识别转换失败（" + unNO + ")，共计" + unreturn + "条...");
            }
        }


        public static string Post(string url, string addr)
        {
            //请求路径

            //定义request并设置request的路径
            WebRequest request = WebRequest.Create(url);

            //定义请求的方式
            request.Method = "POST";

            //初始化request参数
            string postData = "&address=";
            postData += addr;

            //设置参数的编码格式，解决中文乱码
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            //设置request的MIME类型及内容长度
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            //打开request字符流
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //定义response为前面的request响应
            WebResponse response = request.GetResponse();

            //获取相应的状态代码
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            //定义response字符流
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();//读取所有
            Console.WriteLine(responseFromServer);

            //关闭资源
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (SqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

    }
}
