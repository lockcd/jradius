/**
 * JRadius - A RADIUS Server Java Adapter
 * Copyright (c) 2006-2008 David Bird <david@coova.com>
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
package net.jradius.handler.dhcp;

import java.io.File;
import java.io.FileWriter;
import java.io.PrintWriter;
import java.io.Serializable;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.Arrays;
import net.jradius.util.RadiusRandom;
import net.jradius.util.RadiusUtils;
import org.ehcache.Cache;
import org.ehcache.event.CacheEvent;
import org.ehcache.event.CacheEventListener;

public class AddressPoolImpl implements AddressPool, CacheEventListener<Object, Object> {
    protected String leaseFile = "/tmp/leases.dhcp";
    protected InetAddress network;
    protected InetAddress netmask;
    protected InetAddress router;
    protected InetAddress[] dns;
    protected byte next = RadiusRandom.getBytes(1)[0];
    protected int fudge = 10;
    protected int leaseTime;
    protected AddressPoolListener listener;
    protected Cache<Serializable, Serializable> leases;
    
    class MACKey implements Serializable
    {
        private static final long serialVersionUID = 0L;
        byte[] mac;
     
        public MACKey(byte[] b) { mac = b; }
        public byte[] getMAC() { return mac; }
        
        public boolean equals(Object o)
        {
            if (!(o instanceof MACKey))
                return false;
            
            if (this == o)
                return true;
            
            byte[] omac = ((MACKey) o).getMAC();
            
            if (mac.length != omac.length)
                return false;

            for (int i = 0; i < mac.length; i++)
            {
                if (mac[i] != omac[i])
                    return false;
            }
            
            return true;
        }
        
        public int hashCode()
        {
            return Arrays.hashCode(mac);
        }
    }

    public AddressPoolImpl()
    {
    }
    
    public AddressPoolImpl(InetAddress network, InetAddress netmask, InetAddress router, int leaseTime)
    {
        this.network = network;
        this.netmask = netmask;
        this.router = router;
        this.leaseTime = leaseTime;
    }
    
    public boolean contains(InetAddress ip) 
    {
        if (getNetwork() == null || getNetmask() == null)  
        	throw new RuntimeException("network/netmask requierd");

        byte[] networkBytes = getNetwork().getAddress();
        byte[] netmaskBytes = getNetmask().getAddress();
        byte[] ipBytes = ip.getAddress();
        
        if (networkBytes.length != netmaskBytes.length || netmaskBytes.length != ipBytes.length)
        {
            return false;
        }
        
        for (int i=0; i < netmaskBytes.length; i++) 
        {
            int mask = netmaskBytes[i] & 0xff;
            if ((networkBytes[i] & mask) != (ipBytes[i] & mask)) 
            {
                return false;
            }
        }
        
        return true;
    }

    public InetAddress nextIP() throws UnknownHostException
    {
        if (getNetwork() == null || getNetmask() == null)  
        	throw new RuntimeException("network/netmask requierd");

        InetAddress nextAddress = null;

        do
        {
            byte b[] = getNetwork().getAddress();
            b[b.length-1] = next++;
            nextAddress = InetAddress.getByAddress(b);
        }
        while(leases.get(nextAddress) != null ||
             (router != null && nextAddress.equals(router)));
        
        return nextAddress;
    }
    
    private static InetAddress anyIPAddress;

    static {
        try { anyIPAddress = InetAddress.getByAddress(new byte[] { 0, 0, 0, 0 }); }
        catch (Exception e) { }
    }
    
    public InetAddress getIP(byte[] hwa, InetAddress requested, boolean forceNew) throws UnknownHostException
    {
        if (leases == null) throw new RuntimeException("leases not set");

        MACKey hwKey = new MACKey(hwa);
        InetAddress leasedIP = (InetAddress) leases.get(hwKey);
        MACKey leasedHW = (requested != null) ? (MACKey) leases.get(requested) : null;
        
        if (anyIPAddress.equals(requested))
            requested = null;
        
        if (leasedIP == null)
        {
            /**
             *   Client does not yet have a leased IP address
             */
            
            if (requested != null) 
            {
                /**
                 *   Client is requesting an IP
                 */

                if (!contains(requested))
                {
                    /**
                     *  IP address not in our range!
                     */

                    return null;
                }

                if (leasedHW != null && hwKey.equals(leasedHW))
                {
                    /**
                     *  We owned the lease, so let's go ahead and update the IP
                     */
                    
                    leases.remove(requested);
                }
                else if (leasedHW != null)
                {
                    /**
                     *  IP address is already leased
                     */

                    return null;
                }

                leasedIP = requested;
            }
            else
            {
                leasedIP = nextIP();
            }
        }
        else
        {
            /**
             *  Client already has a leased IP
             */

            if (forceNew)
            {
                if (leasedHW != null && hwKey.equals(leasedHW))
                {
                    /**
                     *  We owned the lease, so let's go ahead and update the IP
                     */

                    leases.remove(requested);
                }

                leasedIP = nextIP();
            }
            else if (requested != null) 
            {
                if (!requested.equals(leasedIP))
                {
                    /**
                     *  Requested IP address does not match leased IP
                     */

                    if (leasedHW != null && hwKey.equals(leasedHW))
                    {
                        /**
                         *  We owned the lease, so let's go ahead and update the IP
                         */

                        leases.remove(requested);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        leases.put(hwKey, leasedIP);
        leases.put(leasedIP, hwKey);
        
        writeLeaseFile();
        
        return leasedIP;
    }

    public void writeLeaseFile()
    {
        if (getLeaseFile() == null) return;
        try
        {
            File file = new File(getLeaseFile());
            PrintWriter writer = new PrintWriter(new FileWriter(file));
            
            for (Cache.Entry<Serializable, Serializable> entry : leases)
            {
                if (entry.getKey() instanceof MACKey)
                {
                    InetAddress inet = (InetAddress) entry.getValue();
                    MACKey macKey = (MACKey) entry.getKey();

                    writer.print(inet.getHostAddress());
                    writer.print(" ");
                    writer.println(RadiusUtils.byteArrayToHexString(macKey.getMAC()));
                }
            }
            
            writer.close();
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
    
    @Override
    public void onEvent(CacheEvent<?, ?> event) {
        // Not used
    }

    public void setFudge(int fudge)
    {
        this.fudge = fudge;
    }

    public void setLeaseFile(String leaseFile)
    {
        this.leaseFile = leaseFile;
    }

    public void setLeases(Cache<Serializable, Serializable> leases)
    {
        this.leases = leases;
    }

    public void setLeaseTime(int leaseTime)
    {
        this.leaseTime = leaseTime;
    }

    public void setNetmask(InetAddress netmask)
    {
        this.netmask = netmask;
    }

    public void setNetwork(InetAddress network)
    {
        this.network = network;
    }

    public void setRouter(InetAddress router)
    {
        this.router = router;
    }

    public String getLeaseFile()
    {
        return leaseFile;
    }

    public Cache<Serializable, Serializable> getLeases()
    {
        return leases;
    }

    public int getLeaseTime()
    {
        return leaseTime;
    }

    public InetAddress getNetmask()
    {
        return netmask;
    }

    public InetAddress getNetwork()
    {
        return network;
    }

    public InetAddress getRouter()
    {
        return router;
    }

    public InetAddress[] getDns()
    {
        return dns;
    }

    public void setDns(InetAddress[] dns)
    {
        this.dns = dns;
    }

    public AddressPoolListener getListener()
    {
        return listener;
    }

    public void setListener(AddressPoolListener listener)
    {
        this.listener = listener;
    }
}
