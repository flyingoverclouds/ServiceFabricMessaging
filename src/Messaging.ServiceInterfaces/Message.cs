using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.ServiceInterfaces
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public DateTime MessageDate{ get; set; }

        [DataMember]
        public string Payload { get; set; }
    }
}
