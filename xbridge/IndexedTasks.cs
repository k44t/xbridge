using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace xbridge
{
    public class IndexedTasks<Result>
    {
        public class Info{
            public int ID;
            public Task<Result> Task;
        }

        Dictionary<int, TaskCompletionSource<Result>> requests = new Dictionary<int, TaskCompletionSource<Result>>();
        public IndexedTasks(int max)
        {
            this.Max = max;
        }
        public IndexedTasks()
        {
            this.Max = int.MaxValue;
        }

        public Info Create()
        {
            TaskCompletionSource<Result> src;
            int id;
            lock (requests)
            {
                id = r.Next() % Max;
                while (requests.ContainsKey(id))
                    id = r.Next() % Max;
                requests[id] = src = new TaskCompletionSource<Result>();

            }
            return new Info()
            {
                ID = id,
                Task = src.Task
            };
        }

        public Info Create(int id)
        {
            TaskCompletionSource<Result> src;
            lock (requests)
            {
                if (requests.ContainsKey(id))
                    throw new Exception("duplicate key");
                requests[id] = src = new TaskCompletionSource<Result>();

            }
            return new Info()
            {
                ID = id,
                Task = src.Task
            };
        }

        public bool hasID(int id)
        {
            return requests.ContainsKey(id);
        }

        public void Finish(int id, Result value)
        {
            TaskCompletionSource<Result> tcs;
            lock (requests)
            {
                tcs = requests[id];
                requests.Remove(id);
            }
            tcs.SetResult(value);
        }


        Random r = new Random();
        private int Max;
    }
}
