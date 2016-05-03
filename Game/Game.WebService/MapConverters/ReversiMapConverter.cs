using System;
using System.Linq;
using Game.WebService.Model;

namespace Game.WebService.MapConverters
{
    public class ReversiMapConverter : IMapConverter
    {
        public EnMapData Convert(EnMapData map)
        {
            var rows = map.Rows.ToList();
            map.Rows.Clear();

            foreach (var row in rows)
            {
                var convertedRow =
                    row.Replace(".", "*")
                    .Replace("a", "0")
                    .Replace("A", "0")
                    .Replace("b", "1")
                    .Replace("B", "1");

                map.Rows.Add(convertedRow);
            }

            return map;
        }
    }
}