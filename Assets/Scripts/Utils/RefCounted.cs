
public abstract class RefCounted
{
    private int _refCount = 0;

    public void AddReference()
    {
        _refCount++;
    }

    public void Release()
    {
        _refCount--;
        if (_refCount == 0)
        {
            Destroy();
        }
    }

    public abstract void Destroy();
}
