﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    class Compressor : GZip
    {
        public Compressor(string input, string output)  : base(input, output)
        {

        }

        public override void Launch()
        {
            Console.WriteLine("Compressing...\n");

            Thread _reader = new Thread(new ThreadStart(Read));
            _reader.Start();
            Thread[] _compress = new Thread[_threads];

            for (int i = 0; i < _threads; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);                
                _compress[i] = new Thread(new ParameterizedThreadStart(Compress));
                _compress[i].Start(i);
            }

            Thread _writer = new Thread(new ThreadStart(Write));
            _writer.Start();

            WaitHandle.WaitAll(doneEvents);                
            {
                Console.WriteLine("\nCompressing has been succesfully finished");
                _success = true;
            }
            
        }

        private void Read()
        {
            try
            {

                using (FileStream _fileToBeCompressed = new FileStream(sourceFile, FileMode.Open))
                {

                    int bytesRead;                    
                    byte[] lastBuffer;
                    blocksCountTotal = (int)(_fileToBeCompressed.Length / (long)blockSize);
                    if ((int)(_fileToBeCompressed.Length % (long)blockSize) > 0)
                        blocksCountTotal++;

                    while (_fileToBeCompressed.Position < _fileToBeCompressed.Length)
                    {
                        if (_fileToBeCompressed.Length - _fileToBeCompressed.Position <= blockSize)
                        {
                            bytesRead = (int)(_fileToBeCompressed.Length - _fileToBeCompressed.Position);
                        }

                        else
                        {
                            bytesRead = blockSize;
                        }

                        lastBuffer = new byte[bytesRead];
                        _fileToBeCompressed.Read(lastBuffer, 0, bytesRead);
                        
                        _queueReader.EnqueueForCompressing(lastBuffer);
                        ConsoleProgress.ProgressBar(_fileToBeCompressed.Position, _fileToBeCompressed.Length);
                    }

                    _queueReader.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
            }

        }

        private void Compress(object i)
        {
            try
            {
                while (true)
                {
                    ByteBlock _block = _queueReader.Dequeue();

                    if (_block == null)
                        return;

                    using (MemoryStream _memoryStream = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                        {

                            cs.Write(_block.Buffer, 0, _block.Buffer.Length);
                        }


                        byte[] compressedData = _memoryStream.ToArray();
                        ByteBlock _out = new ByteBlock(_block.ID, compressedData);
                        _queueWriter.EnqueueForWriting(_out);
                    }
                    ManualResetEvent doneEvent = doneEvents[(int)i];
                    doneEvent.Set();
                    //doneEvents[(int)i].Set();
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread number {0}. \n Error description: {1}", i, ex.Message);                
            }

}

        private void Write()
        {
            try
            {
                using (FileStream _fileCompressed = new FileStream(destinationFile + ".zip", FileMode.Create))
                {
                    while (blocksCountTotal>blocksCountWrite)
                    {
                        
                        ByteBlock _block = _queueWriter.Dequeue();

                        if (_block == null)
                            return;

                        BitConverter.GetBytes(_block.Buffer.Length).CopyTo(_block.Buffer, 4);
                        _fileCompressed.Write(_block.Buffer, 0, _block.Buffer.Length);
                        blocksCountWrite++;

                    }
                    _fileCompressed.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
            }

        }
    }
}