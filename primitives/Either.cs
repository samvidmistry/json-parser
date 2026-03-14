namespace Primitives;

public class Either<T>
{
    private readonly T? obj;
    private readonly Error? err;

    public Either(T obj)
    {
	if (obj is null)
	{
	    throw new ArgumentNullException("Object cannot be null for this constructor!");
	}
	
	this.obj = obj;
	this.err = null;
    }

    public Either(Error err)
    {
	this.err = err;
	this.obj = default(T);
    }

    public T GetObject()
    {
	if (this.err is not null)
	{
	    throw new InvalidOperationException("Either contains an Error!");
	}

	return this.obj!;
    }

    public Error? GetError()
    {
	return this.err;
    }
}
