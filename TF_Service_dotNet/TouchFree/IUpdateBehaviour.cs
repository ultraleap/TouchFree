namespace Ultraleap.TouchFree.Library
{
    public interface IUpdateBehaviour
    {
        public delegate void UpdateEvent();
        public event UpdateEvent OnUpdate;
        public event UpdateEvent OnSlowUpdate;
    }
}
