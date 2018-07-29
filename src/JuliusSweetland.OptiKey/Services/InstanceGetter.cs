using JuliusSweetland.OptiKey.UI.Controls;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services
{
    public class InstanceGetter
    {
        public List<Key> allKeys { get; set; }

        private static InstanceGetter instance;

        public static InstanceGetter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InstanceGetter();
                }
                return instance;
            }
        }
    }
}

