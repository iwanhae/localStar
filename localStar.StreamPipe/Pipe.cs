using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;

namespace localStar.StreamPipe
{
    public static class Pipe
    {
        private static Queue<PipeLine> Pipes = new Queue<PipeLine>();
        private static SortedSet<Stream> ReadingStreams = new SortedSet<Stream>();
        private static Thread loop = new Thread(pipeLoop);

        public static int Length { get => ReadingStreams.Count; }

        private static void pipeLoop()
        {
            int pointer = 0;
            bool canISleep = true;
            while (Pipes.Count != 0)
            {
                PipeLine pipe = Pipes.Dequeue();
                try
                {
                    if (ReadyToRead(pipe.from) && pipe.to.CanWrite)
                    {
                        canISleep = false;
                        // Message 의 최대 크기는 data (ushort.Max) + header (10)
                        int length = pipe.from.Length > ushort.MaxValue + 10 ? ushort.MaxValue + 10 : (int)pipe.from.Length;
                        // 단일 메세지 스트림은 한번에 전송됨.
                        pipe.from.CopyTo(pipe.to, length);
                    }
                    if ((!pipe.from.CanWrite) && (pipe.from.Length == pipe.from.Position)) removePipe(pipe);    // 더 쓸수 없는데 다 읽었으면.
                    else if (!(pipe.from.CanRead && pipe.to.CanWrite)) removePipe(pipe);   // 못읽거나 못쓰게 되면.
                    else Pipes.Enqueue(pipe);
                }
                catch
                {
                    removePipe(pipe);
                }

                pointer++;
                if (pointer >= Pipes.Count)
                {
                    pointer = 0;
                    if (canISleep) Thread.Sleep(100);
                    canISleep = true;
                }
            }
            loop = new Thread(pipeLoop);
        }
        /// <summary>
        /// 두 파이프를 연결해줌.
        /// 주의사항: canWrite한 Stream은 Dispose 해줘야 등록 해제됨.
        /// 주의사항: From에다가 NetworkStream넣으면 안됨.
        /// 읽기만 가능하면 다 읽은 후 알아서 해제됨.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void Connect(Stream from, Stream to)
        {
            addPipe(new PipeLine(from, to));
            if (loop.ThreadState == ThreadState.Unstarted) loop.Start();
        }
        private static void addPipe(PipeLine pipe)
        {
            if (ReadingStreams.TryGetValue(pipe.from, out _)) throw new StreamBeingPipedException(pipe.from);
            if (!(pipe.from.CanRead)) throw new StreamIsNotUsableException(pipe.from);
            if (!(pipe.to.CanWrite)) throw new StreamIsNotUsableException(pipe.to);


            Pipes.Enqueue(pipe);
            ReadingStreams.Add(pipe.from);
        }
        private static void removePipe(PipeLine pipe)
        {
            ReadingStreams.Remove(pipe.from);
        }
        private static bool ReadyToRead(Stream stream)
        {
            if (stream.CanSeek && stream.Position == stream.Length) return false;
            // MemoryStream, FileStream의 경우 탐색가능한데 다 읽었으면, 읽을게 없다고 판단.
            if (stream.Length == 0) return false;
            // NetworkStream의 경우 남은 데이터가 없으면 다 읽음
            return true;
        }
    }

    struct PipeLine
    {
        public Stream from;
        public Stream to;
        public PipeLine(Stream from, Stream to) { this.from = from; this.to = to; }
    }
    class StreamBeingPipedException : Exception
    {
        Stream stream;
        public StreamBeingPipedException(Stream stream) => this.stream = stream;

        public override string ToString()
        {
            return "다음 객체는 이미 사용중입니다. : " + stream.ToString();
        }
    }
    class StreamIsNotUsableException : Exception
    {
        Stream stream;
        public StreamIsNotUsableException(Stream stream) => this.stream = stream;

        public override string ToString()
        {
            return "다음 객체는 사용할 수 없습니다. : " + stream.ToString();
        }
    }
}