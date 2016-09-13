using System;

namespace AutoFactory
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PerInstanceAttribute : Attribute
    {
    }
}
