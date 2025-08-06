package net.jradius.client;

import java.io.IOException;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import org.ehcache.Cache;
import org.ehcache.CacheManager;

public class RadiusMultiClient extends RadiusClient 
{
	CacheManager cacheManager;
	String requestCacheName;
	Cache<java.io.Serializable, Object> requestCache;
	
	public RadiusMultiClient() throws IOException 
	{
		super();
	}

	public RadiusMultiClient(DatagramSocket socket, InetAddress address, String secret, int authPort, int acctPort, int timeout) throws SocketException 
	{
		super(socket, address, secret, authPort, acctPort, timeout);
	}

	public RadiusMultiClient(DatagramSocket socket, InetAddress address, String secret) 
	{
		super(socket, address, secret);
	}

	public RadiusMultiClient(InetAddress address, String secret, int authPort, int acctPort, int timeout) throws IOException 
	{
		super(address, secret, authPort, acctPort, timeout);
	}

	public RadiusMultiClient(InetAddress address, String secret) throws IOException 
	{
		super(address, secret);
	}

	public RadiusMultiClient(RadiusClientTransport transport) 
	{
		super(transport);
	}
}
