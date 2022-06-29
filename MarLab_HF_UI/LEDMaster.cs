using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarLab_HF_UI
{
    class LEDMaster
    {
        // Delegate a LED-ek frissítésére, hiszen a frissítés egy másik Thread-ből történik
        delegate void Safe_UpdateLEDs_Delegate(TextBox tb, Color color);
        // A saját példány változója
        public static LEDMaster theLEDMaster;
        // Property a saját példányról
        public static LEDMaster Instance
        {
            get { return theLEDMaster; }
        }
        // A saját példány inicializlása
        public static void Init(MainForm form)
        {
            theLEDMaster = new LEDMaster();
        }

        public void UpdateLEDs(TextBox tb, Color color)
        {
            // Metódus, ami frissíti az adott textbox színét az adott színre

            // Ha másik Thread-ből akarják elérni a textbox-ot
            if (tb.InvokeRequired)
                // Akkor invoke-olunk
                tb.BeginInvoke(new Safe_UpdateLEDs_Delegate(UpdateLEDs), new object[] { tb, color });
            else
                // Ellenkező esetben beállítjuk a színt
                tb.BackColor = color;
        }

        public void ResetLEDs(List<TextBox> tbs)
        {
            // Metódus, ami Reset-eli a LED-eket

            // Egyszerűen csak végigmegyünk a textbox-okon
            foreach (TextBox tb in tbs)
                // És üresbe állítjuk a színüket
                UpdateLEDs(tb, Color.Empty);
        }
    }
}
