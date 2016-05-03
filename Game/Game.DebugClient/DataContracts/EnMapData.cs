using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Game.DebugClient.Properties;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class EnMapData
    {
        [DataMember] public int Height;

        [DataMember] public List<string> Rows;
        [DataMember] public int Width;

    }
}