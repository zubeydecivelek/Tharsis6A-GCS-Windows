using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Globalization;
using System.Net.Sockets;
using WinSCP;
using System.Reflection;
using System.Windows.Documents;
using Emgu.CV.Features2D;

namespace tharsis6A
{
    public partial class Form1 : Form
    {
        #region TANIMLAMALAR


        bool isRecording = false;


        StringBuilder stringBuffer = new StringBuilder();
        SessionOptions sessionOptions;
        Session FTPsession;
        OpenFileDialog file;
        string videoPath;
        string videoName;
        byte[] bytes;

        TcpClient tcpClient = new TcpClient();
        NetworkStream networkStream = null;
        private StreamWriter streamWriter;
        public StreamWriter clientData;
        VideoWriter video_writer;
        #endregion

        VideoCapture capture;
        public static SerialPort port;
        string textname;
        string output;
        string savePath = Environment.CurrentDirectory;


        GMapMarker marker;
        PointLatLng point1 = new PointLatLng();
        GMapOverlay markers = new GMapOverlay("markers");


        bool ayrilma = false;

        string[] splitVeri;

        public NetworkStream NetworkStream { get => networkStream; set => networkStream = value; }

        #region THREADS
        Thread cameraThread;
        Thread mapThread;
        Thread dataGridThread;
        Thread chartThread;
        Thread rpyInfoThread;
        Thread sendVideo;
        #endregion



        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            txtBuild();
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            #region SERIAL PORT SETUP

            comports.Items.Clear();
            String[] ports = SerialPort.GetPortNames();
            comports.Items.AddRange(ports);
            #endregion

            #region GMAP SETUP
            //GMap Initialize
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            gMap.DragButton = MouseButtons.Left;
            gMap.MapProvider = GMapProviders.GoogleMap;

            gMap.MinZoom = 10;
            gMap.MaxZoom = 200;
            gMap.Zoom = 17;

            gMap.Position = new PointLatLng(38.3991310, 33.7117840);

            //gMap.ShowCenter = true;

            // gMap.Refresh();

            #endregion
        }

        private void comBaglanBtn_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comports.Text;
                serialPort1.BaudRate = 115200;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;

                serialPort1.Open();
                port = serialPort1;
                if (serialPort1.IsOpen)
                {
                    baglantiCheckBox.Checked = true;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void comBaglantiKesBtn_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            port = serialPort1;
            if (!serialPort1.IsOpen)
            {
                baglantiCheckBox.Checked = false;  
            }
        }

        private void baslatBtn_Click(object sender, EventArgs e)
        {
            /*cameraThread = new Thread(new ThreadStart(setCamera));
            cameraThread.Start();
            Thread.Sleep(50);*/

            serialPort1.DataReceived += serialPort1_DataReceived;


        }

        private void durdurBtn_Click(object sender, EventArgs e)
        {
            comBaglantiKesBtn_Click(sender, e);
            capture.Stop();
        }

        private void ayrilBtn_Click(object sender, EventArgs e)
        {
            komutGonder("A");
            ayrilmaCheckBox.Checked = true;
        }

        private void birlestirBtn_Click(object sender, EventArgs e)
        {
            komutGonder("B");
        }

        private void statüBtn_Click(object sender, EventArgs e)
        {
            komutGonder("P");
        }

        private void sensorBtn_Click(object sender, EventArgs e)
        {
            komutGonder("K");
        }

        private void sifirlaBtn_Click(object sender, EventArgs e)
        {
            komutGonder("Z");
        }

        private void ipBaglanBtn_Click(object sender, EventArgs e)
        {
            tcpClient.Connect("192.168.4.1", 80);
            NetworkStream = tcpClient.GetStream();
            streamWriter = new StreamWriter(NetworkStream);
        }

        private void ipBagalantiKesBtn_Click(object sender, EventArgs e)
        {
            tcpClient.Dispose();
            tcpClient.Close();
            NetworkStream.Close();
        }

