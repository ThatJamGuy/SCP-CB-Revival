using System;

namespace Unity.Burst
{
    /// <summary>
    /// Can be used to specify that a parameter to a function, a field of a struct, a struct, or a function return will not alias. (Advanced - see User Manual for a description of Aliasing).
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.ReturnValue)]
    public class NoAliasAttribute : Attribute
    {
    }

#if UNITY_BURST_EXPERIMENTAL_ALIAS_ATTRIBUTE
    /// <summary>
    /// The opposite of [NoAlias], used for job structs whose fields do not conform to the default alias rules.
    /// This only works for structs and fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class AliasAttribute : Attribute
    {
    }
#endif
}
