/**
 * JRadius - A Radius Server Java Adapter
 * Copyright (C) 2004-2006 PicoPoint, B.V.
 * Copyright (c) 2006 David Bird <david@coova.com>
 * 
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation; either version 2 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
 * for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 *
 */
package net.jradius.webservice;

import java.io.DataInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.SocketException;
import java.net.URI;
import java.net.URISyntaxException;
import java.nio.ByteBuffer;
import java.time.Duration;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.StringTokenizer;
import net.jradius.server.JRadiusEvent;
import net.jradius.server.ListenerRequest;
import net.jradius.server.TCPListener;
import org.ehcache.Cache;
import org.ehcache.CacheManager;
import org.ehcache.config.CacheConfiguration;
import org.ehcache.config.builders.CacheConfigurationBuilder;
import org.ehcache.config.builders.CacheManagerBuilder;
import org.ehcache.config.builders.ExpiryPolicyBuilder;
import org.ehcache.config.builders.ResourcePoolsBuilder;
import org.ehcache.event.CacheEvent;
import org.ehcache.event.CacheEventListener;
import org.ehcache.event.EventType;
import org.springframework.beans.factory.InitializingBean;


/**
 * JRadius TCP/SSL Proxy Listen
 *
 * @author David Bird
 */
public class WebServiceListener extends TCPListener implements InitializingBean, CacheEventListener<String, WebServiceRequestObject>
{
    protected String cacheName = "ws-requests";
    protected Map<String, WebServiceRequestObject> requestMap;
    protected CacheManager cacheManager;
    protected Cache<String, WebServiceRequestObject> requestCache;
    protected Integer timeToLive;
    protected Integer idleTime;
    
    public JRadiusEvent parseRequest(ListenerRequest listenerRequest, ByteBuffer byteBuffer, InputStream inputStream) throws IOException, WebServiceException
    {
        DataInputStream reader = new DataInputStream(inputStream);
        WebServiceRequest request = new WebServiceRequest();
        
        String line = null;
        
        try
        {
	        line = reader.readLine();
        }
        catch (SocketException e)
        {
        	return null;
        }
        
        if (line == null) throw new WebServiceException("Invalid relay request");
        
        StringTokenizer tokens = new StringTokenizer(line);
        String method = tokens.nextToken();
        String uri = tokens.nextToken();
        String httpVersion = tokens.nextToken();
        
        if ("GET".equals(method)) request.setMethod(WebServiceRequest.GET);
        else if ("POST".equals(method)) request.setMethod(WebServiceRequest.POST);
        else if ("PUT".equals(method)) request.setMethod(WebServiceRequest.PUT);
        else throw new WebServiceException("Does not handle HTTP request method: " + method);
        
        request.setHttpVersion(httpVersion);
        
        try
        {
            request.setUri(new URI(uri));
        }
        catch (URISyntaxException e)
        {
            throw new WebServiceException(e.getMessage());
        }
        
        Map<String, String> headers = getHeaders(reader);
        request.setHeaderMap(headers);
        
        String clen = headers.get("content-length");
        if (clen != null)
        {
            request.setContent(getContent(reader, Integer.parseInt(clen)));
        }
        
        return request;
    }
    
    private Map<String, String> getHeaders(DataInputStream reader) throws IOException
    {
        LinkedHashMap<String, String> map = new LinkedHashMap<String, String>();
        String line;
        do
        {
            line = reader.readLine().trim();
            if (line != null && line.length() > 0)
            {
                String[] parts = line.split(":", 2);
                if (parts != null && parts.length == 2)
                {
                    map.put(parts[0].toLowerCase().trim(), parts[1].trim());
                }
                else break;
            }
            else break;
        }
        while (true);
        
        return map;
    }
    
    private byte[] getContent(DataInputStream reader, int clen) throws IOException
    {
        byte[] buf = new byte[clen];
        reader.readFully(buf);
        return buf;
    }
    
    public void remove(OTPProxyRequest request)
    {
        request.interrupt();
        if (requestMap != null)
            requestMap.remove(request.getOtpName());
        else
            requestCache.remove(request.getOtpName());
    }

    public void put(WebServiceRequestObject obj)
    {
        if (requestMap != null)
            requestMap.put(obj.getKey(), obj);
        else
            requestCache.put(obj.getKey(), obj);
    }
    
    public WebServiceRequestObject get(String username)
    {
        if (requestMap != null)
        {
            return requestMap.get(username);
        }
        return requestCache.get(username);
    }

    private void deleteObject(WebServiceRequestObject o)
    {
	if (o == null) return;
        o.delete();
    }
    
    public Object clone() throws CloneNotSupportedException
    {
        return super.clone();
    }

    @Override
    public void onEvent(CacheEvent<? extends String, ? extends WebServiceRequestObject> event) {
        if (event.getType() == EventType.EXPIRED || event.getType() == EventType.REMOVED || event.getType() == EventType.EVICTED) {
            deleteObject(event.getOldValue());
        }
    }

    public void afterPropertiesSet() throws Exception
    {
        if (idleTime == null) idleTime = 120;
        if (timeToLive == null) timeToLive = 180;
        if (requestMap != null) return;
        
        if (requestCache == null) 
        {
            if (cacheManager == null) 
            {
            	throw new RuntimeException("cacheManager required");
            }

            requestCache = cacheManager.getCache(cacheName, String.class, WebServiceRequestObject.class);
        
            if (requestCache == null)
            {
                CacheConfiguration<String, WebServiceRequestObject> cacheConfig = CacheConfigurationBuilder.newCacheConfigurationBuilder(
                    String.class, WebServiceRequestObject.class, ResourcePoolsBuilder.heap(2000))
                    .withExpiry(ExpiryPolicyBuilder.timeToLiveExpiration(Duration.ofSeconds(timeToLive)))
                    .build();
                requestCache = cacheManager.createCache(cacheName, cacheConfig);
            }
        }

        requestCache.getRuntimeConfiguration().registerCacheEventListener(this, org.ehcache.event.EventOrdering.UNORDERED, org.ehcache.event.EventFiring.ASYNCHRONOUS, EventType.EVICTED, EventType.EXPIRED, EventType.REMOVED);
    }

    public CacheManager getCacheManager()
    {
        return cacheManager;
    }

    public void setCacheManager(CacheManager cacheManager)
    {
        this.cacheManager = cacheManager;
    }
    
    public String getCacheName()
    {
        return cacheName;
    }
    
    public void setCacheName(String cacheName)
    {
        this.cacheName = cacheName;
    }
    
    public Integer getIdleTime()
    {
        return idleTime;
    }
    
    public void setIdleTime(Integer idleTime)
    {
        this.idleTime = idleTime;
    }
    
    public Cache<String, WebServiceRequestObject> getRequestCache()
    {
        return requestCache;
    }
    
    public void setRequestCache(Cache<String, WebServiceRequestObject> requestCache)
    {
        this.requestCache = requestCache;
    }
    
    public Integer getTimeToLive()
    {
        return timeToLive;
    }
    
    public void setTimeToLive(Integer timeToLive)
    {
        this.timeToLive = timeToLive;
    }

    public Map<String, WebServiceRequestObject> getRequestMap()
    {
        return requestMap;
    }

    public void setRequestMap(Map<String, WebServiceRequestObject> requestMap)
    {
        this.requestMap = requestMap;
    }
}
