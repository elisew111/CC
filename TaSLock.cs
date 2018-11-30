using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public class TaSLock
{
    int l = 0;

    public void Lock()
    {
        while (Interlocked.CompareExchange(ref l, 1, 0) != 0) { }
    }

    public void Unlock()
    {
        Interlocked.Exchange(ref l, 0);
    }

}
