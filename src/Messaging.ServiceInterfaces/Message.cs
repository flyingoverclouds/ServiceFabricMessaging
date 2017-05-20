using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.ServiceInterfaces
{

    /// <summary>
    /// Internal class use to persist message data.
    /// </summary>
    [DataContract]
    public class Message
    {
        /// <summary>
        /// unique Id of message. 
        /// Set by the queue when message is put. It will never changed as long as message is in the system
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Date of insertion in the queue. After set, will never change.
        /// </summary>
        [DataMember]
        public DateTime InsertionDate{ get; set; }

        /// <summary>
        /// Expiration date of the messgae in the syustem. 
        /// If this deadline is reached, message will be automatically destroyed from the system 
        /// and will never be presented to the client app.
        /// </summary>
        [DataMember]
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// If message was not deleted after a Get, as soon as this date is readhed, 
        /// the message will be visbile again (reinserted in the main queue).
        /// </summary>
        [DataMember]
        public DateTime NextVisibleDate { get; set; }

        /// <summary>
        /// Count queue action. Increment each time message is poped (Get) from the queue. 
        /// Used by the client app to implemented poison message detection.
        /// </summary>
        [DataMember]
        public int DequeueCount { get; set; }

        /// <summary>
        /// Content of the message.
        /// </summary>
        [DataMember]
        public string Payload { get; set; }

        /// <summary>
        /// Id of client app (set by client app, used for corellation in debuug, analysis, ...)
        /// never used by the internals feature, only added in log & trace.
        /// </summary>
        [DataMember]
        public string ClientId { get; set; }

        /// <summary>
        /// Id of the GetMessage action when message was poped from the queue. 
        /// this receipt is used by the DeleteMEssage/UpdateMessage
        /// </summary>
        [DataMember]
        public string PopReceipt { get; set; }

    }
}
    