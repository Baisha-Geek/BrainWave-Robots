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

namespace �������ֲ�����
{
    public partial class Form1 : Form
    {
        private string str_load_dir = "";//�ļ�Ŀ¼
        public static float[] channel1 = new float[256];
        public static int counter1 = 0;
        public static float[] channel2 = new float[256];
        public static int counter2 = 0;
        public static float[] signal1 = new float[256];//������ֵ
        public static float[] signal2 = new float[256];
        Signal S1;
        ComplexSignal signal;
        public static int Label_Count = 0;
        private static int CaLm = 170;

        private static int upsad = 90;
        public static int calmstate = 0;
        public static int PowerForAdjust = 0; //��ǰ�Ǿֲ�����
        int state = 0;//������ʱ
        public static int[] Power = new int[10];//�洢fft�任������
        Random ro = new Random();
         int status = 0;///��¼�ϴ�����״̬
        //Signal S2;�����ڶ�ͨ��
        //ComplexSignal signal;

        private void fft_process()
        {
            S1 = Signal.FromArray(channel1, 250, SampleFormat.Format32BitIeeeFloat); //�Ӹ������鴴��һ���µ��ź�
            signal = S1.ToComplex(); //�����ź�ת��ΪComplexSignal�ź�
            signal.ForwardFourierTransform(); //��ComplexSignal�ź�Ӧ��ǰ����ٸ���Ҷ�任

            // Now we can get the power spectrum output and its
            // related frequency vector to plot our spectrometer.
            Complex[] channel = signal.GetChannel(0); //���ź�����ȡͨ��
            double[] power = Accord.Audio.Tools.GetPowerSpectrum(channel); //���㸴���źŵĹ�����
            double[] freqv = Accord.Audio.Tools.GetFrequencyVector(signal.Length, signal.SampleRate); //�������ȼ����Ƶ������������Գ�FFT��
            power[0] = 0; // zero DC
            float[] g = new float[power.Length];
            for (int i = 0; i < power.Length; i++)
            {
                g[i] = (float)power[i];
                signal1[i] = g[i] * 1000;
                SetpowerCC(g[i]); //���power��ֵ
                SetFrequCC(freqv[i]); //���freqv��ֵ
            }
            powerCaculate();   //��С��20hz�Ĳ������
            pictureBox2.Refresh(); //���»���Channel1Ƶ��ͼ

        }

        //���ֲ��ź���
        private void Adjust()
        {
            state++; //������ʱ������任�ٶ�̫�죡
            for (int i = 0; i < 10; i++)
            {
                if (Power[i] > CaLm)
                {
                    PowerForAdjust++;
                    Excited(PowerForAdjust); //���Ϊ�����Ĵ���
                }
                else if (Power[i] < upsad)
                {
                    calmstate++;
                    Upsad(calmstate);  //���Ϊ���˵Ĵ���
                }
                Power[i] = 0;//�ǵ����㣬Ҫ��Ȼ������������
            }
            while (state == 4)//////////7
            {
                if (PowerForAdjust > 12) //�������һ�׸�������������и�
                {                    
                        if (serialPort2.IsOpen)
                        { serialPort2.Write("H"); }
                        if (serialPort3.IsOpen)
                        { serialPort3.Write("H"); }

                        label18.Text = "���е�С�����������������׸��";
                        int iResult;
                        int iUp = ListSongs1.Count;
                        iResult = ro.Next(iUp);//Ҳ����Ƥ����
                        skinEngine1.SkinFile = @"D:\�ٶ���ͬ����\GUI\�������ֲ�����\�������ֲ�����\Ƥ��\skin\SteelBlue.ssk";
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

                        label18.Text = "���ƺ��е���䣬���׸��滺һ�°�";
                        int k = ListSongs3.Count;
                        k = ro.Next(k);
                        axWindowsMediaPlayer1.URL = ListSongs3[k];
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                        skinEngine1.SkinFile = @"D:\�ٶ���ͬ����\GUI\�������ֲ�����\�������ֲ�����\Ƥ��\skin\Vista2_color6.ssk";
                        calmstate = 0;
                        state = 0;                       
                }
                else
                {                    
                        if (serialPort2.IsOpen)
                        { serialPort2.Write("P"); }
                        if (serialPort3.IsOpen)
                        { serialPort3.Write("P"); }

                        label18.Text = "�����ڱȽ�ƽ������������һ����Щ���";
                        int k = ListSongs2.Count;
                        k = ro.Next(k);
                        axWindowsMediaPlayer1.URL = ListSongs2[k];
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                        skinEngine1.SkinFile = @"D:\�ٶ���ͬ����\GUI\�������ֲ�����\�������ֲ�����\Ƥ��\skin\WarmColor3.ssk";
                        state = 0;
                        status = 3;                                       
                }
                PowerForAdjust = 0;
                calmstate = 0;
            }
        }

