/// <summary>
/// Represents a client in this inversion of control implementation. Clients must 
/// implement a Serve handler for each of the services on which they depend, in 
/// order to allow for a strong typed reference to be passed.
/// <typeparam name="T">The class on which this client depends</typeparam>
public interface IClient<T> where T : CService<T>
{
    void Serve(T service);
}
