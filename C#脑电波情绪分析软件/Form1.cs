using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using Accord;
using Accord.Audio;
using Accord.Audio.Windows;
using System.Numerics;
using Accord.Audio.Filters;
using Accord.Audio.ComplexFilters;

namespace 心情音乐播放器
{
    public partial class Form1 : Form
    {
        private string str_load_dir = "";//文件目录
        public static float[] channel1 = new float[256];
        public static int counter1 = 0;
        public static float[] channel2 = new float[256];
        public static int counter2 = 0;
        public static float[] signal1 = new float[256];//用作传值
        public static float[] signal2 = new float[256];
        Signal S1;
        ComplexSignal signal;
        public static int Label_Count = 0;
        private static int CaLm = 170;

        private static int upsad = 90;
        public static int calmstate = 0;
        public static int PowerForAdjust = 0; //以前是局部变量
        int state = 0;//用作延时
        public static int[] Power = new int[10];//存储fft变换后能量
        Random ro = new Random();
         int status = 0;///记录上次情绪状态
        //Signal S2;用作第二通道
        //ComplexSignal signal;

        private void fft_process()
        {
            S1 = Signal.FromArray(channel1, 250, SampleFormat.Format32BitIeeeFloat); //从浮点数组创建一个新的信号
            signal = S1.ToComplex(); //将此信号转换为ComplexSignal信号
            signal.ForwardFourierTransform(); //对ComplexSignal信号应用前向快速傅立叶变换

            // Now we can get the power spectrum output and its
            // related frequency vector to plot our spectrometer.
            Complex[] channel = signal.GetChannel(0); //从信号中提取通道
            double[] power = Accord.Audio.Tools.GetPowerSpectrum(channel); //计算复杂信号的功率谱
            double[] freqv = Accord.Audio.Tools.GetFrequencyVector(signal.Length, signal.SampleRate); //创建均匀间隔的频率向量（假设对称FFT）
            power[0] = 0; // zero DC
            float[] g = new float[power.Length];
            for (int i = 0; i < power.Length; i++)
            {
                g[i] = (float)power[i];
                signal1[i] = g[i] * 1000;
                SetpowerCC(g[i]); //输出power的值
                SetFrequCC(freqv[i]); //输出freqv的值
            }
            powerCaculate();   //对小于20hz的部分求和
            pictureBox2.Refresh(); //重新绘制Channel1频域图

        }

        //音乐播放函数
        private void Adjust()
        {
            state++; //用作延时，否则变换速度太快！
            for (int i = 0; i < 10; i++)
            {
                if (Power[i] > CaLm)
                {
                    PowerForAdjust++;
                    Excited(PowerForAdjust); //输出为激动的次数
                }
                else if (Power[i] < upsad)
                {
                    calmstate++;
                    Upsad(calmstate);  //输出为悲伤的次数
                }
                Power[i] = 0;//记得置零，要不然会慢慢激动的
            }
            while (state == 4)//////////7
            {
                if (PowerForAdjust > 12) //最好听完一首歌后再依据心情切歌
                {                    
                        if (serialPort2.IsOpen)
                        { serialPort2.Write("H"); }
                        if (serialPort3.IsOpen)
                        { serialPort3.Write("H"); }

                        label18.Text = "你有点小激动，不妨试试这首歌吧";
                        int iResult;
                        int iUp = ListSongs1.Count;
                        iResult = ro.Next(iUp);//也换换皮肤吧
                        skinEngine1.SkinFile = @"D:\百度云同步盘\GUI\心情音乐播放器\心情音乐播放器\皮肤\skin\SteelBlue.ssk";
                        axWindowsMediaPlayer1.URL = ListSongs1[iResult];
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                        state = 0;                     
                }
                else if (calmstate >= 4)
                {                   
                        if (serialPort2.IsOpen)
                        { serialPort2.Write("L"); }
                        if (serialPort3.IsOpen)
                        { serialPort3.Write("L"); }

                        label18.Text = "你似乎有点低落，来首歌舒缓一下吧";
                        int k = ListSongs3.Count;
                        k = ro.Next(k);
                        axWindowsMediaPlayer1.URL = ListSongs3[k];
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                        skinEngine1.SkinFile = @"D:\百度云同步盘\GUI\心情音乐播放器\心情音乐播放器\皮肤\skin\Vista2_color6.ssk";
                        calmstate = 0;
                        state = 0;                       
                }
                else
                {                    
                        if (serialPort2.IsOpen)
                        { serialPort2.Write("P"); }
                        if (serialPort3.IsOpen)
                        { serialPort3.Write("P"); }

                        label18.Text = "你现在比较平静，不妨欣赏一下这些歌吧";
                        int k = ListSongs2.Count;
                        k = ro.Next(k);
                        axWindowsMediaPlayer1.URL = ListSongs2[k];
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                        skinEngine1.SkinFile = @"D:\百度云同步盘\GUI\心情音乐播放器\心情音乐播放器\皮肤\skin\WarmColor3.ssk";
                        state = 0;
                        status = 3;                                       
                }
                PowerForAdjust = 0;
                calmstate = 0;
            }
        }

