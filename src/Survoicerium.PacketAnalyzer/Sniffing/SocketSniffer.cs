using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Survoicerium.Logging;
using Survoicerium.PacketAnalyzer.Analyzer;
using Survoicerium.PacketAnalyzer.Network;

namespace Survoicerium.PacketAnalyzer.Sniffing
{
    public class SocketSniffer
    {
        private const int BUFFER_SIZE = 1024 * 64;
        private const int MAX_RECEIVE = 100;

        private bool _isStopping;
        private Socket _socket;
        private readonly ConcurrentStack<SocketAsyncEventArgs> _receivePool;
        private readonly SemaphoreSlim _maxReceiveEnforcer = new SemaphoreSlim(MAX_RECEIVE, MAX_RECEIVE);
        private readonly BufferManager _bufferManager;

        public ILogger _logger { get; }

        private readonly BlockingCollection<TimestampedData> _outputQueue;
        private readonly PacketDataAnalyzer _analyzer;

        public SocketSniffer(NetworkInterfaceInfo nic, ILogger logger, PacketDataAnalyzer analyzer)
        {
            _logger = logger;
            _outputQueue = new BlockingCollection<TimestampedData>();
            _analyzer = analyzer;
            _bufferManager = new BufferManager(BUFFER_SIZE, MAX_RECEIVE);
            _receivePool = new ConcurrentStack<SocketAsyncEventArgs>();

            // IPv4
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            this._socket.Bind(new IPEndPoint(nic.IPAddress, 0));
            this._socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            // Enter promiscuous mode
            try
            {
                this._socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), new byte[4]);
            }
            catch (Exception ex)
            {
                _logger.Log(Severity.Debug, $"Unable to enter promiscuous mode: {ex}");
                throw;
            }
        }

        public void Start()
        {
            // Pre-allocate pool of SocketAsyncEventArgs for receive operations
            for (int i = 0; i < MAX_RECEIVE; i++)
            {
                var socketEventArgs = new SocketAsyncEventArgs();
                socketEventArgs.Completed += (e, args) => this.Receive(socketEventArgs);

                // Allocate space from the single, shared buffer
                this._bufferManager.AssignSegment(socketEventArgs);

                this._receivePool.Push(socketEventArgs);
            }

            Task.Factory.StartNew(() =>
            {
                // GetConsumingEnumerable() will wait when queue is empty, until CompleteAdding() is called
                foreach (var timestampedData in this._outputQueue.GetConsumingEnumerable())
                {
                    _analyzer.Analyze(new IPPacket(timestampedData.Data));
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
            {
                StartReceiving();
            }, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            this._isStopping = true;
        }

        private void EnqueueOutput(TimestampedData timestampedData)
        {
            if (this._isStopping)
            {
                this._outputQueue.CompleteAdding();
                return;
            }

            this._outputQueue.Add(timestampedData);
        }

        private void StartReceiving()
        {
            try
            {
                // Get SocketAsyncEventArgs from pool
                this._maxReceiveEnforcer.Wait();

                SocketAsyncEventArgs socketEventArgs;
                if (!this._receivePool.TryPop(out socketEventArgs))
                {
                    // Because we are controlling access to pooled SocketAsyncEventArgs, this
                    // *should* never happen...
                    throw new Exception("Connection pool exhausted");
                }

                // Returns true if the operation will complete asynchronously, or false if it completed
                // synchronously
                bool willRaiseEvent = this._socket.ReceiveAsync(socketEventArgs);

                if (!willRaiseEvent)
                {
                    Receive(socketEventArgs);
                }
            }
            catch (Exception ex)
            {
                // Exceptions while shutting down are expected
                if (!this._isStopping)
                {
                    Console.WriteLine(ex);
                }

                this._socket.Close();
                this._socket = null;
            }
        }

        private void Receive(SocketAsyncEventArgs e)
        {
            // Start a new receive operation straight away, without waiting
            StartReceiving();

            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    if (!this._isStopping)
                    {
                        Console.WriteLine("Socket error: {0}", e.SocketError);
                    }

                    return;
                }

                if (e.BytesTransferred > 0)
                {
                    // Copy the bytes received into a new buffer
                    var buffer = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, e.Offset, buffer, 0, e.BytesTransferred);

                    EnqueueOutput(new TimestampedData(DateTime.UtcNow, buffer));
                }
            }
            catch (SocketException ex)
            {
                _logger.Log(Severity.Debug, $"Socket error: {ex}");
            }
            catch (Exception ex)
            {
                _logger.Log(Severity.Debug, $"Error: {ex}");
            }
            finally
            {
                // Put the SocketAsyncEventArgs back into the pool
                if (!this._isStopping && this._socket != null && this._socket.IsBound)
                {
                    this._receivePool.Push(e);
                    this._maxReceiveEnforcer.Release();
                }
            }
        }
    }
}
