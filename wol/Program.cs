using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
namespace wol {
    public class Program {
        static void Main( string[] args ) {
            if( args.Length == 0 ) {
                Console.WriteLine( @"Usage: wol MAC,MAC... BROADCAST" );
                Console.WriteLine( @"  eg: wol 9C-B6-54-0B-14-59,9C-B6-54-0B-14-60... [192.168.1.255]" );
            }
            else {
                foreach( var mac in args[ 0 ].Split( ',' ) )
                    SendMagicPacket( mac, args.Length > 1 ? args[ 1 ] : @"192.168.1.255" );
            }
        }
        static int Send( UdpClient client, byte[] dgram, IPAddress broadcast, int port ) {
            Console.WriteLine( $@"Broadcast magic packet to {broadcast} port {port}" );
            return client.Send( dgram, dgram.Length, new IPEndPoint( broadcast, port ) );
        }
        static void SendMagicPacket( string MAC = @"9C-B6-54-0B-14-59", string broadcast = @"255.255.255.0" ) {
            using( var udpClient = new UdpClient() {
                EnableBroadcast = true
            } )
                try {
                    Console.WriteLine( $@"Target {MAC}" );
                    var packet = Magic( MAC ).ToArray();
                    System.Diagnostics.Debug.Assert( packet.Length == 6 + 16 * 6, @"Invalid Packet Length" );
                    var ip = IPAddress.Parse( broadcast );
                    foreach( var port in new int[] { 7, 9 } )
                        Send( udpClient, packet, ip, port );
                }
                finally {
                    udpClient.Close();
                }
        }
        static IEnumerable<byte> Magic( string MAC = @"9C-B6-54-0B-14-59" ) {
            var mac = HexToByte( MAC ).ToArray();
            foreach( var i in Enumerable.Range( 1, 6 ) )
                yield return 0xff;
            foreach( var i in Enumerable.Range( 1, 16 ) )
                foreach( var c in mac )
                    yield return c;
        }
        static IEnumerable<uint> HexToDigit( string value ) {
            foreach( var c in value ) {
                if( char.IsDigit( c ) ) {
                    yield return ( (uint)c - (uint)'0' );
                    continue;
                }
                if( c >= 'A' && c <= 'F' ) {
                    yield return ( 10 + (uint)c - (uint)'A' );
                    continue;
                }
                if( c >= 'a' && c <= 'f' ) {
                    yield return ( 10 + (uint)c - (uint)'a' );
                    continue;
                }
            }
        }
        static IEnumerable<byte> HexToByte( string value ) {
            using( var enumerateDigits = HexToDigit( value ).GetEnumerator() )
                while( enumerateDigits.MoveNext() ) {
                    var h = enumerateDigits.Current;
                    enumerateDigits.MoveNext();
                    var l = enumerateDigits.Current;
                    yield return (byte)( h * 16 + l );
                }
        }
    }
}