        private void powerCaculate()
        {
            for (int j = 0; j < 20; j++)   //计数器j
            {
                Power[Label_Count] = (int)signal1[j] + Power[Label_Count]; //对小于20hz的部分求和
            }
            setLabel(Label_Count, Power[Label_Count]);  //将求和部分的数值在界面的左边显示出来，要是低于clam就绿色，高于就红色

        }


        //卡尔曼滤波
        private float[] KalmanFilter(float[] data)
        {
            float[] Xkalm = new float[data.Length];
            //初值
            float P = 0;                           //先验估计协方差
            float kg = 0;                          //卡尔曼增益
            float Xforecast = 0;                   //状态预测值
            //开始滤波并附初值
            Xkalm[0] = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                Xforecast = Xkalm[i - 1];
                P = P + 1.3f;
                kg = P / (P + 0.01f);
                Xkalm[i] = Xforecast + kg * (data[i] - Xforecast);
                P = (1 - kg) * P;
            }
            return Xkalm;
        }

        private void SetChanne1(float value)
        {
            label1.Invoke(new EventHandler(delegate
             {
                 value = value * 0.224f;
                 channel1[counter1] = value; //得到256个采样点
                 counter1 = counter1 + 1;
                 while (counter1 == 255)
                 {
                     channel1[counter1] = value;
                     //channel1 = KalmanFilter(channel1);
                     Setpower1(channel1); //在右下角输出channel1
                     //signal1.CopyTo(channel1, 0);
                     //L1 = signal1.ToList();
                     //L1.Sort();
                     pictureBox1.Refresh(); //重新绘制Channel1波形图
                     fft_process();
                     //mid_process();
                     //fft_anlaysis(channel1);
                     //signal1.CopyTo(channel1, 0);
                     //pictureBox2.Refresh();//进程中进行不到这一步，那么在同一张画布中作图
                     //fft_process(signal1);
                     //fft_anlaysis(signal1);
                     counter1 = 0;
                 }
                 label1.Text = value.ToString();  //在Channel1后面输出值
             }));
        }

