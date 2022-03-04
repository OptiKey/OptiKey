// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Models
{
    public class Map<T1, T2>
    {
        public void Add(T1 t1, T2 t2)
        {
            Forward.Add(t1, t2);
            Reverse.Add(t2, t1);
        }

        public readonly Dictionary<T1, T2> Forward = new Dictionary<T1, T2>();
        public readonly Dictionary<T2, T1> Reverse = new Dictionary<T2, T1>();
    }
}
