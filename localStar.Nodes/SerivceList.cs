using System;
using System.Collections.Generic;

namespace localStar.Nodes
{
    [Serializable]
    public class ServiceList
    {
        public string Name { get; private set; } = null;
        public int Length { get => list.Count; }
        Queue<Service> queue = new Queue<Service>();
        List<Service> list = new List<Service>();

        public Service getLowestDelayOne()
        {
            Service service = queue.Dequeue();  // 못해도 null은 안내보넴.
            queue.Enqueue(service);
            int delay = service.delay;

            foreach (var s in list)
            {
                if (delay > s.delay)
                {
                    delay = s.delay;
                    service = s;
                }
            }
            return service;
        }
        public Service getOne()
        {
            var s = queue.Dequeue();
            queue.Enqueue(s);
            return s;
        }
        public bool addService(Service service)
        {
            if (Name == null) Name = service.name;
            if (Name != service.name) return false;
            if (list.Contains(service)) return false;

            list.Add(service);
            queue.Enqueue(service);
            return true;
        }
        public bool removeService(Service service)
        {
            if (Name != service.name) return false;
            if (list.Remove(service))
            {
                while (queue.Count > 0)
                {
                    var s = queue.Dequeue();
                    if (s.Equals(service)) break;
                    queue.Enqueue(s);
                }
                return true;
            }
            else return false;
        }
    }
}