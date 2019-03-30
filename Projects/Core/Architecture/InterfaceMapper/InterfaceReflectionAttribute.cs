using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace OnUtils.Architecture.InterfaceMapper
{
    /// <summary>
    /// </summary>
    public sealed class InterfaceReflectedMemberAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public InterfaceReflectedMemberAttribute(string memberTypeFullName, string memberSignature)
        {
            this.MemberSignature = memberSignature;
            this.MemberTypeFullName = memberTypeFullName;
        }

        /// <summary>
        /// </summary>
        public string MemberSignature { get; private set; }

        /// <summary>
        /// </summary>
        public string MemberTypeFullName { get; private set; }
    }
}
