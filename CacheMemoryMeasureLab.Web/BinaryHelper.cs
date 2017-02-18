using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CacheMemoryMeasureLab.Web
{
    /// <summary>
    /// 二進位序列化
    /// ps:會使用Base64String method
    /// </summary>
    public class BinaryHelper
    {
        public static string ObjectToString<T>(T value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var binaryWriter = new BinaryFormatter();
                binaryWriter.Serialize(stream, value);
                return Convert.ToBase64String(stream.ToArray());
            }

        }

        public static T StringToObject<T>(string strValue) where T : new()
        {
            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(strValue)))
            {
                var binaryWriter = new BinaryFormatter();
                return (T)binaryWriter.Deserialize(stream);
            }
        }

        public static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        public static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }
    }
}