        private void SetChanne2(float value)
        {
            label2.Invoke(new EventHandler(delegate
            {
                value = value * 0.224f;
                channel2[counter2] = value; //得到256个采样点
                counter2 = counter2 + 1;
                while (counter2 == 255)
                {
                    channel2 = KalmanFilter(channel2); //进行卡尔曼滤波
                    Setpower2(channel2);    //在右下角输出channel2
                    signal2.CopyTo(channel2, 0);
                    //fft_process(signal2);
                    counter2 = 0;
                }
                label2.Text = value.ToString();  //在Channel2后面数去值
            }));
        }

        
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytes = 1024;//comport.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[bytes];
            serialPort1.Read(buffer, 0, bytes); //从串口读取数据
            for (int i = 0; i < 992; i++)
            {
                if (buffer[i] == 0xA0 || buffer[i + 32] == 0xC0) //蓝牙串口每帧数据33字节，其中0xA0为帧头，0xC0为帧尾
                {
                    buffer[i + 2] <<= 8;
                    buffer[i + 2] |= buffer[i + 3];
                    buffer[i + 2] <<= 8;
                    buffer[i + 2] |= buffer[i + 4];
                    buffer[i + 5] <<= 8;
                    buffer[i + 5] |= buffer[i + 6];
                    buffer[i + 5] <<= 8;
                    buffer[i + 5] |= buffer[i + 7];

                    SetChanne1((float)buffer[i + 2]); //用来FFT计算并输出
                    SetChanne2((float)buffer[i + 5]); //用来卡尔曼滤波计算并输出

                }
            }

        }


        public Form1()
        {
            File.AppendAllText("LoadMusic.txt", "");//打开文件，追加字符串
            InitializeComponent();
            str_load_dir = File.ReadAllText("LoadMusic.txt");
            if (str_load_dir != "")
                Loadmusics();
        }

        private void Loadmusics()
        {
            try
            {   //遍历打开的文件，将文件名添加到ListBox控件中，加入播放文件列表
                foreach (string filename in Directory.GetFiles(str_load_dir))
                {
                    FileInfo fi = new FileInfo(filename);
                    if (fi.Extension == ".*" || fi.Extension == ".wmv" || fi.Extension == ".mp3" || fi.Extension == ".wma" || fi.Extension == ".avi")
                    {
                        listBox1.Items.Add(fi.Name);
                        axWindowsMediaPlayer1.currentPlaylist.insertItem(axWindowsMediaPlayer1.currentPlaylist.count, axWindowsMediaPlayer1.newMedia(filename));
                    }
                }
            }
            catch
            {
                File.WriteAllText("LoadMusic.txt", "");
            }
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            skinEngine1.SkinFile = @"D:\百度云同步盘\GUI\心情音乐播放器\心情音乐播放器\皮肤\skin\MP10.SSK";  //加载皮肤
            string[] portNames = System.IO.Ports.SerialPort.GetPortNames();   //来查询当前计算机的有效的串行端口名称列表
            for (int i = 0; i <= portNames.Length - 1; i++)
            {
                comboBox1.Items.Add(portNames[i]); //在下拉组合框控件中输出有效的串行端口名称
                comboBox2.Items.Add(portNames[i]); //蓝牙
                comboBox3.Items.Add(portNames[i]);

            }
            serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived);
            serialPort2.DataReceived += serialPort2_DataReceived;//////////////
            serialPort3.DataReceived += serialPort3_DataReceived;//////////////
        }
        ///jia/////////////////////////////////////////////////////////////////////////////////

        string datain;
        private void serialPort2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            datain = serialPort2.ReadExisting();
            this.Invoke(new EventHandler(DisplayText));
        }
        private void serialPort3_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            datain = serialPort2.ReadExisting();
            this.Invoke(new EventHandler(DisplayText));
        }

        private void DisplayText(object sender, EventArgs e)
        {
            //label18.Text = datain;
        }
        ///jia////////////////////////////////////////////////////////////////////////////////////

        private void setLabel(int count, int value)
        {
            switch (count)
            {
                case 0:
                    setState(state+1);
                    setValue1(value);
                    Label_Count++;
                    break;
                case 1:
                    setValue2(value);
                    Label_Count++;
                    break;
                case 2:
                    setValue3(value);
                    Label_Count++;
                    break;
                case 3:
                    setValue4(value);
                    Label_Count++;
                    break;
                case 4:
                    setValue5(value);
                    Label_Count++;
                    break;
                case 5:
                    setValue6(value);
                    Label_Count++;
                    break;
                case 6:
                    setValue7(value);
                    Label_Count++;
                    break;
                case 7:
                    setValue8(value);
                    Label_Count++;
                    break;
                case 8:
                    setValue9(value);
                    Label_Count++;
                    break;
                case 9:
                    setValue10(value);
                    Adjust();
                    Label_Count = 0;
                    break;
        
            }
        }

        //输出激动次数
        private void Excited(int PowerForAdjust)
        {
            label25.Invoke(new EventHandler(delegate
            {
                label25.Text = PowerForAdjust.ToString();
                label25.BackColor = Color.Red;
            }));
        }
        //输出低落次数
        private void Upsad(int calmstate)
        {
            label27.Invoke(new EventHandler(delegate
            {
                label27.Text = calmstate.ToString();
                label27.BackColor = Color.Yellow;
            }));
        }

        //输出循环次数
        private void setState(int state)
        {
            label23.Invoke(new EventHandler(delegate
            { 
                label23.Text = state.ToString();
                label23.BackColor = Color.Blue;

            }));
        }

        private void setValue1(int value)
        {
            label8.Invoke(new EventHandler(delegate
                 {
                     label8.Text = value.ToString();
                     if (value <= CaLm && value >=upsad)
                     { label8.BackColor = Color.Green; }
                     else if(value > CaLm)
                         label8.BackColor = Color.Red;
                     else
                         label8.BackColor = Color.Yellow;

                 }));
        }
        private void setValue2(int value)
        {
            label9.Invoke(new EventHandler(delegate
            {
                label9.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label9.BackColor = Color.Green; }
                else if (value > CaLm)
                    label9.BackColor = Color.Red;
                else
                    label9.BackColor = Color.Yellow;
            }));
        }
        private void setValue3(int value)
        {
            label10.Invoke(new EventHandler(delegate
            {
                label10.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label10.BackColor = Color.Green; }
                else if (value > CaLm)
                    label10.BackColor = Color.Red;
                else
                    label10.BackColor = Color.Yellow;
            }));
        }
        private void setValue4(int value)
        {
            label11.Invoke(new EventHandler(delegate
            {
                label11.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label11.BackColor = Color.Green; }
                else if (value > CaLm)
                    label11.BackColor = Color.Red;
                else
                    label11.BackColor = Color.Yellow;
            }));
        }
        private void setValue5(int value)
        {
            label12.Invoke(new EventHandler(delegate
            {
                label12.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label12.BackColor = Color.Green; }
                else if (value > CaLm)
                    label12.BackColor = Color.Red;
                else
                    label12.BackColor = Color.Yellow;
            }));
        }
        private void setValue6(int value)
        {
            label13.Invoke(new EventHandler(delegate
            {
                label13.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label13.BackColor = Color.Green; }
                else if (value > CaLm)
                    label13.BackColor = Color.Red;
                else
                    label13.BackColor = Color.Yellow;
            }));
        }
        private void setValue7(int value)
        {
            label14.Invoke(new EventHandler(delegate
            {
                label14.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label14.BackColor = Color.Green; }
                else if (value > CaLm)
                    label14.BackColor = Color.Red;
                else
                    label14.BackColor = Color.Yellow;
            }));
        }
        private void setValue8(int value)
        {
            label15.Invoke(new EventHandler(delegate
            {
                label15.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label15.BackColor = Color.Green; }
                else if (value > CaLm)
                    label15.BackColor = Color.Red;
                else
                    label15.BackColor = Color.Yellow;
            }));
        }
        private void setValue9(int value)
        {
            label16.Invoke(new EventHandler(delegate
            {
                label16.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label16.BackColor = Color.Green; }
                else if (value > CaLm)
                    label16.BackColor = Color.Red;
                else
                    label16.BackColor = Color.Yellow;
            }));
        }
        private void setValue10(int value)
        {
            label17.Invoke(new EventHandler(delegate
            {
                label17.Text = value.ToString();
                if (value <= CaLm && value >= upsad)
                { label17.BackColor = Color.Green; }
                else if (value > CaLm)
                    label17.BackColor = Color.Red;
                else
                    label17.BackColor = Color.Yellow;
            }));
        }       
        private void SetpowerCC(float value)
        {
            textBox3.Invoke(new EventHandler(delegate
               {
                   textBox3.Text = value.ToString();
               }));
        }
        private void SetFrequCC(double value)
        {
            textBox4.Invoke(new EventHandler(delegate
            {
                textBox4.Text = value.ToString();
            }));
        }

        //“开始”按钮
        private void button4_Click(object sender, EventArgs e)
        {
            string CommandBuffer = "b";  //通过串口发送“b”对OpenBCI使能
            //发送前先清空接收缓存
            //  while (serialPort1.BytesToRead != 0) { serialPort1.ReadByte(); }
            SendCommand(CommandBuffer);
            //ShowSend(CommandBuffer);
            //等待一秒等模块回应
            Thread.Sleep(1000);
        }

        //“停止”按钮
        private void button5_Click(object sender, EventArgs e)
        {
            string CommandBuffer = "s";  //通过串口发送“s”使OpenBCI停止工作
            //发送前先清空接收缓存
            while (serialPort1.BytesToRead != 0) { serialPort1.ReadByte(); }
            SendCommand(CommandBuffer);
            //ShowSend(CommandBuffer);
            ////等待一秒等模块回应
            Thread.Sleep(1000);
        }

        private void SendCommand(string data)
        {
            serialPort1.Write(data);
        }
        //private void ShowSend(string data)
        //{
        //    String mess = "发送\t->\t";
        //    mess += " " + data;
        //    richTextBox1.AppendText(mess);
        //    richTextBox1.ScrollToCaret();
        //}

        //“refresh”按钮
        private void button3_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            string[] portNames = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i <= portNames.Length - 1; i++)
            {
                comboBox1.Items.Add(portNames[i]);
                comboBox2.Items.Add(portNames[i]);
                comboBox3.Items.Add(portNames[i]);
            }
        }

        //“打开开发板串口”按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                while (serialPort1.BytesToRead != 0) { serialPort1.ReadByte(); }//关串口前先清下缓存
                serialPort1.Close();
                button1.Text = "打开开发板串口";
            }

            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = 115200;

                serialPort1.Open();
                //button1.Enabled = false;
                button1.Text = "关闭开发板串口";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //“打开机器人蓝牙”按钮
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort2.IsOpen)
            {
                while (serialPort2.BytesToRead != 0) { serialPort2.ReadByte(); }//关串口前先清下缓存
                serialPort2.Close();
                button2.Text = "打开机器人蓝牙";
            }

            try
            {
                serialPort2.PortName = comboBox2.Text;
                serialPort2.BaudRate = 9600;

                serialPort2.Open();
                //button1.Enabled = false;
                button2.Text = "关闭机器人蓝牙";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //“打开手机蓝牙”按钮
        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort3.IsOpen)
            {
                while (serialPort3.BytesToRead != 0) { serialPort3.ReadByte(); }//关串口前先清下缓存
                serialPort3.Close();
                button6.Text = "打开手机蓝牙";
            }
            try
            {
                serialPort3.PortName = comboBox3.Text;
                serialPort3.BaudRate = 9600;
                serialPort3.Open();
                button6.Text = "关闭手机蓝牙";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //在右下角输出channel1
        private void Setpower1(float[] value)
        {
            richTextBox2.Invoke(new EventHandler(delegate
            {
                string s1 = "";
                foreach (float b1 in value)
                { s1 += b1.ToString() + " "; }
                richTextBox2.AppendText(s1);

            }
                 ));
        }

        //在右下角输出channel2
        private void Setpower2(float[] value)
        {
            richTextBox3.Invoke(new EventHandler(delegate
            {
                string s = "";
                foreach (float b in value)
                { s += b.ToString() + " "; }
                richTextBox3.AppendText(s);

            }
                 ));
        }

        //画channel1波形图
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            int width = pictureBox1.Width;
            int height1 = pictureBox1.Height;

            for (int i = 1; i < 256; i++)
            {
                int x = (width / 260) * i;
                int y = (int)channel1[i - 1] + (height1 / 2);
                System.Drawing.Point piontx = new System.Drawing.Point(x, y);
                // e.Graphics.DrawEllipse(new Pen(Color.Green, 0.5f), x, y, 3, 3);
                int x1 = (width / 260) * (i + 1);
                int y1 = (int)channel1[i] + (height1 / 2);
                System.Drawing.Point piontx1 = new System.Drawing.Point(x1, y1);
                e.Graphics.DrawLine(new Pen(Color.Blue, 0.2f), piontx, piontx1);
            }
        }

        //画channel2频域图
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            int width = pictureBox2.Width;
            int height1 = pictureBox2.Height;

            for (int i = 1; i < 85; i++) //以前为i<60；
            {
                int x = (width / 260) * i * 3;
                int y = (int)signal1[i - 1] + height1 / 2;
                System.Drawing.Point piontx = new System.Drawing.Point(x, y);
                //   e.Graphics.DrawEllipse(new Pen(Color.Green, 0.5f), x, y, 3, 3);
                int x1 = (width / 260) * (i + 1) * 3;
                int y1 = (int)signal1[i] + height1 / 2;
                System.Drawing.Point piontx1 = new System.Drawing.Point(x1, y1);

                e.Graphics.DrawLine(new Pen(Color.Blue, 0.2f), piontx1, piontx);
            }
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();//对话框对象
            //过滤条件
            open.Filter = "所有文件.*|*.*|Wmv视频.wmv|*.wmv|歌曲.mp3|*.mp3|歌曲.wma|*.wma|文件.avi|*.avi";
            //open.FilterIndex = 1;
            if (open.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(open.FileName);//获取文件
                int i;
                //将打开的文件添加到ListBox控件中
                for (i = 0; i < listBox1.Items.Count; i++)
                {
                    if (fi.Name == listBox1.Items[i].ToString())//有重复不添加
                        break;
                }
                if (i == listBox1.Items.Count)
                {
                    this.listBox1.Items.Add(fi.Name);//添加到ListBox控件中
                    //播放文件
                    axWindowsMediaPlayer1.currentPlaylist.insertItem(axWindowsMediaPlayer1.currentPlaylist.count, axWindowsMediaPlayer1.newMedia(open.FileName));
                }
            }
        }
        //     bool isplayer = true;

        private void 快进ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.Ctlcontrols.fastForward();      //快进
        }

        private void 快退ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.Ctlcontrols.fastReverse();      //快退
        }

        private void 循环播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.setMode("loop", true);          //循环播放
        }

        private void 顺序播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.setMode("shuffle", false);      //顺序播放
        }

        private void 随机播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.setMode("shuffle", true);       //随机播放
        }

        private void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();               //停止 
        }

        private void 播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.play();           //播放
                                                                //   isplayer = true;
        }

        private void 说明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("相应的在表二表三表四中放置你在激动时，平静时和失落时想听的歌吧");
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            int j = listBox1.SelectedIndex;
            //axWindowsMediaPlayer1.Ctlcontrols.playItem(axWindowsMediaPlayer1.currentPlaylist.get_Item(j));
        }

        private void label10_Click_1(object sender, EventArgs e)
        {
        }

        
        List<string> ListSongs1 = new List<string>();
        private void listBox2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择要播放的音乐";
            ofd.Multiselect = true;
            ofd.Filter = "音乐文件|*.mp3|所有文件|*.*";
            ofd.ShowDialog();

            //获得我们在对话框中选中的文件的全路径
            string[] filePath = ofd.FileNames;
            //根据全路径截取文件名加载到ListBox列表中
            //需要将数组中的全路径存储起来
            for (int i = 0; i < filePath.Length; i++)
            {
                //将全路径存储到集合中
                ListSongs1.Add(filePath[i]);
                //将文件名截取出来放置到ListBox列表中
                listBox2.Items.Add(Path.GetFileName(filePath[i]));
            }
        }
        List<string> ListSongs2 = new List<string>();
        List<string> ListSongs3 = new List<string>();

        private void listBox3_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择要播放的音乐";
            ofd.Multiselect = true;
            ofd.Filter = "音乐文件|*.mp3|所有文件|*.*";
            ofd.ShowDialog();

            //获得我们在对话框中选中的文件的全路径
            string[] filePath = ofd.FileNames;
            //根据全路径截取文件名加载到ListBox列表中
            //需要将数组中的全路径存储起来
            for (int i = 0; i < filePath.Length; i++)
            {
                //将全路径存储到集合中
                ListSongs2.Add(filePath[i]);
                //将文件名截取出来放置到ListBox列表中
                listBox3.Items.Add(Path.GetFileName(filePath[i]));
            }
        }

        private void listBox4_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择要播放的音乐";
            ofd.Multiselect = true;
            ofd.Filter = "音乐文件|*.mp3|所有文件|*.*";
            ofd.ShowDialog();

            //获得我们在对话框中选中的文件的全路径
            string[] filePath = ofd.FileNames;
            //根据全路径截取文件名加载到ListBox列表中
            //需要将数组中的全路径存储起来
            for (int i = 0; i < filePath.Length; i++)
            {
                //将全路径存储到集合中
                ListSongs3.Add(filePath[i]);
                //将文件名截取出来放置到ListBox列表中
                listBox4.Items.Add(Path.GetFileName(filePath[i]));
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }
    }
}
