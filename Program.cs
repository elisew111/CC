using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Security.Cryptography;

class Program
{
    static int l, b, e, m, p, u, teller, volgnummer;
    static string h;
    static bool gevonden = false;
    static TaSLock taSLock = new TaSLock();
    static readonly object obj = new object();
    static Thread[] threads;

    public static void Main(string[] args)
    {
        string[] input = Console.ReadLine().Split(' ');
        l = int.Parse(input[0]); //locktype: 0 -> zelfgebouwd TaS of TTaS-lock, 1 -> C# lock
        b = int.Parse(input[1]); //ondergrens zoekbereik
        e = int.Parse(input[2]); //bovengrens zoekbereik
        m = int.Parse(input[3]); //modulus m-proef
        p = int.Parse(input[4]); //aantal threads
        u = int.Parse(input[5]); //programmamodus: 0 -> telmodus, 1 -> lijstmodus, 2 -> zoekmodus
        h = input[6];         //hash
        
        teller = 0;
        volgnummer = 0;
        

        TaSLock taSLock = new TaSLock();

        threads = new Thread[p];

        for (int i = 0; i < p; i++)
        {
            threads[i] = new Thread(Telbereik);
        }

        for (int i = 0; i < p; i++)
        {
            threads[i].Start(i);
        }

        for (int i = 0; i < p; i++)
        {
            threads[i].Join();
        }

        if(u == 0) Console.WriteLine(teller);
        if(u == 2 && !gevonden) Console.WriteLine(-1);
        

        Console.ReadLine();

    }
    public static void Tel(int onder, int boven)
    {
        for (int i = onder; i < boven; i++)
        {
            if (mProef(i))
            {
                if(u == 2 && Vergelijk(i))
                {
                    gevonden = true;
                    Console.WriteLine(i);
                    for (int q = 0; q < p; i++)
                    {
                        threads[q].Join();
                    }
                }
                teller++;
                Interlocked.Increment(ref volgnummer);
                if(u == 1) Console.WriteLine(volgnummer + " " + i);
            }
        }
    }

    public static bool mProef(int x)
    {
        //int[] array = IntToIntArray(x);
        int getal = x;
        int totaal = 0;
        for (int i = 1;getal > 0 ; i++)
        {
            int rest = getal % 10;
            totaal += rest * i;
            getal = (getal - rest) / 10;
        }
        return totaal % m == 0;
    }

    public static int[] IntToIntArray(int x)
    {
        int length = x.ToString().Length;
        int[] array = new int[length];
        int index = length - 1;
        while (x > 0)
        {
            int rest = x % 10;
            array[index] = rest;
            index -= 1;
            x = (x - rest) / 10;
        }
        return array;

    }
    public static void Telbereik(object o)
    {
        int from = b + (int)((((int)o * ((long)e - b))) / p);
        int to = b + (int)(((((int)o + 1) * ((long)e - b))) / p);
        //Console.WriteLine("Thread " + o + " telt van " + from + " tot " + to
        //    + " (bereik-omvang: " + (to - from) + ").");

        if(l == 0)
        {
            taSLock.Lock();
            Tel(from, to);
            taSLock.Unlock();
        }
        else { lock (obj) { Tel(from, to); } }

    }

    public static bool Vergelijk(int x)
    {
        SHA1 sha = SHA1.Create();
        string hash = h;
        byte[] hashArray = sha.ComputeHash(Encoding.ASCII.GetBytes(x.ToString()));
        string newHash = "";
        for (int hashPos = 0; hashPos < hashArray.Length; hashPos++)
            newHash += hashArray[hashPos].ToString("x2");
        if(newHash == hash)
        {
            return true;
        }
        return false;
    }

}
