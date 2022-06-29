using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarLab_HF_UI
{
    // A soros kommunikációért felelős osztály
    class ComMaster
    {
        // Változó 1 db aktuálisan számolt parancs tárolására
        string command = string.Empty;
        // A saját példány változója
        public static ComMaster theComMaster;
        // Property a saját példányról
        public static ComMaster Instance
        {
            get { return theComMaster; }
        }
        // A saját példány inicializlása
        public static void Init(MainForm form)
        {
            theComMaster = new ComMaster();
        }

        public void OpenCom(SerialPort sp, string port, int baud, int adat, StopBits stop, string paritas, string hs)
        {
            //Metódus, amely megnyitja a soros kommunikációt abeállított adatok alapján

            try
            {
                // Port neve
                sp.PortName = port;
                // Baud rate
                sp.BaudRate = baud;
                // Adatbitek száma
                sp.DataBits = adat;
                // Stopbitek száma
                sp.StopBits = stop;
                // Megfelelő paritás beállítása
                if (paritas == "Nincs")
                    sp.Parity = Parity.None;
                else if (paritas == "Páratlan (Odd)")
                    sp.Parity = Parity.Odd;
                else if (paritas == "Páros (Even)")
                    sp.Parity = Parity.Even;
                else if (paritas == "Fix 1 (Mark)")
                    sp.Parity = Parity.Mark;
                else if (paritas == "Fix 0 (Space)")
                    sp.Parity = Parity.Space;
                // Megfelelő Handshake beállítása
                if (hs == "Nincs")
                    sp.Handshake = Handshake.None;
                else if (hs == "XON/XOFF")
                    sp.Handshake = Handshake.XOnXOff;
                else if (hs == "RTS")
                    sp.Handshake = Handshake.RequestToSend;
                else if (hs == "XON/XOFF + RTS")
                    sp.Handshake = Handshake.RequestToSendXOnXOff;

                // A kommunikáció megnyitása
                sp.Open();
                // A bufferek törlése biztonsági okokból
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
            }
            catch (Exception e)
            {
                // Hogyha ez valamiért nem sikerült, azt tudatjuk a felhasználóval kivétel formájában
                MessageBox.Show(e.ToString());
            }
        }

        public void CloseCom(SerialPort sp)
        {
            // Metódus, amely lezárja a soros kommunikációt
            try
            {
                // Biztonsági okokból töröljük a buffereket
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                // Lezárjuk a kommunikációt
                sp.Close();
            }
            catch (Exception e)
            {
                // Hogyha ez valamiért nem sikerült, azt tudatjuk a felhasználóval kivétel formájában
                MessageBox.Show(e.ToString());
            }
        }

        public void DataIn(List<TextBox> tbs, SerialPort sp)
        {
            // Metódus, ami a bejövő parancsokat értelmezi és ez alapján frissíti a LED-eket

            // Hogyha még nem jött be egy parancsnyi adat
            if (command.Length < 68)
                // Akkor beolvassuk a buffer tartalmát
                command += sp.ReadExisting();
            // Ha már bejött pont egy parancsnyi adat és az parancs is
            if (command.Length == 68 && command[0] == '/')
            {
                // Akkor végigmegyünk a parancson
                for (int i = 1; i < 65; i++)
                {
                    // És ha az adott bit 0 értékű
                    if (command[i] == '0')
                        // Akkor az adott textbox-ot üresre színezzük 
                        LEDMaster.Instance.UpdateLEDs(tbs[i-1], Color.Empty);
                    // Ha viszont az adott bit 1 értékű
                    else if (command[i] == '1')
                        // Akkor az adott textbox-ot kékre színezzük
                        LEDMaster.Instance.UpdateLEDs(tbs[i-1], Color.Blue);
                }
                // Kiürítjük a command változót
                command = string.Empty;
                // Illetve a bemeneti buffert is a biztonság kedvéért
                sp.DiscardInBuffer();
            }
            // Hogyha pedig valamiért több adat jött be, mint amennyi egy parancs lenne
            if (command.Length > 68)
            {
                // Akkor ezt a hibás adatot eldobjuk
                command = string.Empty;
                // Hiszen sajnos nincs idő a jelenlegi alkalmazásban foglalkozni az újraküldéssel
                sp.DiscardInBuffer();
            }
        }

        public void DataOut(SerialPort sp, string s)
        {
            // Metódus, ami kiküldi az adott string-et az adott Portra

            // Hogyha a kommunikáció nyitva van
            if (sp.IsOpen)
            {
                // A biztonság kedvéért ürítjük a kimeneti buffert
                sp.DiscardOutBuffer();
                // És kiírjük Az adott string-et, amit az AppendStringTo68() megfelelően formáz
                sp.Write(AppendStringTo68(s));
            }
        }

        public void GIFDataOut(SerialPort sp, string[] commands)
        {
            // Metódus, ami feltölti egy teljes GIF adatait a mikrokontrollerre

            // Ha a kommunikáció nyitva van és nincs túl sok frame-ünk
            if (sp.IsOpen && commands.Length <= 250)
            {
                // Kiküldünk egy haszontalan karaktert
                // Ez azért szükséges, hogy a mikrokontroller ha lemarad az első adat érkezéséről,
                // akkor az a lemaradt adat inkább ez legyen
                DataOut(sp, "\r\n\0");
                // Kiküldjük az első parancsot, ami megmondja, mennyi frame-et fog kapni a mikrokontroller
                DataOut(sp, "/" + commands.Length.ToString() + "\r\n\0");
                // Várunk egy pár ms-ot, hogy a mikrokontroller ne maradjon le semmiről
                // (Erre a "gyógymódra" hosszú idő volt rájönni...)
                Thread.Sleep(100);
                // Majd végigmegyünk a parancsokon
                for (int i = 0; i < commands.Length; i++)
                {
                    // Amik már megfelelően vannak formázva, ezért csak simán ki lehet őket küldeni
                    sp.Write(commands[i]);
                    // Itt is hagyjuk egy picit dolgozni a Mikrokontrollert
                    Thread.Sleep(10);
                }
            }
            // Ha túl nagy volt a GIF
            else if (commands.Length > 250)
                // Akkor erről értesítjük a felhasználót
                MessageBox.Show("HIBA!\nA GIF nem lett feltöltve, hiszen sajnos túl sok frame-et tartalmaz!");
            // Ha pedig bármi más baj volt
            else
                // Akkor is
                MessageBox.Show("HIBA!\nA GIF nem lett feltöltve!");
        }

        public string AppendStringTo68(string str)
        {
            // Metódus, ami megfelelően formázza a küldendő string-et.

            // Mivel az én alkalmazásomban legtöbbször 68 méretű adatokat kell küldeni/fogadni,
            // ezért úgy gondoltam, hogy ezt egységesítem és nem bájtonként küldök/fogadok.

            // Ha a parancsunk még nem 68 hosszú
            if (str.Length < 68)
                // Akkor folyamatosan rakosgatunk a végére egy végjel karaktert
                while (str.Length < 68)
                    // Ezt a karaktert a soros kommunikáció valóban +1 karakternek érzékeli
                    str += "\0";
            // Majd a végén visszatérünk a megfelelő hosszúságú string-gel
            return str;
        }
    }
}