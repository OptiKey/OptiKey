using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JuliusSweetland.OptiKey.StandardPlugins
{
    public class TranslationKey
    {
        public void TRANSLATE(string text)
        {
            Process.Start(text);
        }
    }
}
