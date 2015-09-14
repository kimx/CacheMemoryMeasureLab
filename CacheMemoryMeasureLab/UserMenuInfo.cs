using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheMemoryMeasureLab
{
    [ProtoBuf.ProtoContract]
    [Serializable]
    public partial class UserMenuInfo
    {
        public string PRG_NO { get; set; }
        public string PRG_NAME { get; set; }
        public string PRG_TYPE { get; set; }
        public string PRG_AREA { get; set; }
        public string MVC_CTRL { get; set; }
        public string MVC_ACT { get; set; }
        public string ENT_POINT { get; set; }
        public string PARAM_VAL { get; set; }
        public int ORDERNUM { get; set; }
        public string UP_PRGNO { get; set; }



    }
}
