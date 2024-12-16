using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace xbridge
{
    public class HashedTasks<Key, Result>
    {
        public class Info{
            public Key ID;
            public Task<Result> Task;
        }

        Dictionary<Key, TaskCompletionSource<Result>> requests = new Dictionary<Key, TaskCompletionSource<Result>>();
        public HashedTasks()
        {
        }


        public Info Create(Key id)
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



        public void Finish(Key id, Result value)
        {
            TaskCompletionSource<Result> tcs;
            lock (requests)
            {
                tcs = requests[id];
                requests.Remove(id);
            }
            tcs.SetResult(value);
        }



    }
}
