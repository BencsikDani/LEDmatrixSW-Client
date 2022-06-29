using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Drawing.Imaging;

namespace MarLab_HF_UI
{
    public partial class MainForm : Form
    {
        // Változó, ami a betöltött GIF-ből létrehozott parancsokat tárolja
        string[] commands;
        // A 64 db textbox-nak egy lista, hogy gyorsan át lehessen őket adni
        List<TextBox> tbs = new List<TextBox>();

        public MainForm()
        {
            InitializeComponent();
            // Egyes osztályok inicializálása
            ImageMaster.Init(this);
            ComMaster.Init(this);
            LEDMaster.Init(this);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // COM portokat tartalmazó lenyíló lista frissítése
            cbCOMupdate();                              

            // Baud rate lenyíló listájának feltöltése a lehetséges elemekkel
            cbBaud.Items.Add(110);
            cbBaud.Items.Add(300);
            cbBaud.Items.Add(600);
            cbBaud.Items.Add(1200);
            cbBaud.Items.Add(2400);
            cbBaud.Items.Add(4800);
            cbBaud.Items.Add(9600);
            cbBaud.Items.Add(14400);
            cbBaud.Items.Add(19200);
            cbBaud.Items.Add(28800);
            cbBaud.Items.Add(38400);
            cbBaud.Items.Add(57600);
            cbBaud.Items.Add(115200);
            cbBaud.Items.Add(230400);
            cbBaud.Text = cbBaud.Items[12].ToString();

            // Adatbitek számát meghatározó lenyíló lista feltöltése
            cbAdat.Items.Add(5);
            cbAdat.Items.Add(6);
            cbAdat.Items.Add(7);
            cbAdat.Items.Add(8);
            cbAdat.Items.Add(9);
            cbAdat.Text = cbAdat.Items[3].ToString();

            // Stopbitek számát meghatározó lenyíló lista feltöltése
            cbStop.Items.Add(1);
            cbStop.Items.Add(1.5);
            cbStop.Items.Add(2);
            cbStop.Text = cbStop.Items[0].ToString();

            // Paritásbitek számát meghatározó lenyíló lista feltöltése
            cbParitas.Items.Add("Nincs");
            cbParitas.Items.Add("Páratlan (Odd)");
            cbParitas.Items.Add("Páros (Even)");
            cbParitas.Items.Add("Fix 1 (Mark)");
            cbParitas.Items.Add("Fix 0 (Space)");
            cbParitas.Text = cbParitas.Items[0].ToString();

            // Handshake módját maghatározó lenyíló lista feltöltése
            cbHs.Items.Add("Nincs");
            cbHs.Items.Add("XON/XOFF");
            cbHs.Items.Add("RTS");
            cbHs.Items.Add("XON/XOFF + RTS");
            cbHs.Text = cbHs.Items[0].ToString();

            // ProgressBar elrejtése
            progressBar.Visible = false;

            // A textbox lista feltöltése és sorba rendezése
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.Name.Contains("textBox"))
                    tbs.Add((TextBox)ctrl);
            }
            tbs = tbs.OrderBy(o => o.Left).ToList();
            tbs = tbs.OrderBy(o => o.Top).ToList();
        }

        
        void cbCOMupdate()
        {
            // Metódus, ami frissíti magát a COM portokat tartalmazó lenyíló listát

            // Töröljük az eddigi tartalmát
            cbCOM.Items.Clear();
            // Bekérjük az elérhető Portok neveit
            string[] COMnevek = SerialPort.GetPortNames();
            // Sorbarendezzük őket
            Array.Sort(COMnevek);
            // Ez után feltöltjük a lenyíló listát
            int i = 0;
            while (i != COMnevek.Length)
            {
                cbCOM.Items.Add(COMnevek[i]);
                i++;
            }
            // Ha van legalább 1 db Port, akkor beállítjuk alapértelmezetten az elsőt
            if (COMnevek[0] != null)
                cbCOM.Text = COMnevek[0];
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Ha adat érkezett a soros porton, akkor meghívjuk a ComMaster adatbekérő függvényét
            ComMaster.Instance.DataIn(tbs, sp);
        }

        private void button0_Click(object sender, EventArgs e)
        {
            // Ha 1 FPS-t választottunk, kiküldjük a hozzá tartozó utasítást a soros porton
            ComMaster.Instance.DataOut(sp, "b0\r\n\0");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Ha 10 FPS-t választottunk, kiküldjük a hozzá tartozó utasítást a soros porton
            ComMaster.Instance.DataOut(sp, "b1\r\n\0");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Ha 25 FPS-t választottunk, kiküldjük a hozzá tartozó utasítást a soros porton
            ComMaster.Instance.DataOut(sp, "b2\r\n\0");
        }
        

        private void button3_Click(object sender, EventArgs e)
        {
            // Ha 30.3 FPS-t választottunk, kiküldjük a hozzá tartozó utasítást a soros porton
            ComMaster.Instance.DataOut(sp, "b3\r\n\0");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // A MainForm bezárásakor lezárjuk a soros kommunikációs kapcsolatot (ha nyitva van)
            if (sp.IsOpen)
                ComMaster.Instance.CloseCom(sp);
        }

        private void btnCOMupdate_Click(object sender, EventArgs e)
        {
            // Frissítés gombra kattintva frissítjük a COM Portokat tartalmazó lenyló listát
            cbCOMupdate();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // A Csatlakozás/Kapcsolat bontása gombra nyomva, ha...
            // A kommunikáció nem él, akkor
            if (!sp.IsOpen)
            {
                // Elindítjuk a kommunikációt a felhasználói felületen bevitt adatokkal
                ComMaster.Instance.OpenCom(sp, cbCOM.Text, int.Parse(cbBaud.Text),
                    int.Parse(cbAdat.Text), (StopBits)int.Parse(cbStop.Text),
                    cbParitas.Text, cbHs.Text);
                // És megváltoztatjuk a Csatlakozás gomb feliratát
                btnConnect.Text = "Kapcsolat bontása";
                // Illetve ha már van betöltött parancsunk
                if (commands != null)
                    // Akkor engedélyezzük a feltöltés gombot
                    btnUploadGif.Enabled = true;
            }
            // Ha pedig a kommunikáció már élt, akkor
            else if (sp.IsOpen)
            {
                // Bezárjuk azt
                ComMaster.Instance.CloseCom(sp);
                // Visszaállítjuk a gomb feliratát
                btnConnect.Text = "Csatlakozás";
                // Kikapcsoljuk a feltöltés gombot
                btnUploadGif.Enabled = false;
                // És Reset-eljük a LED mátrixot
                LEDMaster.Instance.ResetLEDs(tbs);
            }
        }

        private void btnOpenGif_Click(object sender, EventArgs e)
        {
            // Ha a megnyitás gombra nyomtunk, elindítjuk a GIF megnyitását
            string path = ImageMaster.Instance.OpenGIF();
            // Ha a megnyitás sikerült
            if (path != string.Empty)
            {
                // Láthatóvá tesszük a ProgressBar-t
                progressBar.Visible = true;

                // Kivesszük a GIF-ből az egyes frame-eket
                Image[] frames = ImageMaster.Instance.GetGIFFrames(path);
                // Haladunk
                Progress(20);

                // Az összes frame-et átkonvertáljuk 2 színből (fekete vagy fehér) álló képre
                frames = ImageMaster.Instance.FramesToBiColor(frames);
                // Haladunk
                Progress(40);
                // Ezek után a frame-eket lekicsinyítjük
                frames = ImageMaster.Instance.ResizeGIFframes(frames, 8, 8);
                // Haladunk
                Progress(60);
                // Majd újra 2 színűre konvertáljuk őket
                // Ennek a műveletnek a kétszer való elvégzésére szükség van a szebb végeredmény érdekében
                frames = ImageMaster.Instance.FramesToBiColor(frames);
                // Haladunk
                Progress(80);

                // Ha a felhasználó azt választotta, hogy a megnyitott frame-eket mentsük el külön fájlokba
                if (chbSaveToFolder.Checked)
                    // Akkor elmentjük őket
                    ImageMaster.Instance.SaveFrames(frames, Path.GetDirectoryName(path));
                // A frame-eket átalakítjuk szöveges parancsokká, amiket értelmezni tud a mikrokontroller
                commands = ImageMaster.Instance.FramestoCommands(frames);
                // Ha ez sikerült, akkor
                if (commands != null)
                {
                    // Készen vagyunk
                    Progress(100);
                    // Kiírjuk alulra, hogy melyik fájlt nyitottuk meg
                    lOpenedGIF.Text = "Megnyitott GIF: " + Path.GetFileName(path).ToString();
                    // Értesítjük külön is a felhasználót az alapján, hogy történt-e külön mentés
                    if (chbSaveToFolder.Checked)
                        MessageBox.Show("A GIF megnyitása sikeres volt.\n\nAz egyes frame-ek mentésre kerültek a GIF könyvtárában lévő \"GIF images\" almappába.");
                    else
                        MessageBox.Show("A GIF megnyitása sikeres volt.");
                    // A feltöltés gombot aktiváljuk
                    btnUploadGif.Enabled = true;
                }
                // És elrentjük a ProgressBar-t
                progressBar.Visible = false;
            }
        }

        private void btnSendGif_Click(object sender, EventArgs e)
        {
            // A feltöltés gombra kattintva elindítjuk a feltöltést
            ComMaster.Instance.GIFDataOut(sp, commands);
        }

        
        private void Progress(int p)
        {
            // Metódus, ami frissíti a ProgressBar értékét és újra is rajzolja
            progressBar.Value = p;
            progressBar.Refresh();
        }
    }
}