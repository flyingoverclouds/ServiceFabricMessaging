using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.ServiceInterfaces
{
    /// <summary>
    /// Abstract class for queue and message operation result
    /// </summary>
    public abstract class OperationResult
    {
        /// <summary>
        /// Initialiaze a new OperationREsult instance with random value for OperaitonId & CorrelationId
        /// </summary>
        public OperationResult()
        {
            this.CorrelationId = Guid.NewGuid();
            this.OperationId = Guid.NewGuid();
        }

        /// <summary>
        /// Initialiaze a new OperationREsult instance with random value for OperaitonId 
        /// </summary>
        /// <param name="correlationId"></param>
        public OperationResult(Guid correlationId) 
            : this()
        {
            this.CorrelationId = correlationId;
        }


        /// <summary>
        /// Unique Id of operation. 
        /// </summary>
        public Guid OperationId { get; private set; }

        /// <summary>
        /// Id used for trace correlation
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Generic result : true=request succeeded , false : partial success of faileure (see detailed subclasses)
        /// </summary>
        public bool Success { get; protected set; }

    }
}
