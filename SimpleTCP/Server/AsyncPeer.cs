using SimpleTCP.Extensions;
using SimpleTCP.Interfaces;
using SimpleTCP.Models;
using SimpleTCP.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;

namespace SimpleTCP.Server
{
    public abstract class AsyncPeer<TCommandType> where TCommandType : struct, Enum
    {
        // Size of receive buffer.
        protected const int BufferSize = 1024;

        private Dictionary<TCommandType, Tuple<Type, IMessageDescriptor<TCommandType>>> _messageHandler;
        public Action<NetConnection> OnRemoteSocketDisconnected;

        public AsyncPeer(bool enableAutoRegisterCallback)
        {
            _messageHandler = new Dictionary<TCommandType, Tuple<Type, IMessageDescriptor<TCommandType>>>();
            AutoAddMessageHandler();
            if(enableAutoRegisterCallback) AutoRegisterCallbackTo();
        }

        public void AddMessageHandler(TCommandType key, Tuple<Type, IMessageDescriptor<TCommandType>> value)
        {
            if (!_messageHandler.ContainsKey(key))
            {
                _messageHandler.Add(key, value);
            }
        }

        protected void AutoAddMessageHandler()
        {
            List<Type> messages = TypeUtils.GetAllSubTypes(typeof(IMessage<TCommandType>));

            foreach (Type msg in messages)
            {
                TCommandType cmdType = (TCommandType)msg.GetProperty(nameof(IMessage<TCommandType>.Id)).GetValue(Activator.CreateInstance(msg), null);

                Type descriptorType = typeof(MessageDescriptor<,>).MakeGenericType(msg, cmdType.GetType());
                IMessageDescriptor<TCommandType> descriptor = (IMessageDescriptor<TCommandType>)Activator.CreateInstance(descriptorType);
                if (!_messageHandler.ContainsKey(cmdType))
                {
                    _messageHandler.Add(cmdType, Tuple.Create(msg, descriptor));
                }
            }
        }

        /// <summary>
        /// Register a global action for one message type.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback">callback action</param>
        /// <param name="commandType">the command type</param>
        public void RegisterCallbackTo<TEvent>(Action<TEvent, NetConnection> callback, TCommandType commandType) where TEvent : class, IMessage<TCommandType>
        {
            if (_messageHandler.TryGetValue(commandType, out Tuple<Type, IMessageDescriptor<TCommandType>> handler))
            {
                handler.Item2.RegisterCallback(callback);
            }
        }

        /// <summary>
        /// Register a global action for one message type.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback">callback action</param>
        /// <param name="commandType">the command type</param>
        protected void AutoRegisterCallbackTo()
        {
            List<Type> registers = TypeUtils.GetAllTypes();

            foreach (TypeInfo register in registers)
            {
                foreach (MethodInfo method in register.DeclaredMethods)
                {
                    foreach (ParameterInfo parameter in method.GetParameters())
                    {
                        Type type = parameter.ParameterType;
                        if (type.IsAssignableTo(typeof(IMessage<TCommandType>)))
                        {  
                            Delegate callback = method.CreateDelegate(Expression.GetActionType(type, typeof(NetConnection)));
                            TCommandType cmdType = (TCommandType)type.GetProperty(nameof(IMessage<TCommandType>.Id)).GetValue(Activator.CreateInstance(type), null);
                            if (_messageHandler.TryGetValue(cmdType, out Tuple<Type, IMessageDescriptor<TCommandType>> handler))
                            {
                                handler.Item2.RegisterCallback(callback);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public abstract void Start(string ip, int port);

        protected void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object.
            NetConnection connection = (NetConnection)ar.AsyncState;
            Socket socket = connection.Socket;

            int bytesRead;

            // Read data from the client socket. 
            try
            {
                bytesRead = socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    BinaryReader binReader = new BinaryReader(connection.Stream);

                    //read the CommandType
                    TCommandType commandType = (TCommandType)Enum.ToObject(typeof(TCommandType), binReader.ReadByte());

                    if (_messageHandler.TryGetValue(commandType, out Tuple<Type, IMessageDescriptor<TCommandType>> handler))
                    {
                        IMessage<TCommandType> msg = (IMessage<TCommandType>)handler.Item1.CreateInstance();
                        msg.OnDeserialize(binReader);
                        handler.Item2.Invoke(msg, connection);
                    }

                    // listen again:
                    socket.BeginReceive(connection.buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), connection);
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset || ((ex.SocketErrorCode != SocketError.Interrupted) && (ex.SocketErrorCode != SocketError.ConnectionAborted)))
                {
                    Console.WriteLine("receiver disconnected");
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                OnRemoteSocketDisconnected?.Invoke(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Console.WriteLine("remote client disconnected");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                OnRemoteSocketDisconnected?.Invoke(connection);
            }
        }

        private void Send(NetConnection connection, BinaryWriter binWriter)
        {
            Socket socket = connection.Socket;
            try
            {
                byte[] byteData = binWriter.ToArray();
                // Begin sending the data to the remote device.
                socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), connection);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("remote client disconnected");
                if (ex.SocketErrorCode == SocketError.ConnectionReset || ((ex.SocketErrorCode != SocketError.Interrupted) && (ex.SocketErrorCode != SocketError.ConnectionAborted)))
                {
                    Console.WriteLine("receiver disconnected");
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                OnRemoteSocketDisconnected?.Invoke(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                OnRemoteSocketDisconnected?.Invoke(connection);
            }
        }

        public void Send(NetConnection connection, IMessage<TCommandType> message)
        {
            try
            {
                BinaryWriter binWriter = new BinaryWriter(new MemoryStream());
                message.OnSerialize(binWriter);

                Send(connection, binWriter);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}{Environment.NewLine}{e.StackTrace}");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                NetConnection connection = (NetConnection)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = connection.Socket.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}
