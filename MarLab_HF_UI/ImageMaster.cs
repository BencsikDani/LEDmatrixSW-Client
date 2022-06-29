using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MarLab_HF_UI
{
    class ImageMaster
    {
        // A saját példány változója
        public static ImageMaster theImageMaster;
        // Property a saját példányról
        public static ImageMaster Instance
        {
            get { return theImageMaster; }
        }
        // A saját példány inicializlása
        public static void Init(MainForm form)
        {
            theImageMaster = new ImageMaster();
        }

        public string OpenGIF()
        {
            // Metódus, ami bekéri a felhasználótól a megnyitni kívánt GIF elérési útját
            string path = string.Empty;
            // OpenFileDialog-ot használunk
            OpenFileDialog ofd = new OpenFileDialog();
            // Alapvetően GIF-eket keresünk
            ofd.Filter = "GIF fájl (*.gif)|*.gif|Minden fájl (*.*)|*.*";
            // Alapból a "GIF fájl (*.gif)" legyen kiválasztva
            ofd.FilterIndex = 1;
            // Emlékezzen az előző megnyitás mappájára
            ofd.RestoreDirectory = true;
            // Hogyha a felhasználó OK-t nyomott
            if (ofd.ShowDialog() == DialogResult.OK)
                // Beállítjuk az elérési utat
                path = ofd.FileName;
            // És visszatérünk vele
            return path;
        }

        public Image[] GetGIFFrames(string path)
        {
            // Metódus, ami adott elérési úton levő GIF-et frame-ekre szabdal

            // Megnyitjuk magát a GIF-et
            Image gif = Image.FromFile(path);
            // Megnézzük, hány frame-ből áll
            int frameNumber = gif.GetFrameCount(FrameDimension.Time);
            // Egy ekkora méretű Image tömböt létrehozunk
            Image[] frames = new Image[frameNumber];

            // Végigmegyünk a GIF frame-jein
            for (int i = 0; i < frameNumber; i++)
            {
                gif.SelectActiveFrame(FrameDimension.Time, i);
                // És beklónozzuk őket a tömbünkbe
                frames[i] = (Image)gif.Clone();
            }
            // Végül visszatérünk a tömbbel
            return frames;
        }

        public void SaveFrames(Image[] frames, string folderpath)
        {
            // Metódus, ami elmenti az összes frame-et egy külön létrehozott mappába PNG-ként.

            // Ha még nem létezik ez a mappa
            if (!Directory.Exists(folderpath + @"\GIF images"))
                // Akkor létre kell hoznunk
                Directory.CreateDirectory(folderpath + @"\GIF images");
            // Ezek után végigmegyünk az összes frame-en
            for (int i = 0; i < frames.Length; i++)
                // És elmentjük őket a sorszámuk szerinti néven
                frames[i].Save(folderpath + @"\GIF images\" + i + ".png", ImageFormat.Png);
        }

        public Bitmap ResizeFrame(Image frame, int w, int h)
        {
            // Metódus, ami átméretez egy frame-et a megadott méretűre

            // Letrehozzuk a Bitmap-et, amiben tárolni fogjuk az átméretezett képet
            Bitmap b = new Bitmap(w, h);
            // Beállítjuk a felbontását az eredeti kép szerint (felbontás = PPI)
            b.SetResolution(frame.HorizontalResolution, frame.VerticalResolution);

            // Létrehozunk egy grafikát, amire rajzolni fogunk
            Graphics g = Graphics.FromImage(b);
            // Végzünk pár apró beállítást annak érdekében, hogy a legjobb minőségű végeredményt kapjuk
            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Létrehozunk egy attribútumot, amivel a képek sarkán megjelenő pixelhibák kiküszöbölhetők
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetWrapMode(WrapMode.TileFlipXY);

            // Újrarajzoljuk az eredeti képet a grafikára
            g.DrawImage(frame, new Rectangle(0, 0, w, h), 0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel, attributes);

            // Visszatérünk a Bitmap-pel, amire a grafika rajzolt
            return b;
        }

        public Image[] ResizeGIFframes(Image[] frames, int w, int h)
        {
            // Metódus, ami egyszerűen több kép átméretezésében segít

            // Meghívja az összes frame-re a ResizeFrame()-et
            for (int i = 0; i < frames.Length; i++)
                frames[i] = ResizeFrame(frames[i], w, h);
            // Majd visszatér az átméretezett frame-ekkel
            return frames;
        }

        public Bitmap ToGreyScale(Bitmap b)
        {
            // Metódus, ami szürkeárnyalatossá konvertál egy képet

            // Végigmegyünk...
            for (int i = 0; i < b.Width; i++)
            {
                // ... az összes pixelen
                for (int j = 0; j < b.Height; j++)
                {
                    // Vesszük az adott pixel színét
                    Color c = b.GetPixel(i, j);
                    // Létrehozunk belőle egy szürkeárnyalatos változatot
                    // Ezt az RGB csatornák keverésével lehet elérni
                    // A formulát az interneten találam
                    int greyScale = (int)((0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B));
                    // Beállítjuk a pixel minden csatornáját ugyanarra
                    b.SetPixel(i, j, Color.FromArgb(c.A, greyScale, greyScale, greyScale));
                }
            }
            // Végül visszatérünk a teljes képpel
            return b;
        }

        public Image[] FramesToGreyScale(Image[] frames)
        {
            // Metódus, ami egyszerűen több kép szürkeárnyalatossá alakításában segít

            // Meghívja az összes frame-re a ToGreyScale()-t
            for (int i = 0; i < frames.Length; i++)
                frames[i] = ToGreyScale((Bitmap)frames[i]);
            // Majd visszatér a szürkeárnyalatos frame-ekkel
            return frames;
        }

        public Bitmap ToBiColor(Bitmap b)
        {
            // Metódus, ami 2 színűvé (fekete vagy fehér) konvertál egy képet

            // Végigmegyünk...
            for (int i = 0; i < b.Width; i++)
            {
                // ... az összes pixelen
                for (int j = 0; j < b.Height; j++)
                {
                    // Vesszük az adott pixel színét
                    Color c = b.GetPixel(i, j);
                    // Létrehozunk belőle egy szürkeárnyalatos változatot
                    int greyScale = (int)((0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B));
                    // Megnézzük, hogy ez az árnyalat a feketéhez vagy a fehérhez van-e közelebb
                    // És ez alapján beállítjuk
                    greyScale = greyScale <= 127 ? 0 : 255;
                    // Beállítjuk a pixel minden csatornáját ugyanarra
                    b.SetPixel(i, j, Color.FromArgb(c.A, greyScale, greyScale, greyScale));
                }
            }
            // Végül visszatérünk a teljes képpel
            return b;
        }

        public Image[] FramesToBiColor(Image[] frames)
        {
            // Metódus, ami egyszerűen több kép 2 színűvé alakításában segít

            // Meghívja az összes frame-re a ToBiColor()-t
            for (int i = 0; i < frames.Length; i++)
                frames[i] = ToBiColor((Bitmap)frames[i]);
            // Majd visszatér a 2 színű frame-ekkel
            return frames;
        }

        public string FrameToCommand(Image frame)
        {
            // Metódus, ami egy frame alapján legenerál egy parancsot, amit a mikrokontroller tud olvasni

            // Létrehozunk egy üres string-et
            string str = string.Empty;
            // Az első karaktere egy '/' jel, ami azt jelzi, hogy parancs érkezett
            str += '/';
            // Majd végigmegyünk a 8×8-as frame pixelein
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // Kivesszük a színét
                    Color c = ((Bitmap)frame).GetPixel(j, i);
                    // És ha ez fekete, akkor 1-esre állítjuk az adott bit-et a parancsban
                    // (A textbox-oknál ez fogja jelenteni a kék színű hátteret, tehát a sötétebbet)
                    if (c.R == 0 && c.G == 0 && c.B == 0)
                        str += '1';
                    // Ha pedig fehér, akkor 0-ba állítjuk az adott bit-et a parancsban
                    // (A textbox-oknál ez fogja jelenteni az "üres" (szürke) színű hátteret, tehát a világosabbat)
                    else if (c.R == 255 && c.G == 255 && c.B == 255)
                        str += '0';
                }
            }
            // Nem felejtjük le a sorvége karaktereket és a string végjelet
            str += "\r\n\0";
            // Végül visszatérünk a string-gel
            return str;
        }

        public string[] FramestoCommands(Image[] frames)
        {
            // Metódus, ami egyszerűen több kép paranccsá alakításában segít

            // Létrehozunk egy tároló string tömböt, hiszen itt nem ugyanolyan típusú a ki- és bemenetünk
            string[] strs = new string[frames.Length];
            // Meghívjuk az összes frame-re a FrameToCommand()-ot
            for (int i = 0; i < frames.Length; i++)
                // Elmentjük a parancsot
                strs[i] = FrameToCommand(frames[i]);
            // Majd a végén visszatérünk a parancsokkal
            return strs;
        }
    }
}
