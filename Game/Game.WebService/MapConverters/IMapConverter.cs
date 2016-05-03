using Game.WebService.Model;

namespace Game.WebService.MapConverters
{
    public interface IMapConverter
    {
        EnMapData Convert(EnMapData map);
    }
}
