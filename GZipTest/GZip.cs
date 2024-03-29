﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
   public abstract class GZip
    {        
        protected bool _success = false;
        protected bool _finRead = false;
        protected string sourceFile, destinationFile;
        protected static int _threads = Environment.ProcessorCount;

        protected int blockSize = 10*1024*1024; 
        protected int blocksCountTotal = 0;
        protected int blocksCountWrite = 0;
        protected QueueManager _queueReader = new QueueManager();
        protected QueueManager _queueWriter = new QueueManager();
        protected ManualResetEvent[] doneEvents = new ManualResetEvent[_threads];        
        
        public GZip()
        {

        }
        public GZip(string input, string output)
        {
            this.sourceFile = input;
            this.destinationFile = output;
        }

        public int CallBackResult()
        {
            if (_success)
                return 0;
            return 1;
        }

        public void Cancel()
        {
            
        }

        public abstract void Launch();
    }
}
