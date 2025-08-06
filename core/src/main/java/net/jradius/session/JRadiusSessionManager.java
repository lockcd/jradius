/**
 * JRadius - A RADIUS Server Java Adapter
 * Copyright (C) 2004-2006 PicoPoint, B.V.
 * Copyright (c) 2007 David Bird <david@coova.com>
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2.1 of the License, or (at
 * your option) any later version.
 *
 * This library is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this library; if not, write to the Free Software Foundation,
 * Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 *
 */
package net.jradius.session;

import java.io.Serializable;
import java.net.URL;
import java.time.Duration;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import net.jradius.exception.RadiusException;
import net.jradius.log.JRadiusLogEntry;
import net.jradius.log.RadiusLog;
import net.jradius.server.EventDispatcher;
import net.jradius.server.JRadiusEvent;
import net.jradius.server.JRadiusRequest;
import net.jradius.server.JRadiusServer;
import net.jradius.server.event.SessionExpiredEvent;
import org.ehcache.Cache;
import org.ehcache.CacheManager;
import org.ehcache.Status;
import org.ehcache.config.CacheConfiguration;
import org.ehcache.config.Configuration;
import org.ehcache.config.builders.CacheConfigurationBuilder;
import org.ehcache.config.builders.CacheManagerBuilder;
import org.ehcache.config.builders.ExpiryPolicyBuilder;
import org.ehcache.config.builders.ResourcePoolsBuilder;
import org.ehcache.event.CacheEvent;
import org.ehcache.event.CacheEventListener;
import org.ehcache.event.EventType;
import org.ehcache.xml.XmlConfiguration;
import org.springframework.beans.factory.InitializingBean;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;

