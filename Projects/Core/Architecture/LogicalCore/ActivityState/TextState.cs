using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore.ActivityState
{
    /// <summary>
    /// Состояние, передающее текст.
    /// </summary>
    public class TextState : StateBase
    {
        /// <summary>
        /// Содержание состояния.
        /// </summary>
        public string State { get; set; }
    }
}
