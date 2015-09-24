using System.Collections.Generic;
using System.Windows;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class CompositePointToKeyValueMap : BindableBase
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<int, Dictionary<Rect, KeyValue>> pointToKeyValueMapsByZOrder = new Dictionary<int, Dictionary<Rect, KeyValue>>();
        private Dictionary<Rect, KeyValue> pointToKeyValueMap; 

        #endregion

        #region Properties

        public Dictionary<Rect, KeyValue> this[int zOrder]
        {
            set
            {
                if (pointToKeyValueMapsByZOrder.ContainsKey(zOrder))
                {
                    Log.DebugFormat("Updating PointToKeyValueMap for z-order {0} (map {1})", zOrder, value == null ? "is null" : "has values");
                    pointToKeyValueMapsByZOrder[zOrder] = value;
                }
                else
                {
                    Log.DebugFormat("Adding new PointToKeyValueMap for z-order {0} (map {1})", zOrder, value == null ? "is null" : "has values");
                    pointToKeyValueMapsByZOrder.Add(zOrder, value);
                }

                CreatePointToKeyValueMap();
            }
        }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            get { return pointToKeyValueMap; }
            set { SetProperty(ref pointToKeyValueMap, value); }
        }

        #endregion

        #region Private Methods

        private void CreatePointToKeyValueMap()
        {

            PointToKeyValueMap = ;
        }

        #endregion
    }
}
