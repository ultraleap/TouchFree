
namespace Ultraleap.TouchFree.Tooling.Configuration
{
    public struct MaskingData
    {
        public float lower;
        public float upper;
        public float right;
        public float left;

        public MaskingData(float _lower, float _upper, float _right, float _left)
        {
            lower = _lower;
            upper = _upper;
            right = _right;
            left = _left;
        }
    }
}