        private void videoGonderBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            file = ofd;
            if (file.ShowDialog() == DialogResult.OK)
            {
                videoPath = file.FileName;
                videoName = file.SafeFileName;
                MessageBox.Show("Gönderilmek istenen dosya:" + videoPath, "", MessageBoxButtons.OKCancel);
                bytes = File.ReadAllBytes(videoPath);



            }
            try
            {
                if (!backgroundWorker2.IsBusy)
                {
                    backgroundWorker2.RunWorkerAsync();
                }
            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }
        }



        private void setCamera()
        {
            
                if (capture == null)
                {
                    capture = new VideoCapture(0);
                    capture.ImageGrabbed += Capture_ImageGrabbed;
                    capture.Start();
                    string path = savePath + @"\Saves\" + DateTime.Now.ToString().Replace(' ', '-').Replace(':', ';') + @" output.avi";
                    video_writer = new VideoWriter(path, VideoWriter.Fourcc('M', 'P', '4', 'V'), 30, new Size(640, 480), true);
                    isRecording = true;
                }

            
            


        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                capture.Retrieve(m);

                Image<Bgr, byte> image = m.ToImage<Bgr, byte>();
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Image = image.ToBitmap();

                if (isRecording && video_writer != null)
                {
                    video_writer.Write(m);
                }

            }
            catch (Exception)
            {

            }
        }

        void saveText(String text)
        {
            try
            {
                StreamWriter writer = new StreamWriter(output, true);

                writer.Write(text);
                writer.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        void txtBuild()
        {
            try
            {
                textname = DateTime.Now.ToString().Replace(' ', '-');
                textname = textname.Replace(':', ';');
                textname = string.Concat(textname, ".csv");


                output = string.Concat(textname);

                output = savePath + @"\Saves\" + output;

                saveText("Takım No,Paket No, Görev Yükü Basınç,Taşıyıcı Basınç,Görev Yükü Yükseklik,Taşıyıcı Yükseklik,İrtifa Farkı, İniş Hızı,Sıcaklık,Pil Gerilimi,Latitude1,Longitude1,Altitude1,Latitude2,Longitude2,Altitude2,Statü,Pitch,Roll,Yaw,Dönüş Sayısı,Video Aktarımı\n");

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        void komutGonder(String komut)
        {
            DialogResult dialogResult = MessageBox.Show("Emin misiniz?", "Komut Onaylama", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //do something
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(komut + "~\n\r");
                    MessageBox.Show("Komut Gönderildi.");
           
                }
                else
                {
                    MessageBox.Show("Port bağlı değil.");
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
                MessageBox.Show("Komut Gönderilemedi.");
            }
        }


        
        private void drawMap(string gps1Latitude, string gps1Longitude)
        {
            double lat1 = double.Parse(gps1Latitude, CultureInfo.InvariantCulture);
            double long1 = double.Parse(gps1Longitude, CultureInfo.InvariantCulture);

            // double lat2 = lat1 + 0.01;
            // double long2 = long1 + 0.01;

            gMap.Position = new PointLatLng(lat1, long1);
            markers.Clear();
            gMap.Overlays.Clear();

            try
            {

                point1.Lat = lat1;
                point1.Lng = long1;
                marker = new GMarkerGoogle(point1, GMarkerGoogleType.red_dot);


                gMap.Overlays.Add(markers);

                markers.Markers.Add(marker);

                gMap.Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


        }

        private void RPYInfo(string[] splitVeri)
        {

            rollTextBox.Text = splitVeri[16].ToString();
            pitchTextBox.Text = splitVeri[15].ToString();
            yawTextBox.Text = splitVeri[17].ToString();

            try
            {

                Invoke(new Action(() =>
                {
                    rpySimulation.Rotate(double.Parse(splitVeri[16]), double.Parse(splitVeri[15]), double.Parse(splitVeri[17]));
                }));


            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void drawGraphs(string[] splitVeri)
        {
            try
            {
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }


        }

        private void addToGrid(string[] splitVeri)
        {
            DataGridViewRow row = (DataGridViewRow)veriGrid.Rows[0].Clone();
            for (int i = 0; i < 19; i++)
            {

                row.Cells[i].Value = splitVeri[i];

            }
            veriGrid.Rows.Add(row);

            if (veriGrid.Rows.Count > 6)
            {
                veriGrid.Rows.RemoveAt(0);
            }
        }



        void SendVideo()
        {
            if (NetworkStream.CanWrite)
            {
                NetworkStream.Write(bytes, 0, bytes.Length);
                //textBox1.Text = "GÖNDERİLİYOR";
                NetworkStream.Flush();


                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("V~\n\r");
                }

            }
            else if (!NetworkStream.CanWrite)
            {
                MessageBox.Show("Video Gönderilemiyor.");

            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
              try
            {
                irtifaLabel.Text = splitVeri[8].ToString();

                basıncChart.Series["Taşıyıcı"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[5], CultureInfo.InvariantCulture));
                basıncChart.Series["Görev Yükü"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[4], CultureInfo.InvariantCulture));

                yukseklikChart.Series["Taşıyıcı"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[7], CultureInfo.InvariantCulture));
                yukseklikChart.Series["Görev Yükü"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[6], CultureInfo.InvariantCulture));

                sıcaklıkChart.Series["Sıcaklık"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[10], CultureInfo.InvariantCulture));

                hızChart.Series["İniş Hızı"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[9], CultureInfo.InvariantCulture));

                pilChart.Series["Pil"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[11], CultureInfo.InvariantCulture));
            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            sendVideo = new Thread(() => SendVideo());
            sendVideo.Start();
            Thread.Sleep(50);
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    string newData = serialPort1.ReadExisting();

                    textBox2.Text = newData;

                    if (newData != null)
                    {
                        string[] telemetri = newData.Split(',');


                        if (telemetri.Length == 19)

                        {
                            setStatus(telemetri[1]);

                            setAras(telemetri[2]);

                            gauge(telemetri);

                            saveText(newData);
                            //telemetri[8] = GetRandomNumber(0.0, 1.0).ToString();
                            splitVeri = telemetri;

                            chartThread = new Thread(() => drawGraphs(telemetri));
                            chartThread.Start();
                            Thread.Sleep(50);

                            dataGridThread = new Thread(() => addToGrid(telemetri));
                            dataGridThread.Start();
                            Thread.Sleep(50);

                            rpyInfoThread = new Thread(() => RPYInfo(telemetri));
                            rpyInfoThread.Start();
                            Thread.Sleep(50);

                            mapThread = new Thread(() => drawMap(telemetri[12], telemetri[13]));
                            mapThread.Start();
                            Thread.Sleep(50);

                        }
                    }
                });

            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }
        }


        private void setStatus(string status)
        {
            int intStatus = int.Parse(status);
            switch (intStatus)
            {
                case 0:
                    statuLabel.Text = "Uçuşa Hazır";
                    break;
                case 1:
                    statuLabel.Text = "Yükselme";
                    break;
                case 2:
                    statuLabel.Text = "Uydu İniş";
                    break;
                case 3:
                    statuLabel.Text = "Ayrılma";
                    ayrilmaCheckBox.Checked = true;
                    break;
                case 4:
                    statuLabel.Text = "GY İniş";
                    break;
                case 5:
                    statuLabel.Text = "Kurtarma";
                    break;
                case 6:
                    statuLabel.Text = "Video Alındı";
                    videoCheckBox.Checked = true;
                    break;
                case 7:
                    statuLabel.Text = "Bonus Görev";
                    bonusCheckBox.Checked = true;
                    break;

            }
        }

        private void gauge(string[] telemetri)
        {
            inisHiziGauge.Value = Math.Abs(float.Parse(telemetri[9], CultureInfo.InvariantCulture));
            yukseklikGauge.Value = Math.Abs(float.Parse(telemetri[6], CultureInfo.InvariantCulture));
            pilGauge.Value = float.Parse(telemetri[11], CultureInfo.InvariantCulture);
            sicaklikGauge.Value = float.Parse(telemetri[10], CultureInfo.InvariantCulture);
        }

        private void setAras(string hataKodu)
        {
            if (hataKodu[0].Equals("0"))
            {
                aras0.BackColor = Color.Green;
            }
            else
            {
                aras0.BackColor= Color.Red;
            }

            if (hataKodu[1].Equals("0"))
            {
                aras1.BackColor = Color.Green;
            }
            else
            {
                aras1.BackColor = Color.Red;
            }

            if (hataKodu[2].Equals("0"))
            {
                aras2.BackColor = Color.Green;
            }
            else
            {
                aras2.BackColor = Color.Red;
            }

            if (hataKodu[3].Equals("0"))
            {
                aras3.BackColor = Color.Green;
            }
            else
            {
                aras3.BackColor = Color.Red;
            }

            if (hataKodu[4].Equals("0"))
            {
                aras4.BackColor = Color.Green;
            }
            else
            {
                aras4.BackColor = Color.Red;
            }

            
        }
    }
}