        private void powerCaculate()
        {
            for (int j = 0; j < 20; j++)   //������j
            {
                Power[Label_Count] = (int)signal1[j] + Power[Label_Count]; //��С��20hz�Ĳ������
            }
            setLabel(Label_Count, Power[Label_Count]);  //����Ͳ��ֵ���ֵ�ڽ���������ʾ������Ҫ�ǵ���clam����ɫ�����ھͺ�ɫ

        }


        //�������˲�
        private float[] KalmanFilter(float[] data)
        {
            float[] Xkalm = new float[data.Length];
            //��ֵ
            float P = 0;                           //�������Э����
            float kg = 0;                          //����������
            float Xforecast = 0;                   //״̬Ԥ��ֵ
            //��ʼ�˲�������ֵ
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
                 channel1[counter1] = value; //�õ�256��������
                 counter1 = counter1 + 1;
                 while (counter1 == 255)
                 {
                     channel1[counter1] = value;
                     //channel1 = KalmanFilter(channel1);
                     Setpower1(channel1); //�����½����channel1
                     //signal1.CopyTo(channel1, 0);
                     //L1 = signal1.ToList();
                     //L1.Sort();
                     pictureBox1.Refresh(); //���»���Channel1����ͼ
                     fft_process();
                     //mid_process();
                     //fft_anlaysis(channel1);
                     //signal1.CopyTo(channel1, 0);
                     //pictureBox2.Refresh();//�����н��в�����һ������ô��ͬһ�Ż�������ͼ
                     //fft_process(signal1);
                     //fft_anlaysis(signal1);
                     counter1 = 0;
                 }
                 label1.Text = value.ToString();  //��Channel1�������ֵ
             }));
        }

        private void SetChanne2(float value)
        {
            label2.Invoke(new EventHandler(delegate
            {
                value = value * 0.224f;
                channel2[counter2] = value; //�õ�256��������
                counter2 = counter2 + 1;
                while (counter2 == 255)
                {
                    channel2 = KalmanFilter(channel2); //���п������˲�
                    Setpower2(channel2);    //�����½����channel2
                    signal2.CopyTo(channel2, 0);
                    //fft_process(signal2);
                    counter2 = 0;
                }
                label2.Text = value.ToString();  //��Channel2������ȥֵ
            }));
        }

        
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytes = 1024;//comport.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[bytes];
            serialPort1.Read(buffer, 0, bytes); //�Ӵ��ڶ�ȡ����
            for (int i = 0; i < 992; i++)
            {
                if (buffer[i] == 0xA0 || buffer[i + 32] == 0xC0) //��������ÿ֡����33�ֽڣ�����0xA0Ϊ֡ͷ��0xC0Ϊ֡β
                {
                    buffer[i + 2] <<= 8;
                    buffer[i + 2] |= buffer[i + 3];
                    buffer[i + 2] <<= 8;
                    buffer[i + 2] |= buffer[i + 4];
                    buffer[i + 5] <<= 8;
                    buffer[i + 5] |= buffer[i + 6];
                    buffer[i + 5] <<= 8;
                    buffer[i + 5] |= buffer[i + 7];

                    SetChanne1((float)buffer[i + 2]); //����FFT���㲢���
                    SetChanne2((float)buffer[i + 5]); //�����������˲����㲢���

                }
            }

        }


        public Form1()
        {
            File.AppendAllText("LoadMusic.txt", "");//���ļ���׷���ַ���
            InitializeComponent();
            str_load_dir = File.ReadAllText("LoadMusic.txt");
            if (str_load_dir != "")
                Loadmusics();
        }

        private void Loadmusics()
        {
            try
            {   //�����򿪵��ļ������ļ�����ӵ�ListBox�ؼ��У����벥���ļ��б�
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
            skinEngine1.SkinFile = @"D:\�ٶ���ͬ����\GUI\�������ֲ�����\�������ֲ�����\Ƥ��\skin\MP10.SSK";  //����Ƥ��
            string[] portNames = System.IO.Ports.SerialPort.GetPortNames();   //����ѯ��ǰ���������Ч�Ĵ��ж˿������б�
            for (int i = 0; i <= portNames.Length - 1; i++)
            {
                comboBox1.Items.Add(portNames[i]); //��������Ͽ�ؼ��������Ч�Ĵ��ж˿�����
                comboBox2.Items.Add(portNames[i]); //����
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

        //�����������
        private void Excited(int PowerForAdjust)
        {
            label25.Invoke(new EventHandler(delegate
            {
                label25.Text = PowerForAdjust.ToString();
                label25.BackColor = Color.Red;
            }));
        }
        //����������
        private void Upsad(int calmstate)
        {
            label27.Invoke(new EventHandler(delegate
            {
                label27.Text = calmstate.ToString();
                label27.BackColor = Color.Yellow;
            }));
        }

        //���ѭ������
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

        //����ʼ����ť
        private void button4_Click(object sender, EventArgs e)
        {
            string CommandBuffer = "b";  //ͨ�����ڷ��͡�b����OpenBCIʹ��
            //����ǰ����ս��ջ���
            //  while (serialPort1.BytesToRead != 0) { serialPort1.ReadByte(); }
            SendCommand(CommandBuffer);
            //ShowSend(CommandBuffer);
            //�ȴ�һ���ģ���Ӧ
            Thread.Sleep(1000);
        }

        //��ֹͣ����ť
        private void button5_Click(object sender, EventArgs e)
        {
            string CommandBuffer = "s";  //ͨ�����ڷ��͡�s��ʹOpenBCIֹͣ����
            //����ǰ����ս��ջ���
            while (serialPort1.BytesToRead != 0) { serialPort1.ReadByte(); }
            SendCommand(CommandBuffer);
            //ShowSend(CommandBuffer);
            ////�ȴ�һ���ģ���Ӧ
            Thread.Sleep(1000);
        }

        private void SendCommand(string data)
        {
            serialPort1.Write(data);
        }
        //private void ShowSend(string data)
        //{
        //    String mess = "����\t->\t";
        //    mess += " " + data;
        //    richTextBox1.AppendText(mess);
        //    richTextBox1.ScrollToCaret();
        //}

        //��refresh����ť
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

        //���򿪿����崮�ڡ���ť
        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                while (serialPort1.BytesToRead != 0) { serialPort1.ReadByte(); }//�ش���ǰ�����»���
                serialPort1.Close();
                button1.Text = "�򿪿����崮��";
            }

            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = 115200;

                serialPort1.Open();
                //button1.Enabled = false;
                button1.Text = "�رտ����崮��";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //���򿪻�������������ť
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort2.IsOpen)
            {
                while (serialPort2.BytesToRead != 0) { serialPort2.ReadByte(); }//�ش���ǰ�����»���
                serialPort2.Close();
                button2.Text = "�򿪻���������";
            }

            try
            {
                serialPort2.PortName = comboBox2.Text;
                serialPort2.BaudRate = 9600;

                serialPort2.Open();
                //button1.Enabled = false;
                button2.Text = "�رջ���������";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //�����ֻ���������ť
        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort3.IsOpen)
            {
                while (serialPort3.BytesToRead != 0) { serialPort3.ReadByte(); }//�ش���ǰ�����»���
                serialPort3.Close();
                button6.Text = "���ֻ�����";
            }
            try
            {
                serialPort3.PortName = comboBox3.Text;
                serialPort3.BaudRate = 9600;
                serialPort3.Open();
                button6.Text = "�ر��ֻ�����";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //�����½����channel1
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

        //�����½����channel2
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

        //��channel1����ͼ
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

        //��channel2Ƶ��ͼ
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            int width = pictureBox2.Width;
            int height1 = pictureBox2.Height;

            for (int i = 1; i < 85; i++) //��ǰΪi<60��
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

        private void ��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();//�Ի������
            //��������
            open.Filter = "�����ļ�.*|*.*|Wmv��Ƶ.wmv|*.wmv|����.mp3|*.mp3|����.wma|*.wma|�ļ�.avi|*.avi";
            //open.FilterIndex = 1;
            if (open.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(open.FileName);//��ȡ�ļ�
                int i;
                //���򿪵��ļ���ӵ�ListBox�ؼ���
                for (i = 0; i < listBox1.Items.Count; i++)
                {
                    if (fi.Name == listBox1.Items[i].ToString())//���ظ������
                        break;
                }
                if (i == listBox1.Items.Count)
                {
                    this.listBox1.Items.Add(fi.Name);//��ӵ�ListBox�ؼ���
                    //�����ļ�
                    axWindowsMediaPlayer1.currentPlaylist.insertItem(axWindowsMediaPlayer1.currentPlaylist.count, axWindowsMediaPlayer1.newMedia(open.FileName));
                }
            }
        }
        //     bool isplayer = true;

        private void ���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.Ctlcontrols.fastForward();      //���
        }

        private void ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.Ctlcontrols.fastReverse();      //����
        }

        private void ѭ������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.setMode("loop", true);          //ѭ������
        }

        private void ˳�򲥷�ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.setMode("shuffle", false);      //˳�򲥷�
        }

        private void �������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.setMode("shuffle", true);       //�������
        }

        private void ֹͣToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();               //ֹͣ 
        }

        private void ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.play();           //����
                                                                //   isplayer = true;
        }

        private void ˵��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("��Ӧ���ڱ�����������з������ڼ���ʱ��ƽ��ʱ��ʧ��ʱ�����ĸ��");
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
            ofd.Title = "��ѡ��Ҫ���ŵ�����";
            ofd.Multiselect = true;
            ofd.Filter = "�����ļ�|*.mp3|�����ļ�|*.*";
            ofd.ShowDialog();

            //��������ڶԻ�����ѡ�е��ļ���ȫ·��
            string[] filePath = ofd.FileNames;
            //����ȫ·����ȡ�ļ������ص�ListBox�б���
            //��Ҫ�������е�ȫ·���洢����
            for (int i = 0; i < filePath.Length; i++)
            {
                //��ȫ·���洢��������
                ListSongs1.Add(filePath[i]);
                //���ļ�����ȡ�������õ�ListBox�б���
                listBox2.Items.Add(Path.GetFileName(filePath[i]));
            }
        }
        List<string> ListSongs2 = new List<string>();
        List<string> ListSongs3 = new List<string>();

        private void listBox3_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "��ѡ��Ҫ���ŵ�����";
            ofd.Multiselect = true;
            ofd.Filter = "�����ļ�|*.mp3|�����ļ�|*.*";
            ofd.ShowDialog();

            //��������ڶԻ�����ѡ�е��ļ���ȫ·��
            string[] filePath = ofd.FileNames;
            //����ȫ·����ȡ�ļ������ص�ListBox�б���
            //��Ҫ�������е�ȫ·���洢����
            for (int i = 0; i < filePath.Length; i++)
            {
                //��ȫ·���洢��������
                ListSongs2.Add(filePath[i]);
                //���ļ�����ȡ�������õ�ListBox�б���
                listBox3.Items.Add(Path.GetFileName(filePath[i]));
            }
        }

        private void listBox4_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "��ѡ��Ҫ���ŵ�����";
            ofd.Multiselect = true;
            ofd.Filter = "�����ļ�|*.mp3|�����ļ�|*.*";
            ofd.ShowDialog();

            //��������ڶԻ�����ѡ�е��ļ���ȫ·��
            string[] filePath = ofd.FileNames;
            //����ȫ·����ȡ�ļ������ص�ListBox�б���
            //��Ҫ�������е�ȫ·���洢����
            for (int i = 0; i < filePath.Length; i++)
            {
                //��ȫ·���洢��������
                ListSongs3.Add(filePath[i]);
                //���ļ�����ȡ�������õ�ListBox�б���
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
