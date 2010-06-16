﻿/*
 * Copyright (C) Mark Alan Jones 2010
 * This code is published under the Microsoft Public License (Ms-Pl)
 * A copy of the Ms-Pl license is included with the source and 
 * binary distributions or online at
 * http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace OpenSoundControl
{
    public class OscUdpPacketIODevice : IDisposable, IOscPacketIODevice
    {
        private UdpClient udp;
        private bool disposed;

        /// <summary>
        /// Creates a UDP packet IO device using the given local port number.
        /// </summary>
        /// <param name="localPort">Creates a UDP packet IO device using the given local IPEndPoint.</param>
        public OscUdpPacketIODevice(int localPort)
        {
            udp = new UdpClient(localPort);
            udp.EnableBroadcast = true;
            BeginReceive();
        }

        public OscUdpPacketIODevice(IPEndPoint localEP)
        {
            udp = new UdpClient(localEP);
            BeginReceive();
        }

        ~OscUdpPacketIODevice()
        {
            Dispose(false);
        }

        /// <summary>
        /// Raised when a send operation completes.
        /// </summary>
        public event EventHandler<OscPacketIOEventArgs> SendCompleted;

        /// <summary>
        /// Raised when a packet is received.
        /// </summary>
        public event EventHandler<OscPacketIOEventArgs> ReceiveCompleted;

        /// <summary>
        /// Sends an OSC message to the given UDP address.
        /// </summary>
        public void Send(
            OscMessage message,
            OscPacketIODeviceAddress deviceAddress
            )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a OSC bundle to the given UDP address.
        /// </summary>
        public void Send(
            OscBundle bundle,
            OscPacketIODeviceAddress deviceAddress
            )
        {
            throw new NotImplementedException();
        }

        private void OnSend(IAsyncResult ar)
        {
            throw new System.NotImplementedException();
        }

        private void BeginReceive()
        {
            udp.BeginReceive(new AsyncCallback(OnReceive), null);
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                // get the datagram
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] buf = udp.EndReceive(ar, ref remoteEP);

                // create an device address for the remote end point
                OscPacketIODeviceAddress deviceAddress = new OscPacketIODeviceAddress(OscPacketIOAddressType.Udp, remoteEP);

                OscBundle bundle;
                OscMessage message;
                OscPacketIOEventArgs eventArgs = null;

                // try parsing a bundle                
                if (OscPacket.TryParseBundle(buf, out bundle))
                {
                    eventArgs = new OscPacketIOEventArgs(bundle, deviceAddress);
                }
                //try parsing a message                    
                else if (OscPacket.TryParseMessage(buf, out message))
                {
                    eventArgs = new OscPacketIOEventArgs(message, deviceAddress);
                }

                // raise event
                if (eventArgs != null && ReceiveCompleted != null)
                    ReceiveCompleted(this, eventArgs);
            }
            finally
            {
                BeginReceive();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    udp.Close();
                }
            }

            disposed = true;
        }
    }
}