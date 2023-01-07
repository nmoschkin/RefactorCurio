using DataTools.Code.CS.Filtering;

using Newtonsoft.Json;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CSRefactorCurio.Options
{
    public class FilterProfile : CodeFilterOptions, ISerializable
    {
        public string ProfileName { get; set; }

        [JsonIgnore]
        public bool IsDefault => ProfileName == "[DEFAULT]";

        public FilterProfile()
        {
        }

        public FilterProfile(SerializationInfo info, StreamingContext context)
        {
            var json = info.GetString("blob");
            JsonConvert.PopulateObject(json, this);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("blob", JsonConvert.SerializeObject(this));
        }
    }
}