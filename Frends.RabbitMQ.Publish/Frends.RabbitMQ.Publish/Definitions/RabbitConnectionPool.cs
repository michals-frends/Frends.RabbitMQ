using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Frends.RabbitMQ.Publish.Definitions;

public static class RabbitConnectionPool
{
    private static readonly ConcurrentDictionary<string, Lazy<RabbitSession>> _connectionPool = new();
    
    public static RabbitSession GetConnection(string host, Func<ConnectionFactory, IConnection> factory)
    {
        Console.WriteLine($"pool contains key {host}? {_connectionPool.ContainsKey(host)}");
        
        var pooledConnection = _connectionPool.GetOrAdd(host, (s) => new Lazy<RabbitSession>(() => new RabbitSession(factory)));
        
        return pooledConnection.Value;
    }

    private static Task PurgeConnectionPool = Task.Factory.StartNew(() =>
    {
        DateTime? emptySince = null;
        
        while (true)
        {
            Console.WriteLine("purging connection pool");
            if (_connectionPool.IsEmpty)
            {
                if (emptySince.HasValue && emptySince.Value < DateTime.Now.AddSeconds(-20))
                {
                    break;
                }
                
                emptySince = emptySince ?? DateTime.Now;
                Thread.Sleep(1000);
            }
            else
            {
                emptySince = null;
            }
            
            var conn = _connectionPool
                .Where(pool => pool.Value.Value.LastAccess < DateTime.Now.AddSeconds(-10))
                .Select(pair => new { pair.Key, pair.Value })
                .ToList();
            if (conn.Any())
            {
                var first = conn.First();
                Console.WriteLine("purging old connections");
                _connectionPool.TryRemove(first.Key, out var session);
                session.Value.Dispose();
            }
            else
            {
                Console.WriteLine("sleeping as nothing to purge");
                Thread.Sleep(1000);
            }
        }
    });
}

public class RabbitSession : IDisposable
{
    private Lazy<IConnection> LazyConnection { get; set; }
    private Lazy<IModel> LazyModel { get; set; }
    
    private ThreadLocal<Lazy<IModel>> LazyModelThreadLocal { get; set; }

    public DateTime LastAccess { get; private set; }
    
    public IConnection Connection {
        get
        {
            Console.WriteLine("getting connection");
            LastAccess = DateTime.Now;
            return LazyConnection.Value;
        }
    }
    public IModel Model {
        get
        {
            Console.WriteLine("getting model");
            LastAccess = DateTime.Now;
            //return LazyModel.Value;
            return LazyModelThreadLocal.Value.Value;
        }
    }
    
    public RabbitSession(Func<ConnectionFactory, IConnection> factory)
    {
        LazyConnection = new Lazy<IConnection>(factory(new ConnectionFactory()));
        LazyModel = new Lazy<IModel>(() =>
        {
            Console.WriteLine("creating new model");
            return LazyConnection.Value.CreateModel();
        });

        LazyModelThreadLocal = new ThreadLocal<Lazy<IModel>>(() => new Lazy<IModel>(() =>
        {
            Console.WriteLine("creating new threaded model");
            return LazyConnection.Value.CreateModel();
        }));
    }

    public void Dispose()
    {
        Console.WriteLine("Disposing RabbitSession");
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (LazyModel.IsValueCreated) LazyModel.Value.Dispose();
            if (LazyConnection.IsValueCreated) LazyConnection.Value.Dispose();
        }
    }
}

