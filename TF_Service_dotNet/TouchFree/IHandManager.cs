using System.Collections.Generic;
using Leap;

namespace Ultraleap.TouchFree.Library
{
    public interface IHandManager
    {
        List<Vector> RawHandPositions { get; }
    }
}
