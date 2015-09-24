using System.Collections.Generic;
using System.Linq;
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
            var newPointToKeyValueMap = new Dictionary<Rect, KeyValue>();
            foreach (int zOrder in pointToKeyValueMapsByZOrder.Keys)
            {
                var currentMap = pointToKeyValueMapsByZOrder[zOrder];
                if (currentMap == null) continue;

                foreach (var rect in currentMap.Keys)
                {
                    var localRect = rect; //Access to modified closure
                    var localZOrder = zOrder; //Access to modified closure
                    if (!pointToKeyValueMapsByZOrder.Keys
                            .Where(z => z > localZOrder) //All z-orders higher than the current z-order
                            .Select(k => pointToKeyValueMapsByZOrder[k]) //Select all mappings
                            .SelectMany(m => m.Keys) //Flat map all the rects from the mappings
                            .Any(r => r.IntersectsWith(localRect))) //Check if any of the rects intersect with the current 'rect' variable
                    {
                        newPointToKeyValueMap.Add(rect, currentMap[rect]);
                    }
                }

                PointToKeyValueMap = newPointToKeyValueMap;
            }
        }

        #endregion
    }
}
