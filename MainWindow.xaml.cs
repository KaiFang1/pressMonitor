using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.IO;
using System.Windows.Threading;

namespace PressMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 参数定义
        private DispatcherTimer SensorTimer; //传感器读写计时器
        private DispatcherTimer COMTimer;
        private SerialPort sensor1_SerialPort = new SerialPort(); //传感器1串口
        //获取可用串口名
        private string[] IsOpenSerialPortCount = null;
        public string[] SPCount = null;           //用来存储计算机串口名称数组
        public int comcount = 0;                  //用来存储计算机可用串口数目，初始化为0
        public bool flag = false;
        public string sensor1_com = null;
        #endregion

        private void Function_Loaded(object sender, RoutedEventArgs e)//打开窗口后进行的初始化操作
        {
            COMTimer = new DispatcherTimer();
            COMTimer.Tick += new EventHandler(ShowCurTimer);
            COMTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            COMTimer.Start();

        }
        public void sensor1_SerialPort_Init(string comstring)//传感器1串口初始化
        {
            if (sensor1_SerialPort != null)
            {
                if (sensor1_SerialPort.IsOpen)
                {
                    sensor1_SerialPort.Close();
                }
            }

            sensor1_SerialPort = new SerialPort();
            sensor1_SerialPort.PortName = comstring;
            sensor1_SerialPort.BaudRate = 9600;
            sensor1_SerialPort.Parity = Parity.None;
            sensor1_SerialPort.StopBits = StopBits.One;
            sensor1_SerialPort.Open();
            sensor1_SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);
        }

        public double presN = new double();
        public string presVolt="";
        public int presVoltDec;
        private void sensor1_DataReceived(object sender, SerialDataReceivedEventArgs e)//传感器1串口接收数据
        {
            byte[] bytes = new byte[7];          //声明一个临时数组存储当前来的串口数据
            sensor1_SerialPort.Read(bytes, 0, 7);  //读取串口内部缓冲区数据到buf数组
            sensor1_SerialPort.DiscardInBuffer();          //清空串口内部缓存
            //string presVolt = bytes[4].ToString();

            presVolt = bytes[4].ToString("X2");
            presVoltDec = Int32.Parse(presVolt, System.Globalization.NumberStyles.HexNumber);

            //presN = 5.0 / 1.65 * (1.65 - 5.0 / 4095 * presVoltDec);
            presN = (presVoltDec - 128) * 5.0 / 127 * 50.0 / 1.65;
        }


        public string[] CheckSerialPortCount()//获取可用串口名
        {
            IsOpenSerialPortCount = SerialPort.GetPortNames();
            return IsOpenSerialPortCount;
        }

        public bool SerialPortClose()//关闭窗口时执行
        {
            byte[] clearBytes = new byte[19] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            SendControlCMD(clearBytes);//避免不规范操作造成再开机时电机自启动

            if (sensor1_SerialPort != null)
            {
                sensor1_SerialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);

                sensor1_SerialPort.Close();
            }

            return true;
        }

        public void SendControlCMD(byte[] command)//串口写入字节命令
        {
            //01 03 00 00 00 01 84 0A
            //byte[] command = new byte[19];
            //command[0] = 0x01;//开始字符
            //command[1] = 0x03;//电机1 使能端
            //command[2] = 0x00;//电机1 方向
            //command[3] = 0x00;//电机1 转速高位
            //command[4] = 0x88;//电机1 转速低位（范围1800-16200）对应速度范围（0-2590r/min）
            //command[5] = 0x01;//电机2
            //command[6] = 0x01;//电机2
            //command[7] = 0x0A;//电机2
            //command[8] = 0x88;//电机2
            //command[9] = 0x01;//电机3
            //command[10] = 0x01;//电机3
            //command[11] = 0x08;//电机3
            //command[12] = 0x88;//电机3
            //command[13] = 0x01;//电机4
            //command[14] = 0x01;//电机4
            //command[15] = 0x08;//电机4
            //command[16] = 0x88;//电机4
            //command[17] = 0x0D;//结束字符
            //command[18] = 0x0A;
            sensor1_SerialPort.Write(command, 0, 8);
        }

        //public void WriteCMD()//串口写入字节命令
        //{
        //    //01 03 00 00 00 01 84 0A
        //    byte[] command = new byte[8];
        //    command[0] = 0x01;//#设备地址
        //    command[1] = 0x03;//#功能代码，读寄存器的值
        //    command[2] = 0x00;//电机1 方向
        //    command[3] = 0x00;//电机1 转速高位
        //    command[4] = 0x00;//从第AIn号口开始读数据
        //    command[5] = 0x01;//读几个口
        //    command[6] = 0x84;//CRC 校验的低 8 位
        //    command[7] = 0x0A;//CRC 校验的高 8 位
        //    //command[8] = 0x88;//电机2
        //    //command[9] = 0x01;//电机3
        //    //command[10] = 0x01;//电机3
        //    //command[11] = 0x08;//电机3
        //    //command[12] = 0x88;//电机3
        //    //command[13] = 0x01;//电机4
        //    //command[14] = 0x01;//电机4
        //    //command[15] = 0x08;//电机4
        //    //command[16] = 0x88;//电机4
        //    //command[17] = 0x0D;//结束字符
        //    //command[18] = 0x0A;
        //    sensor1_SerialPort.Write(command, 0, 8);
        //}
        public void ShowCurTimer(object sender, EventArgs e)//取当前时间的委托
        {
            string timeDateString = "";
            DateTime now = DateTime.Now;
            timeDateString = string.Format("{0}年{1}月{2}日 {3}:{4}:{5}",
                now.Year,
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));

            timeDateTextBlock.Text = timeDateString;

            ScanPorts();//扫描可用串口
        }

        public string[] aCheckSerialPortCount()//获取可用串口名
        {
            IsOpenSerialPortCount = SerialPort.GetPortNames();
            return IsOpenSerialPortCount;
        }

        public void ScanPorts()//扫描可用串口
        {
            SPCount = aCheckSerialPortCount();      //获得计算机可用串口名称数组

            ComboBoxItem tempComboBoxItem = new ComboBoxItem();

            if (comcount != SPCount.Length)            //SPCount.length其实就是可用串口的个数
            {
                //当可用串口计数器与实际可用串口个数不相符时
                //初始化下拉窗口并将flag初始化为false

                Sensor1_comboBox.Items.Clear();


                tempComboBoxItem = new ComboBoxItem();
                tempComboBoxItem.Content = "请选择串口";
                Sensor1_comboBox.Items.Add(tempComboBoxItem);
                Sensor1_comboBox.SelectedIndex = 0;

                sensor1_com = null;
                flag = false;

                comcount = SPCount.Length;     //将可用串口计数器与现在可用串口个数匹配
            }

            if (!flag)
            {
                if (SPCount.Length > 0)
                {
                    //有可用串口时执行
                    comcount = SPCount.Length;

                   

                    for (int i = 0; i < SPCount.Length; i++)
                    {
                        //分别将可用串口添加到各个下拉窗口中
                        string tempstr = "串口" + SPCount[i];

                        tempComboBoxItem = new ComboBoxItem();
                        tempComboBoxItem.Content = tempstr;
                        Sensor1_comboBox.Items.Add(tempComboBoxItem);

                    }

                    flag = true;

                }
                else
                {
                    comcount = 0;
                    
                }
            }
        }

        private void Sensor1_comboBox_DropDownClosed(object sender, EventArgs e)//传感器1串口下拉菜单收回时发生
        {
            ComboBoxItem item = Sensor1_comboBox.SelectedItem as ComboBoxItem; //下拉窗口当前选中的项赋给item
            string tempstr = item.Content.ToString();                        //将选中的项目转为字串存储在tempstr中

            for (int i = 0; i < SPCount.Length; i++)
            {
                if (tempstr == "串口" + SPCount[i])
                {
                    try
                    {
                        sensor1_com = SPCount[i];
                        sensor1_SerialPort_Init(SPCount[i]);
                    }
                    catch
                    {
                        
                    }

                }
            }
        }

        public void WriteCMD(object sender, EventArgs e)//取当前时间的委托
        {
            //01 03 00 00 00 01 84 0A
            byte[] command = new byte[8];
            command[0] = 0x01;//#设备地址
            command[1] = 0x03;//#功能代码，读寄存器的值
            command[2] = 0x00;//电机1 方向
            command[3] = 0x00;//电机1 转速高位
            command[4] = 0x00;//从第AIn号口开始读数据
            command[5] = 0x01;//读几个口
            command[6] = 0x84;//CRC 校验的低 8 位
            command[7] = 0x0A;//CRC 校验的高 8 位

            string returnStr = "";

            for (int i = 0; i < command.Length; i++)
            {
                returnStr += command[i].ToString("X2");
            }

            WritetextBox.Text = returnStr;

            sensor1_SerialPort.Write(command, 0, 8);
            presstextBox.Text = presN.ToString("F");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SensorTimer = new DispatcherTimer();
            SensorTimer.Tick += new EventHandler(WriteCMD);
            SensorTimer.Interval = TimeSpan.FromMilliseconds(20);
            SensorTimer.Start();

        }
    }
}