public class JRadiusSessionManager
    implements InitializingBean, ApplicationContextAware, CacheEventListener<Object, Object> {
    private static JRadiusSessionManager defaultManager;

    private static Map<String, JRadiusSessionManager> managers = new HashMap<String, JRadiusSessionManager>();

    private Map<String, SessionKeyProvider> providers = new HashMap<String, SessionKeyProvider>();
    private Map<String, SessionFactory> factories = new HashMap<String, SessionFactory>();

    private ApplicationContext applicationContext;

    private int minInterimInterval  = 300;
    private int maxInactiveInterval = 2100;

    private String cacheName = "jradius-session";
    private String logCacheName = "jradius-log";
    

    private CacheManager cacheManager;
    private Cache<Serializable, JRadiusSession> sessionCache;
    private Cache<Serializable, JRadiusLogEntry> logCache;

    private EventDispatcher eventDispatcher;

    /**
     * There is a single JRadiusSessionManager available that
     * is accessible through this method. Using the default
     * application-wide manager is sufficient for most uses.
     * For specific needs, it is possible to create a new
     * JRadiusSessionManager object.
     * @return the default JRadiusSessionManager instance
     */
    public static JRadiusSessionManager getManager(Object name)
    {
        JRadiusSessionManager manager = null;
     
        if (name != null)
        {
            manager = managers.get(name.toString());
        }

        if (manager == null) 
        {
            if (defaultManager == null)
            {
                defaultManager = new JRadiusSessionManager();
                try
                {
                    defaultManager.afterPropertiesSet();
                }
                catch (Exception e)
                {
                    RadiusLog.error("Error in JRadius Session Manager", e);
                }
            }
         
            manager = defaultManager;
        }
        
        return manager;
    }

    public static JRadiusSessionManager setManager(String name, JRadiusSessionManager manager)
    {
        if (name != null)
        {
            managers.put(name, manager);
        }
        else
        {
            defaultManager = manager;
        }

        return manager;
    }

    public static void shutdownManagers() 
    {
        if (defaultManager != null)
        {
            defaultManager.shutdown();
        }
        
        for (Iterator<?> i = managers.values().iterator(); i.hasNext();)
        {
            JRadiusSessionManager manager = (JRadiusSessionManager)i.next();
            manager.shutdown();
        }
    }
    
    /**
     * Creates a new JRadiusSessionManager instance. This
     * sets the key provider and session factory to the
     * DefaultSessionKeyProvider and RadiusSessionFactory,
     * respectively
     */
    public JRadiusSessionManager()
    {
        initialize();
    }
    
    private void initialize()
    {
        try
        {
            // If we can find the extended JRadius classes, configure
            // the default RadiusSessionKeyProvider and RadiusSessionFactory
            Class<?> c;
            c = Class.forName("net.jradius.session.RadiusSessionKeyProvider");
            providers.put(null, (SessionKeyProvider) c.newInstance());
            c = Class.forName("net.jradius.session.RadiusSessionFactory");
            factories.put(null, (SessionFactory) c.newInstance());
        }
        catch (Exception e)
        {
            RadiusLog.error("Could not find extended JRadius classes - not running JRadiusSessionManager", e);
            throw new RuntimeException(e);
        }
    }

    public void shutdown()
    {
	if (cacheManager != null && cacheManager.getStatus() == Status.AVAILABLE)
        {
            cacheManager.close();
        }
    }
    
    public void afterPropertiesSet() throws Exception
    {
	if (cacheManager == null)
        {
            URL url = JRadiusServer.class.getResource("/ehcache.xml");
            if (url != null) {
                RadiusLog.info("Loading EHCache configuration from " + url);
                Configuration xmlConfig = new XmlConfiguration(url);
                cacheManager = CacheManagerBuilder.newCacheManager(xmlConfig);
            } else {
                RadiusLog.warn("ehcache.xml not found, using programmatic configuration.");
                cacheManager = CacheManagerBuilder.newCacheManagerBuilder().build();
            }
            cacheManager.init();
        }

        sessionCache = getOrCreateCache(cacheName, Serializable.class, JRadiusSession.class, 1000);
        logCache = getOrCreateCache(logCacheName, Serializable.class, JRadiusLogEntry.class, 100);

        sessionCache.getRuntimeConfiguration().registerCacheEventListener(this, org.ehcache.event.EventOrdering.UNORDERED, org.ehcache.event.EventFiring.ASYNCHRONOUS, EventType.EXPIRED);
        logCache.getRuntimeConfiguration().registerCacheEventListener(this, org.ehcache.event.EventOrdering.UNORDERED, org.ehcache.event.EventFiring.ASYNCHRONOUS, EventType.EXPIRED);
    }

    private <K extends Serializable, V extends Serializable> Cache<K, V> getOrCreateCache(String alias, Class<K> keyType, Class<V> valueType, long heapSize) {
        Cache<K, V> cache = cacheManager.getCache(alias, keyType, valueType);
        if (cache == null) {
            RadiusLog.info("Cache '" + alias + "' not found in ehcache.xml, creating programmatically.");

            CacheConfiguration<K, V> cacheConfig = CacheConfigurationBuilder.newCacheConfigurationBuilder(
                keyType, valueType, ResourcePoolsBuilder.heap(heapSize))
                .withExpiry(ExpiryPolicyBuilder.timeToLiveExpiration(Duration.ofSeconds(maxInactiveInterval)))
                .build();

            cache = cacheManager.createCache(alias, cacheConfig);
        }
        return cache;
    }


    /**
     * Sets the key provider for this session manager. The
     * key provider generates a key to store a session with.
     * The key is generated based on a JRadiusRequest that is
     * passed to the key provider's getSessionKey method.
     * Keys are used to retrieve stored sessions from the session
     * manager.
     * @param name The name of the SessionKeyProvider (null for default)
     * @param provider The SessionKeyProvider
     * @see SessionKeyProvider
     */
    public void setSessionKeyProvider(String name, SessionKeyProvider provider)
    {
        providers.put(name, provider);
    }
    
    /**
     * Sets the session factory for this session manager. The
     * session factory generates a new session object, possibly
     * initialized based on values of a JRadiusRequest.
     * @param name The name of the SessionFactory (null for default)
     * @param factory a SessionFactory
     * @see SessionFactory
     */
    public void setSessionFactory(String name, SessionFactory factory)
    {
        factories.put(name, factory);
    }
    
    /**
     * returns the session manager's key provider
     * @param name The name of the SessionKeyProvider (null for default)
     * @return the session manager's key provider
     */
    public SessionKeyProvider getSessionKeyProvider(Object name)
    {
        SessionKeyProvider provider = providers.get(name);
        if (provider == null && name != null) provider = providers.get(null);
        return provider;
    }
    
    /**
     * returns the session manager's session factory
     * @param name The name of the SessionFactory (null for default)
     * @return the session manager's session factory
     */
    public SessionFactory getSessionFactory(Object name)
    {
        SessionFactory factory = factories.get(name);
        if (factory == null && name != null) factory = factories.get(null);
        return factory;
    }
    
    /**
     * Returns a session object. First, a key is generated by
     * the session manager's key provider, based on the JRadiusRequest.
     * If there is a stored session based on the key, this session is
     * returned, otherwise a new session created by the session factory
     * is returned
     * @param request a JRadiusRequest used to retrieve or generate a session with
     * @return Returns a RadiusSession
     * @throws RadiusException
     */
    public JRadiusSession getSession(JRadiusRequest request) throws RadiusException
    {
        SessionKeyProvider skp = getSessionKeyProvider(request.getSender());
        Serializable key = skp.getAppSessionKey(request);
        JRadiusSession session = null;
        Serializable nkey = null;
        
        if (key != null) 
        {
            RadiusLog.debug("** Looking for session: " + key);
            
            session = getSession(request, key);
            if (session == null)
            {
                RadiusLog.error("Broken JRadius-Session-Id implementation for session: " + key);
                key = null;
            }
        }

        if (key == null)
        {
            key = skp.getClassKey(request);
            
            if (key != null) 
            {
                RadiusLog.debug("** Looking for session: " + key);
                
                session = getSession(request, key);
                if (session == null)
                {
                    RadiusLog.error("Broken Class implementation for session: " + key);
                    key = null;
                }
                else
                {
                    if (session.getJRadiusKey() != null && !session.getJRadiusKey().equals(session.getSessionKey()))
                    {
                        rehashSession(session, session.getJRadiusKey(), key);
                    }
                }
            }
        }

        if (key == null)
        {
            Serializable keys = skp.getRequestSessionKey(request);
        
            if (keys == null)
            {
                return null;
            }
        
            if (keys instanceof Serializable[])
            {
                key = ((Serializable[])(keys))[0];
                nkey = ((Serializable[])(keys))[1];
                RadiusLog.debug("Rehashing session with key " + key + " under new key " + nkey);
            }
            else
            {
                key = keys;
            }
            
            RadiusLog.debug("** Looking for session: " + key);
            session = getSession(request, key);

            if (session != null && nkey != null && !nkey.equals(key))
            {
                rehashSession(session, key, nkey);
            }
        }        

        if (session == null) 
        {
            session = newSession(request, nkey == null ? key : nkey);
        }
        
        session.setTimeStamp(System.currentTimeMillis());
        //session.setLastRadiusRequest(request);
        
        return session;
    }

    public void rehashSession(JRadiusSession session, Serializable okey, Serializable nkey) throws RadiusException
    {
        remove(okey);
        session.setJRadiusKey((String)nkey);
        put(session.getJRadiusKey(), session);
    }

    public JRadiusSession newSession(JRadiusRequest request, Object key) throws RadiusException
    {
        JRadiusSession session = (JRadiusSession) getSessionFactory(request.getSender()).newSession(request);
        session.setJRadiusKey((String)key);
        put((Serializable)session.getJRadiusKey(), session);
        put((Serializable)session.getSessionKey(), session);
        return session;
    }

    public JRadiusSession getSession(JRadiusRequest request, Serializable key) throws RadiusException
    {
        JRadiusSession session = sessionCache.get(key);

        if (session == null && request != null)
        {
            SessionFactory sf = getSessionFactory(request.getSender());
            session = sf.getSession(request, key);
            if (session != null)
            {
                put(session.getJRadiusKey(), session);
                put(session.getSessionKey(), session);
            }
        }
        
        if (session == null) return null;
        
        return session;
    }

    public void lock(JRadiusSession session)
    {
        session.lock();
        //RadiusLog.error("Appropriate session locking must be implemented");
    }

    public void unlock(JRadiusSession session, boolean save)
    {
        session.unlock();
        //RadiusLog.error("Appropriate session locking must be implemented");
    }

    public JRadiusLogEntry newLogEntry(JRadiusEvent event, JRadiusSession session, String packetId) 
    {
        Object sender = null;
        
        if (event != null) 
            sender = event.getSender();
        
//        else if (session != null && session.getLastRadiusRequest() != null) 
//            sender = session.getLastRadiusRequest().getSender();
        
        return getSessionFactory(sender).newSessionLogEntry(event, session, packetId);
    }
    
    public void removeSession(JRadiusSession session) 
    {
        if (session != null)
        {
            remove(session.getJRadiusKey());
            remove(session.getSessionKey());
        }
    }
    
    private void remove(Serializable key)
    {
    	RadiusLog.debug("Removing session key: " + key);
        sessionCache.remove(key);
    }

    private void put(Serializable key, JRadiusSession value)
    {
        RadiusLog.debug("Adding session key: " + key);
        sessionCache.put(key, value);
    }

    public int getMaxInactiveInterval()
    {
        return maxInactiveInterval;
    }

    public void setMaxInactiveInterval(int maxInactiveInterval)
    {
        this.maxInactiveInterval = maxInactiveInterval;
    }

    public int getMinInterimInterval()
    {
        return minInterimInterval;
    }

    public void setMinInterimInterval(int minInterimInterval)
    {
        this.minInterimInterval = minInterimInterval;
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

    @Override
    public void onEvent(CacheEvent<?, ?> event) {
        if (event.getType() == EventType.EXPIRED) {
            Object value = event.getOldValue();
            if (value != null && value instanceof JRadiusSession)
            {
                JRadiusSession session = (JRadiusSession) value;
                RadiusLog.debug("Expired session: " + session.getSessionKey());
                if (eventDispatcher != null)
                {
                    SessionExpiredEvent evt = new SessionExpiredEvent(session);
                    evt.setApplicationContext(applicationContext);
                    eventDispatcher.post(evt);
                }
            }
        }
    }

    public Cache<Serializable, JRadiusSession> getSessionCache()
    {
        return sessionCache;
    }

    public void setSessionCache(Cache<Serializable, JRadiusSession> sessionCache)
    {
        this.sessionCache = sessionCache;
    }

    public void setEventDispatcher(EventDispatcher eventDispatcher)
    {
		this.eventDispatcher = eventDispatcher;
	}

	public ApplicationContext getApplicationContext()
    {
        return applicationContext;
    }

    public void setApplicationContext(ApplicationContext applicationContext)
    {
        this.applicationContext = applicationContext;
    }
}
