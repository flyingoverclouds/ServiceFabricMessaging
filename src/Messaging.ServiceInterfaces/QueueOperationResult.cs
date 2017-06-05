using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.ServiceInterfaces
{
    /// <summary>
    /// Contains detailed result for a queue operation
    /// </summary>
    [DataContract]
    public class QueueOperationResult : OperationResult
    {
        /// <summary>
        /// Initialize a new QueueOperation Result
        /// </summary>
        /// <param name="success">success value</param>
        public QueueOperationResult(bool success)
            :base()
        {
            this.Success = success;
        }

        /// <summary>
        /// Initialize a new QueueOperation Result
        /// </summary>
        /// <param name="correlationId">existing correlationId to propagate</param>
        /// <param name="success">success value</param>
        public QueueOperationResult(Guid correlationId,bool success)
            : base(correlationId)
        {
            this.Success = success;
        }

    }
}
