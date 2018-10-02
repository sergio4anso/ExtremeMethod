using UnityEngine;
using System;

/// <summary>
/// Base class for the implementation of the inversion of control principle.
/// This class is used to represent a service, and includes the code required to
/// inject itself into its clients.
/// </summary>
/// <typeparam name="T">The class that will be used as a service. It provides the name and the signatures</typeparam>
public abstract class CService<T> : MonoBehaviour where T : CService<T>
{
    /// <summary>
    /// In order to work as a service a unique instance is required
    /// </summary>
    private static CService<T> Instance
    {
        get;
        set;
    }

    /// <summary>
    /// Initialization of the unique instance. If the instance is already initialized,
    /// destroys this gameObject.
    /// </summary>
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// When destroyed, if this instance was the singleton, performs cleanup.
    /// Notifies its clients that the service no longer exists.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            if (Serve != null)
            {
                Serve(null);
            }

            ready = false;
            Instance = null;
        }
    }

    /// <summary>
    /// Event handler, used to notify clients about this service's availability
    /// </summary>
    private static Action<T> Serve;

    /// <summary>
    /// A reference to the current implementation of this service
    /// </summary>
    private static T typedReference;

    /// <summary>
    /// Flag used to indicate whether the service is ready or not.
    /// </summary>
    private static bool ready;

    /// <summary>
    /// Adds a client so that it gets notified when the service is ready. If the service is
    /// ready, it gets notified straight away.
    /// </summary>
    /// <param name="serveHandler">An event delegate that will be called when the service availability changes</param>
    public static void AddClient(IClient<T> client)
    {
        if (ready)
        {
            client.Serve(typedReference);
        }

        Serve -= client.Serve; // Prevent double calls
        Serve += client.Serve;
    }

    /// <summary>
    /// Removes a client so that it does not get notified anymore
    /// </summary>
    /// <param name="serveHandler">An event delegate that will be removed from the delegate</param>
    public static void RemoveClient(IClient<T> client)
    {
        Serve -= client.Serve;
    }

    /// <summary>
    /// Notifies all the clients about the availability of the service and injects a reference
    /// </summary>
    protected void ServeClients(T reference)
    {
        if (reference != null)
        {
            if ((CService<T>)reference == this && !ready)
            {
                typedReference = reference;
                ready = true;

                if (Serve != null)
                {
                    Serve(reference);
                }
            }
        }
    }
}
